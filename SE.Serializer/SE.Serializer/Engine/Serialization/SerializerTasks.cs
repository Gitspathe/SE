using System.Collections.Generic;

namespace SE.Serialization
{

    internal class SharedSerializeTaskData
    {
        // TODO: Object reference handling.
        public Dictionary<object, ObjectRef> ObjectRefs;
        public int curReference;
    }

    // TODO: Reference loop handling.
    public ref struct SerializeTask
    {
        public SerializerSettings Settings;

        internal int CurrentDepth;
        internal int CurrentParameterIndex;

        internal SharedSerializeTaskData Data;

        internal bool TryGetObjectRef(object obj, out ObjectRef id)
        {
            return Data.ObjectRefs.TryGetValue(obj, out id);
        }

        internal bool AddReference(object obj, ObjectRef objRef)
        {
            if (Data.ObjectRefs.ContainsKey(obj)) {
                return false;
            }

            Data.ObjectRefs.Add(obj, objRef);
            return true;
        }

        public SerializeTask(SerializerSettings settings)
        {
            CurrentDepth = 0;
            CurrentParameterIndex = 0;
            Settings = settings;
            Data = new SharedSerializeTaskData();
            Data.curReference = 0;
            Data.ObjectRefs = null;
            if(settings.ReferenceHandling == ReferenceHandling.Preserve) {
                Data.ObjectRefs = new Dictionary<object, ObjectRef>();
            }
        }

        public SerializeTask Clone(int incrementDepth = 1)
        {
            SerializeTask copy = new SerializeTask {
                CurrentDepth = CurrentDepth + incrementDepth,
                Settings = Settings,
                Data = Data
            };
            return copy;
        }
    }

    public ref struct DeserializeTask
    {
        public SerializerSettings Settings;

        private Dictionary<object, ObjectRef> ObjectRefs;
        private Dictionary<int, ObjectRef> ObjectRefsByKey;

        internal bool TryGetObjectRef(object obj, out ObjectRef id)
        {
            return ObjectRefs.TryGetValue(obj, out id);
        }

        internal bool TryGetObjectRefByKey(int refId, out ObjectRef id)
        {
            return ObjectRefsByKey.TryGetValue(refId, out id);
        }

        internal bool AddReference(object obj, ObjectRef objRef)
        {
            if (ObjectRefs.ContainsKey(obj)) {
                return false;
            }

            ObjectRefs.Add(obj, objRef);
            ObjectRefsByKey.Add(objRef.Reference, objRef);
            return true;
        }

        public DeserializeTask(SerializerSettings settings)
        {
            Settings = settings;
            ObjectRefs = null;
            ObjectRefsByKey = null;
            if (settings.ReferenceHandling == ReferenceHandling.Preserve) {
                ObjectRefs = new Dictionary<object, ObjectRef>();
                ObjectRefsByKey = new Dictionary<int, ObjectRef>();
            }
        }
    }

    public struct ObjectRef
    {
        public object Obj;
        public int Reference;
        public int RefCount;

        public ObjectRef(object obj, int reference)
        {
            Obj = obj;
            Reference = reference;
            RefCount = 0;
        }
    }
}
