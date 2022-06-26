using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using SE.Common;
using SE.Components;
using SE.Core;
using System;
using System.Collections.Generic;
using System.Text;
using SE.Utility;
using SE.Threading;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

// ReSharper disable InconsistentlySynchronizedField

namespace SE.NeoRenderer
{
    public interface IRenderController
    {
        uint UpdateOrder { get; }
        
        void Initialize();
        void PrepareActions(RenderActionContainer actionContainer, Camera2D camera);
    }

    public sealed class SpritesRenderController : IRenderController
    {
        public uint UpdateOrder => 0;

        internal SpriteVertexMaterialData[] SpriteVertexArray = new SpriteVertexMaterialData[4096];
        internal readonly QuickList<SpriteComponent> CullingSprites = new QuickList<SpriteComponent>(4096);
        internal readonly QuickList<SpriteBatcher> BatchersToCall = new QuickList<SpriteBatcher>();

        private IThreadTask[] threadTasks;

        public void Initialize()
        {

        }

        public void PrepareActions(RenderActionContainer actionContainer, Camera2D camera)
        {
            // Culling.
            SpatialPartitionManager<SpriteComponent>.GetFromRegion(CullingSprites, camera.VisibleArea);
            
            EnsureVertexArrayCapacity(CullingSprites.Count);

            int batchNum = Math.Clamp(ThreadManager.ThreadCount, 1, ThreadManager.ThreadCount);
            if (threadTasks == null || batchNum != threadTasks.Length) {
                threadTasks = new IThreadTask[batchNum];
                for (int i = 0; i < batchNum; i++) {
                    threadTasks[i] = new WriteVertexTask(this);
                }
            }

            int from = 0;
            int to = 0;
            for (int i = 0; i < batchNum; i++) {
                if (i == batchNum - 1) {
                    to = CullingSprites.Count;
                } else {
                    to += CullingSprites.Count / batchNum;
                }

                WriteVertexTask task = (WriteVertexTask)threadTasks[i];
                task.Set(from, to);

                from = to;
            }

            ThreadManager.RunParallelTasks(threadTasks);

            CopyVertexToBatchers(CullingSprites.Count);

            actionContainer.AddAction(new ConfigureSpriteBatchManager(camera));
            foreach (SpriteBatcher batcher in BatchersToCall) {
                actionContainer.AddAction(new RenderBatchTest(batcher.Material.RenderQueue, batcher));
            }

            CullingSprites.Clear();
            BatchersToCall.Clear();
        }

        private void EnsureVertexArrayCapacity(int count)
        {
            if (SpriteVertexArray.Length >= count)
                return;

            int nextSize = (SpriteVertexArray.Length + 1) * 2;
            SpriteVertexMaterialData[] newArray = new SpriteVertexMaterialData[nextSize];
            Array.Copy(SpriteVertexArray, newArray, SpriteVertexArray.Length);
            SpriteVertexArray = newArray;
        }

        private unsafe void CopyVertexToBatchers(int length)
        {
            for (int i = 0; i < length; i++) {
                SpriteVertexMaterialData data = SpriteVertexArray[i];
                SpriteMaterialInfo info = SpriteMaterialHandler.GetSpriteMaterialInfo(data.MaterialID);

                if (!info.Material.SpecialRenderOrderingInternal) {
                    SpriteBatcher batcher = info.Batcher;

                    if (batcher.batchItemCount == 0) {
                        batcher.EnsureNextFrameCapacity();
                        BatchersToCall.Add(batcher);
                    }

                    batcher.Add(&data);

                } else {
                    // TODO: Unsorted render list shit.
                }
            }
        }

        public class WriteVertexTask : IThreadTask
        {
            private readonly SpritesRenderController controller;
            private int from;
            private int to;

            public void Set(int from, int to)
            {
                this.from = from;
                this.to = to;
            }

            public WriteVertexTask(SpritesRenderController controller)
            {
                this.controller = controller;
            }

            public unsafe void Execute()
            {
                SpriteComponent[] array = controller.CullingSprites.Array;

                for (int i = from; i < to; i++) {
                    SpriteComponent sprite = array[i];
                    fixed (SpriteVertexMaterialData* ptr = &controller.SpriteVertexArray[i]) {
                        sprite.SetMaterialVertex(ptr);
                    }
                }
            }
        }

    }
}
