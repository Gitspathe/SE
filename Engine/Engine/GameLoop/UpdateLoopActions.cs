﻿using SE.Core;

namespace SE.GameLoop
{
    public interface IUpdateLoopAction
    {
        string Name { get; }
        void Invoke();
    }

    public class LoopTime : IUpdateLoopAction
    {
        public string Name => "Update time";
        public void Invoke() => Time.Update(GameEngine.Engine.GameTime);
    }

    public class LoopPhysics : IUpdateLoopAction
    {
        public string Name => "Update physics";
        public void Invoke() => Core.Physics.Update();
    }

    public class LoopDynamicUpdateObjects : IUpdateLoopAction
    {
        public string Name => "Update dynamic GameObjects";
        public void Invoke()
        {
            for (int i = 0; i < GameEngine.DynamicGameObjects.Count; i++) {
                GameEngine.DynamicGameObjects.Array[i].Update();
            }
        }
    }

    public class LoopNetworking : IUpdateLoopAction
    {
        public string Name => "Update network";
        public void Invoke() => Network.Update(Time.UnscaledDeltaTime);
    }

    public class LoopScene : IUpdateLoopAction
    {
        public string Name => "Update scene manager";
        public void Invoke() => SceneManager.Update();
    }

    public class LoopSpatialPartition : IUpdateLoopAction
    {
        public string Name => "Update spatial partition manager";
        public void Invoke() => SpatialPartitionManager.Update();
    }

    public class LoopConsole : IUpdateLoopAction
    {
        public string Name => "Update console";
        public void Invoke() => Console.Update();
    }

    public class LoopFinalizeTime : IUpdateLoopAction
    {
        public string Name => "Finalize time";
        public void Invoke() => Time.FinalizeFixedTimeStep();
    }

    public class LoopBeginAsyncParticles : IUpdateLoopAction
    {
        public string Name => "Begin asyncronous particles update task";

        // TODO: Actually support zero or more cameras properly!
        public void Invoke() => ParticleEngine.Update(Time.DeltaTime, Core.Rendering.CameraBounds[0]);
    }

    public class LoopInputManager : IUpdateLoopAction
    {
        public string Name => "Update input manager";
        public void Invoke() => InputManager.Update(Time.UnscaledDeltaTime);
    }

    public class LoopUIManager : IUpdateLoopAction
    {
        public string Name => "Update UI manager";
        public void Invoke() => UIManager.Update();
    }

    public class LoopScreen : IUpdateLoopAction
    {
        public string Name => "Update screen";
        public void Invoke() => Screen.Update();
    }

    public class LoopRendering : IUpdateLoopAction
    {
        public string Name => "Perform render loop";
        public void Invoke() => Core.Rendering.Update();
    }
}
