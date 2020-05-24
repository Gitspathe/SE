using System;
using System.Collections.Generic;
using SE.Core;
using Console = SE.Core.Console;

namespace SE
{
    /// <summary>
    /// Class which controls the main game loop.
    /// </summary>
    public class GameLoop
    {
        internal SortedDictionary<int, Action> Loop { get; private set; } = new SortedDictionary<int, Action>();

        /// <summary>
        /// Adds a new Action to the game loop.
        /// </summary>
        /// <param name="order">Sequence of the Action. Controls where in the queue the Action is called.</param>
        /// <param name="action">Action to call.</param>
        public void Add(int order, Action action)
        {
            Loop.Add(order, action);
        }

        /// <summary>
        /// Adds a new Action to the game loop.
        /// </summary>
        /// <param name="order">Sequence of the Action, based on the default game loop.</param>
        /// <param name="action">Action to call.</param>
        public void Add(DefaultEnum order, Action action)
        {
            Add((int)order, action);
        }

        /// <summary>
        /// Removes an action from the game loop, at a given sequence position.
        /// </summary>
        /// <param name="order">Sequence position to remove.</param>
        public void Remove(int order)
        {
            Loop.Remove(order);
        }

        /// <summary>
        /// Removes an action from the game loop, at a given default game loop sequence position.
        /// </summary>
        /// <param name="order">Sequence position to remove, based on the default game loop.</param>
        public void Remove(DefaultEnum order)
        {
            Remove((int)order);
        }

        /// <summary>
        /// Creates a new default game loop.
        /// </summary>
        public GameLoop()
        {
        #if EDITOR
            if (GameEngine.Engine.LevelEditMode && !Screen.IsFullHeadless) {
                //Add(DefaultEnum.LevelEditor, LevelEditor.Update);
            }
        #endif

            Add(DefaultEnum.Time, () => Time.Update(GameEngine.Engine.GameTime));
            Add(DefaultEnum.NewPhysics, Core.Physics.Update);
            Add(DefaultEnum.UpdateDynamicGameObjects, GameEngine.Engine.UpdateDynamicGameObjects);
            Add(DefaultEnum.Networking, () => Network.Update(Time.UnscaledDeltaTime));
            Add(DefaultEnum.UpdateLevelManager, SceneManager.Update);
            Add(DefaultEnum.SpatialPartition, SpatialPartitionManager.Update);
            Add(DefaultEnum.Console, Console.Update);
            Add(DefaultEnum.FinalizeTime, Time.FinalizeFixedTimeStep);

            if (!Screen.IsFullHeadless) {
                Add(DefaultEnum.StartParticles, ParticleEngine.Update);
                Add(DefaultEnum.StartNewParticles, () => SEParticles.ParticleEngine.Update(Time.DeltaTime));

                Add(DefaultEnum.InputManager, () => InputManager.Update(Time.UnscaledDeltaTime));
                Add(DefaultEnum.UIManager, UIManager.Update);
                Add(DefaultEnum.Screen, Screen.Update);
                Add(DefaultEnum.RenderingSystem, Core.Rendering.Update);
            }
        }

        /// <summary>
        /// Creates a new game loop.
        /// </summary>
        /// <param name="loop">Dictionary of Actions and their order for the new game loop.</param>
        public GameLoop(SortedDictionary<int, Action> loop)
        {
            Loop = loop;
        }

        /// <summary>
        /// Enum containing the complete sequence of the default game loop.
        /// </summary>
        public enum DefaultEnum
        {
            LevelEditor = 100,
            Time = 200,
            InputManager = 300,
            Screen = 400,
            UIManager = 500,
            NewPhysics = 650,
            UpdateDynamicGameObjects = 700,
            StartNewParticles = 701,
            Networking = 800,
            UpdateLevelManager = 900,
            SpatialPartition = 1000,
            StartParticles = 1100,
            Console = 1300,
            RenderingSystem = 2000,
            FinalizeTime = 3000
        }

        public override string ToString()
        {
            string s = "";
            foreach (KeyValuePair<int, Action> loop in Loop) {
                if (loop.Value.Method.ReflectedType != null) {
                    s += "  " + loop.Key + ", " + loop.Value.Method.ReflectedType.FullName + "." + loop.Value.Method.Name + "\n";
                }
            }
            return s;
        }

    }

}