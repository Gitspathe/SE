using Microsoft.Xna.Framework.Graphics;
using SE.Rendering;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using Vector2 = System.Numerics.Vector2;
using Vector3 = System.Numerics.Vector3;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace SE.NeoRenderer
{
    // TODO: Poolable, thread safe sprite batcher.
    // TODO: Use ArrayPool for buffers (see particle system)

    // TODO: The unordered SpriteBatcher (for alpha / transparent sprites) will need to be more similar to monogame's batcher.
    //       I can't neatly use a single SpriteMaterial per batcher in that case. Because the material changes!

    internal sealed unsafe class SpriteBatcher
    {
        private const int InitialBatchSize = 256;

        private const int MaxBatchSize = ushort.MaxValue / 6; // 6 = 4 vertices unique and 2 shared, per quad

        internal SpriteMaterial Material;
        internal int RegisterFlag;

        public int batchItemCount;

        private SpriteBatcherBucket[] buckets;

        private bool initialized;

        private GraphicsDevice GraphicsDevice => GameEngine.Engine.GraphicsDevice;
        private bool UseCustomEffect => Material.Effect != null;

        // The SpriteBatcher is reusable. It changes it's material when taken from the pool.
        internal void Configure(SpriteMaterial material)
        {
            Material = material;
            batchItemCount = 0;
        }

        internal void Initialize()
        {
            Texture2D mainTexture = Material.MainTexture;
            buckets = new SpriteBatcherBucket[1];
            buckets[0] = new SpriteBatcherBucket(InitialBatchSize);

            initialized = true;
        }

        internal void Clear()
        {
            Material = null;
        }

        internal void EnsureNextFrameCapacity()
        {
            int bucketsNeeded = 1 + (Material.RefCount / MaxBatchSize);
            if (bucketsNeeded > buckets.Length) {
                SpriteBatcherBucket[] newBuckets = new SpriteBatcherBucket[bucketsNeeded];
                Array.Copy(buckets, newBuckets, buckets.Length);
                for (int i = buckets.Length; i < newBuckets.Length; i++) {
                    newBuckets[i] = new SpriteBatcherBucket(MaxBatchSize);
                }
                buckets = newBuckets;
            }

            int curElements = Material.RefCount;
            int curBucket = 0;
            while (curElements > 0) {
                int nextCapacity = Math.Clamp(curElements, 0, MaxBatchSize);
                buckets[curBucket].EnsureNextFrameCapacity(nextCapacity);
                curElements -= nextCapacity;
                curBucket++;
            }
        }

        internal void Add(SpriteVertexMaterialData* materialVert)
        {
            int index = batchItemCount;

            int bucketIndex = index / MaxBatchSize;
            index -= bucketIndex * MaxBatchSize;

            VertexPositionColorTexture* vertexArrayPtr = buckets[bucketIndex].GetVertexPtr(index);
            SpriteVertexHelper.CopyTo(materialVert, vertexArrayPtr);
            batchItemCount++;
        }

        private void ApplyDeviceStates()
        {
            GraphicsDevice gd = GraphicsDevice;
            
            // Defaults.
            gd.RasterizerState = RasterizerState.CullCounterClockwise;
            gd.SamplerStates[0] = SamplerState.PointClamp;
            gd.BlendState = BlendState.Opaque;
            gd.DepthStencilState = DepthStencilState.None;

            // TODO: Maybe depth stencil state should be set in the RenderActions.
            //       That way, unordered can set it to read, normal can set it to read/write.
        }

        internal void DrawBatch()
        {
            if (batchItemCount == 0)
                return;

            ApplyDeviceStates();

            int batchItems = batchItemCount;
            int curBucket = 0;

            if (!UseCustomEffect) {
                SpriteBatchManager.DefaultEffect.MainTexture = Material.MainTexture;
                SpriteBatchManager.DefaultEffectPass.Apply();

                while (batchItems != 0) {
                    int itemCount = Math.Clamp(batchItems, 1, MaxBatchSize);
                    int vertexCount = itemCount * 4;
                    SpriteBatcherBucket bucket = buckets[curBucket];

                    GraphicsDevice.DrawUserIndexedPrimitives(
                        PrimitiveType.TriangleList,
                        bucket.VertexArray,
                        0,
                        vertexCount,
                        bucket.IndexArray,
                        0,
                        (vertexCount / 4) * 2,
                        VertexPositionColorTexture.VertexDeclaration);
                    
                    batchItems -= itemCount;
                    curBucket++;
                }

            } else {
                Material.Effect.TransformMatrix = SpriteBatchManager.TransformMatrix;
                Material.Effect.MainTexture = Material.MainTexture;
                EffectPassCollection passes = Material.Effect.CurrentTechnique.Passes;

                while (batchItems != 0) {
                    int itemCount = Math.Clamp(batchItems, 1, MaxBatchSize);
                    int vertexCount = itemCount * 4;
                    SpriteBatcherBucket bucket = buckets[curBucket];

                    foreach (EffectPass pass in passes) {
                        pass.Apply();
                        GraphicsDevice.DrawUserIndexedPrimitives(
                            PrimitiveType.TriangleList,
                            bucket.VertexArray,
                            0,
                            vertexCount,
                            bucket.IndexArray,
                            0,
                            (vertexCount / 4) * 2,
                            VertexPositionColorTexture.VertexDeclaration);
                    }

                    batchItems -= itemCount;
                    curBucket++;
                }
            }

            // Reset count.
            batchItemCount = 0;
            RegisterFlag = 0;
        }
    }

    internal sealed unsafe class SpriteBatcherBucket
    {
        private const int MaxBatchSize = ushort.MaxValue / 6; // 6 = 4 vertices unique and 2 shared, per quad

        internal VertexPositionColorTexture[] VertexArray;
        internal short[] IndexArray;

        public SpriteBatcherBucket(int size)
        {
            VertexArray = new VertexPositionColorTexture[size];
        }

        internal void EnsureNextFrameCapacity(int size)
        {
            if (size * 4 >= VertexArray.Length) {
                EnsureCapacity(size * 4);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)] // Hot path.
        internal VertexPositionColorTexture* GetVertexPtr(int index)
        {
            fixed(VertexPositionColorTexture* ptr = &VertexArray[index * 4]) {
                return ptr;
            }
        }

        private void EnsureCapacity(int newSize)
        {
            var oldSize = VertexArray.Length;
            newSize = (newSize + 63) & (~63); // grow in chunks of 64.
            Array.Resize(ref VertexArray, newSize);
            for (int i = oldSize; i < newSize; i++)
                VertexArray[i] = new VertexPositionColorTexture();

            EnsureIndexArrayCapacity(Math.Min(newSize, MaxBatchSize));
        }

        private void EnsureIndexArrayCapacity(int numBatchItems)
        {
            int neededCapacity = 6 * numBatchItems;
            if (IndexArray != null && neededCapacity <= IndexArray.Length)
                return;

            short[] newIndex = new short[6 * numBatchItems];
            
            int start = 0;
            if (IndexArray != null) {
                IndexArray.CopyTo(newIndex, 0);
                start = IndexArray.Length / 6;
            }

            fixed (short* indexFixedPtr = newIndex) {
                short* indexPtr = indexFixedPtr + (start * 6);
                for (int i = start; i < numBatchItems; i++, indexPtr += 6) {
                    /*
                     *  TL    TR
                     *   0----1 0,1,2,3 = index offsets for vertex indices
                     *   |   /| TL,TR,BL,BR are vertex references in SpriteBatchItem.
                     *   |  / |
                     *   | /  |
                     *   |/   |
                     *   2----3
                     *  BL    BR
                     */
                    // Triangle 1
                    *(indexPtr + 0) = (short)(i * 4);
                    *(indexPtr + 1) = (short)(i * 4 + 1);
                    *(indexPtr + 2) = (short)(i * 4 + 2);
                    // Triangle 2
                    *(indexPtr + 3) = (short)(i * 4 + 1);
                    *(indexPtr + 4) = (short)(i * 4 + 3);
                    *(indexPtr + 5) = (short)(i * 4 + 2);
                }
            }
            IndexArray = newIndex;
        }
    }
}
