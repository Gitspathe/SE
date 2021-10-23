using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace SE.Serialization
{
    public ref struct SerializeTask
    {
        public SerializerSettings Settings;
        public int CurrentDepth;
        public int CurrentParameterIndex;

        public Dictionary<object, ObjectRef> ObjectRefs;
        public int curReference;

        internal bool TryGetObjectRef(object obj, out ObjectRef id)
        {
            return ObjectRefs.TryGetValue(obj, out id);
        }

        internal bool AddReference(object obj, ObjectRef objRef)
        {
            if (ObjectRefs.ContainsKey(obj)) {
                return false;
            }

            ObjectRefs.Add(obj, objRef);
            return true;
        }

        public SerializeTask(SerializerSettings settings)
        {
            CurrentDepth = 0;
            CurrentParameterIndex = 0;
            Settings = settings;
            curReference = 0;
            ObjectRefs = null;
            if(settings.ReferenceHandling == ReferenceHandling.Preserve) {
                ObjectRefs = new Dictionary<object, ObjectRef>();
            }
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
