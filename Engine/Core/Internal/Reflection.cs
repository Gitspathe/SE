using SE.Attributes;
using SE.Common;
using SE.Serialization;
using SE.World;
using System;
using System.Collections.Generic;
using System.Linq;
using static SE.Core.ReflectionUtil;

namespace SE.Core.Internal
{
    internal static class Reflection
    {
        public static void Initialize()
        {
            SerializerReflection.RegenerateEngineSerializers();
            SerializerReflection.RegenerateSerializers();
            GameEngine.RequestGC();
        }

        internal static bool TypeHasAnyOfInterface(Type type, IEnumerable<Type> interfaces, out Type foundType)
        {
            // Check if it has interface
            foundType = null;
            Type[] i = type.GetInterfaces();
            foreach (Type inter in i) {
                if (interfaces.Contains(inter)) {
                    foundType = inter;
                    return true;
                }
            }
            return false;
        }

        internal static GameObjectInfo GetGameObjectInfo(Type gameObject)
        {
            // Try and fetch the list of components from cache.
            if (Cache.GameObjects.TryGetValue(gameObject, out GameObjectInfo foundInfo)) {
                return foundInfo;
            }

            // Throw an error if the Type provided isn't a GameObject.
            if (gameObject != typeof(GameObject) && !gameObject.IsSubclassOf(typeof(GameObject)))
                throw new Exception(nameof(gameObject) + " is not a GameObject.");

            // If an entry isn't found, create one.
            GameObjectInfo info = new GameObjectInfo();

            // Components attribute.
            ComponentsAttribute componentsAttribute = gameObject
               .GetCustomAttributes(typeof(ComponentsAttribute), true)
               .FirstOrDefault() as ComponentsAttribute;
            if (componentsAttribute != null) {
                List<Type> components = new List<Type>();
                foreach (Type t in componentsAttribute.Components) {
                    components.Add(t);
                }
                info.Components = components;
            }

            // Headless attribute.
            HeadlessModeAttribute headlessAttribute = gameObject
               .GetCustomAttributes(typeof(HeadlessModeAttribute), true)
               .FirstOrDefault() as HeadlessModeAttribute;
            if (headlessAttribute != null) {
                info.HeadlessSupportMode = headlessAttribute.SupportMode;
            }

            // Add to cache and return components.
            info.Update();
            Cache.GameObjects.Add(gameObject, info);
            return info;
        }

        internal static ComponentInfo GetComponentInfo(Type component)
        {
            // Try and fetch from cache.
            if (Cache.Components.TryGetValue(component, out ComponentInfo result)) {
                return result;
            }

            // Throw exception if the Type isn't a component.
            if (component != typeof(Component) && !component.IsSubclassOf(typeof(Component)))
                throw new Exception(nameof(component) + " is not a Component.");

            // If an entry isn't found, create one.
            result = new ComponentInfo();

            // Execute in editor attribute.
            ExecuteInEditorAttribute attribute = component
               .GetCustomAttributes(typeof(ExecuteInEditorAttribute), true)
               .FirstOrDefault() as ExecuteInEditorAttribute;
            if (attribute != null) {
                result.RunInEditor = true;
            }

            // Headless attribute.
            HeadlessModeAttribute headlessAttribute = component
               .GetCustomAttributes(typeof(HeadlessModeAttribute), true)
               .FirstOrDefault() as HeadlessModeAttribute;
            if (headlessAttribute != null) {
                result.HeadlessSupportMode = headlessAttribute.SupportMode;
            }


            // Add to cache and return.
            result.Update();
            Cache.Components.Add(component, result);
            return result;
        }

        internal static SceneInfo GetSceneInfo(string nameSpace, string name)
        {
            // Try and fetch from cache.
            if (Cache.Scenes.TryGetValue(new ValueTuple<string, string>(nameSpace, name), out SceneInfo result)) {
                return result;
            }

            result = new SceneInfo();

            IEnumerable<SceneScript> enumerable = GetTypeInstances<SceneScript>(myType =>
                myType.IsClass
                && !myType.IsAbstract
                && myType.IsSubclassOf(typeof(SceneScript)));
            foreach (SceneScript script in enumerable) {
                if (script.LevelNamespace != nameSpace || script.LevelName != name)
                    continue;

                result.SceneScript = script;
                ExecuteInEditorAttribute attribute = script?.GetType()
                   .GetCustomAttributes(typeof(ExecuteInEditorAttribute), true)
                   .FirstOrDefault() as ExecuteInEditorAttribute;
                if (attribute != null) {
                    result.RunInEditor = true;
                }
            }

            Cache.Scenes.Add((nameSpace, name), result);
            return result;
        }

        internal static class Cache
        {
            internal static Dictionary<Type, GameObjectInfo> GameObjects = new Dictionary<Type, GameObjectInfo>();
            internal static Dictionary<Type, ComponentInfo> Components = new Dictionary<Type, ComponentInfo>();
            internal static Dictionary<ValueTuple<string, string>, SceneInfo> Scenes = new Dictionary<(string, string), SceneInfo>();

            internal static void Clear()
            {
                GameObjects.Clear();
                Components.Clear();
                Scenes.Clear();
            }
        }

        public class GameObjectInfo
        {
            public List<Type> Components = new List<Type>();
            public HeadlessSupportMode HeadlessSupportMode = HeadlessSupportMode.Default;
            public bool Execute;

            internal void Update()
            {
                bool isHeadless = Screen.IsFullHeadless;
                Execute = true;
                if (HeadlessSupportMode == HeadlessSupportMode.NoHeadless && isHeadless)
                    Execute = false;
                else if (HeadlessSupportMode == HeadlessSupportMode.OnlyHeadless && !isHeadless)
                    Execute = false;
            }
        }

        public class ComponentInfo
        {
            public bool RunInEditor;
            public HeadlessSupportMode HeadlessSupportMode = HeadlessSupportMode.Default;
            public bool Execute;

            internal void Update()
            {
                bool isHeadless = Screen.IsFullHeadless;
                Execute = true;
                if (HeadlessSupportMode == HeadlessSupportMode.NoHeadless && isHeadless)
                    Execute = false;
                else if (HeadlessSupportMode == HeadlessSupportMode.OnlyHeadless && !isHeadless)
                    Execute = false;
                else if (GameEngine.IsEditor && !RunInEditor)
                    Execute = false;
            }
        }

        public class SceneInfo
        {
            public SceneScript SceneScript;
            public bool RunInEditor;
        }
    }
}
