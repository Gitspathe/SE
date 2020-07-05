using System;
using System.Collections.Generic;
using SE.Core;
using Console = SE.Core.Console;

namespace SE.GameLoop
{
    /// <summary>
    /// Class which controls the main game loop.
    /// </summary>
    public class GameLoop
    {
        internal SortedDictionary<int, IGameLoopAction> Loop { get; private set; } = new SortedDictionary<int, IGameLoopAction>();

        /// <summary>
        /// Adds a new Action to the game loop.
        /// </summary>
        /// <param name="order">Sequence of the Action. Controls where in the queue the Action is called.</param>
        /// <param name="action">Action to call.</param>
        public void Add(int order, IGameLoopAction action)
        {
            Loop.Add(order, action);
        }

        /// <summary>
        /// Adds a new Action to the game loop.
        /// </summary>
        /// <param name="order">Sequence of the Action, based on the default game loop.</param>
        /// <param name="action">Action to call.</param>
        public void Add(DefaultEnum order, IGameLoopAction action)
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

            Add(DefaultEnum.Time, new LoopTime());
            Add(DefaultEnum.Physics, new LoopPhysics());
            Add(DefaultEnum.UpdateDynamicGameObjects, new LoopDynamicGameObjects());
            Add(DefaultEnum.Networking, new LoopNetworking());
            Add(DefaultEnum.UpdateLevelManager, new LoopScene());
            Add(DefaultEnum.SpatialPartition, new LoopSpatialPartition());
            Add(DefaultEnum.Console, new LoopConsole());
            Add(DefaultEnum.FinalizeTime, new LoopFinalizeTime());

            if (!Screen.IsFullHeadless) {
                Add(DefaultEnum.StartNewParticles, new LoopBeginAsyncParticles());
                Add(DefaultEnum.InputManager, new LoopInputManager());
;               Add(DefaultEnum.UIManager, new LoopUIManager());
                Add(DefaultEnum.Screen, new LoopScreen());
                Add(DefaultEnum.RenderingSystem, new LoopRendering());
            }
        }

        /// <summary>
        /// Creates a new game loop.
        /// </summary>
        /// <param name="loop">Dictionary of Actions and their order for the new game loop.</param>
        public GameLoop(SortedDictionary<int, IGameLoopAction> loop)
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
            Physics = 600,
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
            foreach (KeyValuePair<int, IGameLoopAction> loop in Loop) {
                s += "  " + loop.Key + ", " + loop.Value.Name + ".\n";
            }
            return s;
        }

    }

}