using SE.Common;
using SE.Utility;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace SE.Pooling
{
    public class GameObjectPool<T> : IGameObjectPool where T : GameObject, IPoolableGameObject
    {
        public PoolBehavior Behaviour = PoolBehavior.Grow;

        protected QuickList<T> Pool;
        protected HashSet<T> Active;

        private Func<T> instantiateFunc;

        private bool returnOnDestroy;
        /// <summary>If true, GameObjects belonging to this pool will be returned on Destroy().</summary>
        public bool ReturnOnDestroy {
            get => returnOnDestroy;
            set {
                returnOnDestroy = value;
                for (int i = 0; i < Pool.Count; i++) {
                    Pool.Array[i].ReturnOnDestroy = value;
                }
                foreach (T go in Active) {
                    go.ReturnOnDestroy = value;
                }
            }
        }

        public int Count => PoolCount + ActiveCount;
        public int PoolCount => Pool.Count;
        public int ActiveCount => Active.Count;

        public T Take()
        {
            T gameObject;
            if (Pool.Count < 1) {
                switch (Behaviour) {
                    case PoolBehavior.Grow:
                        gameObject = instantiateFunc != null
                            ? instantiateFunc.Invoke()
                            : (T)GameObject.Instantiate(typeof(T), Vector2.Zero, 0f, Vector2.One);
                        gameObject.PoolInitialize();
                        break;
                    case PoolBehavior.Fixed:
                        return null;
                    case PoolBehavior.FixedInstantiate:
                        gameObject = instantiateFunc != null
                            ? instantiateFunc.Invoke()
                            : (T)GameObject.Instantiate(typeof(T), Vector2.Zero, 0f, Vector2.One);
                        return gameObject;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            } else {
                gameObject = Pool.Array[Pool.Count - 1];
            }
            if (gameObject == null)
                return null;

            Pool.Remove(gameObject);
            Active.Add(gameObject);
            gameObject.Enable();
            gameObject.TakenFromPool();
            return gameObject;
        }

        public T Take(Vector2 position, float rotation = 0f)
        {
            T go = Take();
            go.Transform.Position = position;
            go.Transform.Rotation = rotation;
            return go;
        }

        public T Take(Vector2 position, Vector2 scale, float rotation = 0f)
        {
            T go = Take();
            go.Transform.Position = position;
            go.Transform.Rotation = rotation;
            go.Transform.Scale = scale;
            return go;
        }

        public void Return(GameObject go)
        {
            if (go is T pooled) {
                if (!Active.Contains(pooled))
                    return;

                pooled.Disable();
                Active.Remove(pooled);
                Pool.Add(pooled);
                pooled.ReturnedToPool();
            } else {
                throw new Exception("Invalid object was added!");
            }
        }

        public void DestroyedCallback(GameObject go)
        {
            if (go is T pooled) {
                Active.Remove(pooled);
                Pool.Remove(pooled);
            }
        }

        public GameObjectPool(int startingCapacity = 128)
        {
            Pool = new QuickList<T>(startingCapacity);
            Active = new HashSet<T>(startingCapacity);
            for (int i = 0; i < startingCapacity; i++) {
                T go = (T)GameObject.Instantiate(typeof(T), Vector2.Zero, 0f, Vector2.One);
                go.MyPool = this;
                go.PoolInitialize();
                go.Disable();
                Pool.Add(go);
            }
            ReturnOnDestroy = false;
        }

        public GameObjectPool(Func<T> instantiateFunc, int startingCapacity = 128)
        {
            this.instantiateFunc = instantiateFunc;
            Pool = new QuickList<T>(startingCapacity);
            Active = new HashSet<T>(startingCapacity);
            for (int i = 0; i < startingCapacity; i++) {
                T go = instantiateFunc.Invoke();
                go.MyPool = this;
                go.PoolInitialize();
                go.Disable();
                Pool.Add(go);
            }
            ReturnOnDestroy = false;
        }
    }
}
