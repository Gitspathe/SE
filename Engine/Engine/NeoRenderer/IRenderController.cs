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

        internal readonly QuickList<SpriteComponent> CullingSprites = new QuickList<SpriteComponent>(2048);
        internal readonly QuickList<SpriteBatcher> BatchersToCall = new QuickList<SpriteBatcher>();

        private IThreadTask[] threadTasks;

        public void Initialize()
        {

        }

        public void PrepareActions(RenderActionContainer actionContainer, Camera2D camera)
        {
            // Culling.
            for (int i = 0; i < 20; i++) {
                SpatialPartitionManager<SpriteComponent>.GetFromRegion(CullingSprites, camera.VisibleArea);
            }
            //SpatialPartitionManager<SpriteComponent>.GetFromRegion(CullingSprites, camera.VisibleArea);
            SpriteBatchManager.EnsureNextFrameCapacity();

            int batchNum = Math.Clamp(ThreadManager.ThreadCount, 1, ThreadManager.ThreadCount);
            if (threadTasks == null || batchNum != threadTasks.Length) {
                threadTasks = new IThreadTask[batchNum];
                for (int i = 0; i < batchNum; i++) {
                    threadTasks[i] = new BatchTask(this);
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

                BatchTask task = (BatchTask)threadTasks[i];
                task.Set(from, to);

                from = to;
            }

            ThreadManager.RunParallelTasks(threadTasks);


            actionContainer.AddAction(new ConfigureSpriteBatchManager(camera));
            foreach (SpriteBatcher batcher in BatchersToCall) {
                actionContainer.AddAction(new RenderBatchTest(batcher.Material.RenderQueue, batcher));
            }

            CullingSprites.Clear();
            BatchersToCall.Clear();
        }

        public class BatchTask : IThreadTask
        {
            private readonly SpritesRenderController controller;
            private int from;
            private int to;

            public void Set(int from, int to)
            {
                this.from = from;
                this.to = to;
            }

            public BatchTask(SpritesRenderController controller)
            {
                this.controller = controller;
            }

            public void Execute()
            {
                SpriteComponent[] array = controller.CullingSprites.Array;

                for (int i = from; i < to; i++) {
                    SpriteComponent sprite = array[i];
                    SpriteMaterial mat = sprite.Material;

                    if (!mat.SpecialRenderOrderingInternal) {

                        SpriteBatcher batcher = SpriteBatchManager.GetBatcher(mat);

                        // Interlocked is causing slowdown due to low-level contention!
                        // Idea: Put all sprites into a giant vertex array in parallel with NO synchronization (no lock nor interlocked)
                        //       And then just copy them into the relevant SpriteBatchers.

                        int spriteIndex = Interlocked.Increment(ref batcher.batchItemCount) - 1;
                        if (spriteIndex == 0) {
                            controller.BatchersToCall.Add(batcher);
                        }

                        sprite.AddToBatcher(spriteIndex, batcher);

                    } else {
                        // TODO: Unsorted render list shit.
                    }

                }
            }
        }

    }
}
