using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using SE.Attributes;
using SE.Common;
using SE.Engine.Utility;
using SE.Serialization;
using SE.World;
using Vector2 = System.Numerics.Vector2;

namespace SE.Core.Internal
{
    internal static class Reflection
    {
        public static HashSet<Type> SerializableTypes = new HashSet<Type>
        {
            typeof(bool), typeof(byte), typeof(sbyte),
            typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong),
            typeof(float), typeof(double),
            typeof(string), typeof(Color), typeof(Vector2), typeof(Rectangle), typeof(RectangleF),
            typeof(List<>)
        };

        public static void Initialize()
        {
            SerializerReflection.RegenerateEngineSerializers();
            SerializerReflection.RegenerateSerializers();
            GameEngine.RequestGC();
        }

        internal static GameObjectInfo GetGameObjectInfo(Type gameObject)
        {
            // Throw an error if the Type provided isn't a GameObject.
            if (gameObject != typeof(GameObject) && !gameObject.IsSubclassOf(typeof(GameObject)))
                throw new Exception(nameof(gameObject) + " is not a GameObject.");

            // Try and fetch the list of components from cache.
            if (Cache.GameObjects.TryGetValue(gameObject, out GameObjectInfo foundInfo)) {
                return foundInfo;
            }
                   
            // If an entry isn't found, create one.
            List<Type> components = new List<Type>();
            ComponentsAttribute attribute = gameObject
               .GetCustomAttributes(typeof(ComponentsAttribute), true)
               .FirstOrDefault() as ComponentsAttribute;
            if (attribute != null) {
                foreach (Type t in attribute.Components) {
                    components.Add(t);
                }
            }

            GameObjectInfo info = new GameObjectInfo();
            info.Components = components;

            // Add to cache and return components.
            Cache.GameObjects.Add(gameObject, info);
            return info;
        }

        internal static ComponentInfo GetComponentInfo(Type component)
        {
            if (component != typeof(Component) && !component.IsSubclassOf(typeof(Component)))
                throw new Exception(nameof(component) + " is not a Component.");

            // Try and fetch from cache.
            if (Cache.Components.TryGetValue(component, out ComponentInfo result)) {
                return result;
            }

            result = new ComponentInfo();

            // If an entry isn't found, create one.
            ExecuteInEditorAttribute attribute = component
               .GetCustomAttributes(typeof(ExecuteInEditorAttribute), true)
               .FirstOrDefault() as ExecuteInEditorAttribute;
            if (attribute != null) {
                result.RunInEditor = true;
            }

            // Add to cache and return.
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

            IEnumerable<SceneScript> enumerable = GetTypes<SceneScript>(myType =>
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

        public static IEnumerable<Type> GetTypes(Func<Type, bool> predicate)
        {
            List<Type> enumerable = new List<Type>();
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()) {
                enumerable.AddRange(assembly
                   .GetTypes()
                   .Where(predicate)
                   .ToList()
                );
            }
            return enumerable;
        }

        public static IEnumerable<T> GetTypes<T>(Func<Type, bool> predicate)
        {
            IEnumerable<Type> types = GetTypes(predicate);
            List<T> enumerable = new List<T>();
            foreach (Type t in types) {
                enumerable.Add((T)Activator.CreateInstance(t));
            }
            return enumerable;
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
        }

        public class ComponentInfo
        {
            public bool RunInEditor;
        }

        public class SceneInfo
        {
            public SceneScript SceneScript;
            public bool RunInEditor;
        }
    }
}
