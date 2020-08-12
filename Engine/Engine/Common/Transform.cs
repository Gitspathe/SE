using System;
using System.Numerics;
using SE.Utility;
using Quaternion = System.Numerics.Quaternion;
using Vector2 = System.Numerics.Vector2;
using Vector3 = System.Numerics.Vector3;

namespace SE.Common
{
    /// <summary>
    /// Contains information about an object's position, rotation and scale. 
    /// Transforms are hierarchical, their positions, rotations and scales are derived from their parent Transform.
    /// </summary>
    public class Transform : IDisposable
    {
        /// <summary>The GameObject's transform.</summary>
        public GameObject GameObject {
            get => GameObjectProp;
            private protected set => GameObjectProp = value;
        }
        private protected virtual GameObject GameObjectProp { get; set; }

        /// <summary>Parent Transform.</summary>
        public Transform Parent { get; private set; }

        /// <summary>Children of this Transform.</summary>
        public QuickList<Transform> Children { get; } = new QuickList<Transform>(0);

        /// <summary>Holds child Transforms enable or disabled state.</summary>
        internal StateTree ChildStateTree;

        /// <summary>
        /// Gets the Transform's global position.
        /// </summary>
        /// <returns>Global position in pixels.</returns>
        public Vector2 GlobalPosition {
            get => GlobalPositionInternal;
            set {
                if(value == GlobalPositionInternal)
                    return;

                if(Parent == null)
                    localPosition = value;
                else
                    localPosition = value - Parent.localPosition;

                UpdateTransformation();
            }
        }

        /// <summary>
        /// Gets the Transform's global rotation.
        /// </summary>
        /// <returns>Global rotation in degrees.</returns>
        public float GlobalRotation {
            get => GlobalRotationInternal;
            set {
                if (value == GlobalRotationInternal)
                    return;

                if (Parent == null)
                    localRotation = value;
                else 
                    localRotation = value - GlobalRotationInternal;

                UpdateTransformation();
            }
        }

        /// <summary>
        /// Gets the Transform's global scale.
        /// </summary>
        /// <returns>Global scale multiplier.</returns>
        public Vector2 GlobalScale {
            get => GlobalScaleInternal;
            set {
                if (value == GlobalScaleInternal)
                    return;

                if (Parent == null)
                    localScale = value;
                else
                    localScale = value - GlobalScaleInternal;

                UpdateTransformation();
            }
        }

        /// <summary>
        /// Gets the Transform's local position, or global position if it has no parent Transform.
        /// </summary>
        /// <returns>Local position if Parent isn't null, global position if Parent is null.</returns>
        public Vector2 Position {
            get => Parent == null ? GlobalPositionInternal : localPosition;
            set {
                if (value == localPosition)
                    return;

                localPosition = value;
                UpdateTransformation();
            }
        }

        /// <summary>
        /// Gets the Transform's local rotation, or global rotation if it has no parent Transform.
        /// </summary>
        /// <returns>Local rotation if Parent isn't null, global rotation if Parent is null.</returns>
        public float Rotation {
            get => Parent == null ? GlobalRotationInternal : localRotation;
            set {
                if (value == localRotation)
                    return;

                localRotation = value;
                UpdateTransformation();
            }
        }

        /// <summary>
        /// Gets the Transform's local scale, or global scale if it has no parent Transform.
        /// </summary>
        /// <returns>Local scale if Parent isn't null, global scale if Parent is null.</returns>
        public Vector2 Scale {
            get => Parent == null ? GlobalScaleInternal : localScale;
            set {
                if (value == localScale)
                    return;

                localScale = value;
                UpdateTransformation();
                GameObject.RecalculateBoundsInternal();
            }
        }

        /// <summary>
        /// Gets the root, top-most parent of this Transform.
        /// </summary>
        /// <returns>Root Transform.</returns>
        public Transform Root => Parent == null ? this : Parent.Root;

        /// <summary>
        /// Gets the Transform's local transformation matrix.
        /// </summary>
        /// <returns>Local transformation matrix.</returns>
        public Matrix4x4 LocalTransformation =>
            Matrix4x4.CreateScale(localScale.X, localScale.Y, 1.0f) *
            Matrix4x4.CreateRotationZ(localRotation) *
            Matrix4x4.CreateTranslation(localPosition.X, localPosition.Y, 0f);

        // Events
        /// <summary>Called when the Transform's parent is set. May be null in the case that the parent is removed or nonexistent.</summary>
        public event Action<Transform> ParentSet;

        /// <summary>Called when a child is added to the Transform.</summary>
        public event Action<Transform> ChildAdded;

        /// <summary>Called when a child is removed from the Transform.</summary>
        public event Action<Transform> ChildRemoved;

        // INTERNAL FOR PERFORMANCE! DO NOT MODIFY DIRECTLY!!
        internal Vector2 GlobalPositionInternal;
        internal Vector2 GlobalScaleInternal;
        internal float GlobalRotationInternal;

        private Vector2 localPosition;
        private Vector2 localScale;
        private float localRotation;

        /// <summary>
        /// Creates a new Transform instance.
        /// </summary>
        /// <param name="position">2D Position</param>
        /// <param name="rotation">Rotation in degrees.</param>
        /// <param name="scale">Scale.</param>
        /// <param name="ownerGameObject">GameObject ownerGameObject of the Transform.</param>
        /// <param name="parent">Parent Transform.</param>
        public Transform(Vector2 position, Vector2 scale, float rotation = 0.0f, GameObject ownerGameObject = null, Transform parent = null)
        {
            localPosition = position;
            localRotation = rotation;
            localScale = scale;
            GameObject = ownerGameObject;
            if (parent != null) {
                SetParent(parent);
            }
            UpdateTransformation();
            ChildStateTree = new StateTree(this);
        }

        /// <summary>
        /// Sets the Transform's parent Transform. Position, rotation and scale will be scaled with this Transform.
        /// </summary>
        /// <param name="transform">Parent</param>
        /// <param name="resetPosition">If true, transformation will be updated. Defaults to true.</param>
        public void SetParent(Transform transform, bool resetPosition = true)
        {
            if (transform == this)
                return;

            if (transform == null && Parent != null) {
                Parent.RemoveChild(this);
            } else {
                Parent?.RemoveChild(this);
                Parent = transform;
                Parent?.AddChild(this);
            }
            if (resetPosition) {
                UpdateTransformation();
            }

            GameObject.RecalculateBoundsInternal();
            ParentSet?.Invoke(transform);
            EngineUtility.TransformHierarchyDirty = true;
        }

        private void RemoveParent()
        {
            if (Parent == null)
                return;

            Parent = null;
            ParentSet?.Invoke(null);
            EngineUtility.TransformHierarchyDirty = true;
        }

        /// <summary>
        /// Adds a child to the Transform.
        /// </summary>
        /// <param name="child">Child Transform.</param>
        private void AddChild(Transform child)
        {
            Children.Add(child);
            UpdateTransformation();
            ChildAdded?.Invoke(child);
            EngineUtility.TransformHierarchyDirty = true;
        }

        /// <summary>
        /// Removes a child from the Transform.
        /// </summary>
        /// <param name="child">Child Transform.</param>
        private void RemoveChild(Transform child)
        {
            if (child.Parent != this)
                return;

            child.RemoveParent();
            Children.Remove(child);
            UpdateTransformation();
            ChildRemoved?.Invoke(child);
            EngineUtility.TransformHierarchyDirty = true;
        }

        /// <summary>
        /// Gets all of this Transform's children, including the children of it's children.
        /// </summary>
        /// <param name="children"></param>
        /// <returns></returns>
        public QuickList<Transform> GetAllChildren(QuickList<Transform> children)
        {
            foreach (Transform child in Children) {
                children.Add(child);
                child.GetAllChildren(children);
            }
            return children;
        }

        /// <summary>
        /// Gets all of this Transform's children, including the children of it's children. Non-allocating.
        /// </summary>
        /// <param name="children">List which the children will be added to.</param>
        public void GetAllChildrenNonAlloc(QuickList<Transform> children)
        {
            foreach (Transform child in Children) {
                children.Add(child);
                child.GetAllChildrenNonAlloc(children);
            }
        }

        /// <summary>
        /// Updates the Transform's global transformation.
        /// </summary>
        public void UpdateTransformation()
        {
            if (Children.Count == 0) {
                UpdateTransformationMatrix();
            } else {
                for (int i = 0; i < Children.Count; i++) {
                    Children.Array[i].UpdateTransformation();
                }
            }
            GameObject.PhysicsObject?.OverridePosition(GlobalPositionInternal);
            GameObject.RecalculateBoundsInternal();
        }

        protected virtual Matrix4x4 UpdateTransformationMatrix()
        {
            Matrix4x4 globalTransform = LocalTransformation;
            if (Parent != null) {
                globalTransform *= Parent.UpdateTransformationMatrix();
            }

            DecomposeMatrix(ref globalTransform, out GlobalPositionInternal, out GlobalRotationInternal, out GlobalScaleInternal);
            return globalTransform;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing = true)
        {
            ParentSet = null;
            ChildAdded = null;
            ChildRemoved = null;
        }

        /// <summary>
        /// Decomposes a matrix into it's position, rotation and scale.
        /// </summary>
        /// <param name="matrix">Matrix to decompose.</param>
        /// <param name="position">Position ref.</param>
        /// <param name="rotation">Rotation ref.</param>
        /// <param name="scale">Scale ref.</param>
        public static void DecomposeMatrix(ref Matrix4x4 matrix, out Vector2 position, out float rotation, out Vector2 scale)
        {
            Matrix4x4.Decompose(matrix, out Vector3 scale3, out Quaternion rotationQ, out Vector3 position3);
            Vector2 direction = Vector2.Transform(Vector2.UnitX, rotationQ);
            rotation = MathF.Atan2(direction.Y, direction.X);
            position = new Vector2(position3.X, position3.Y);
            scale = new Vector2(scale3.X, scale3.Y);
        }

        /// <summary>
        /// Returns an empty transform instance.
        /// </summary>
        public static Transform Empty => new Transform(Vector2.Zero, Vector2.One);
    }

    /// <summary>
    /// Class which holds the enabled or disabled state of a Transform's children. This is used to allow for complex and remembered Transform hierarchy states.
    /// The UI system is a good example of where this is used. When opening and closing menus, the enabled/disabled state of the Transform's children is retained.
    /// </summary>
    public class StateTree
    {
        private TransformNode owner;

        /// <summary>
        /// Creates a new state tree.
        /// </summary>
        /// <param name="o">Owner Transform.</param>
        public StateTree(Transform o) 
            => owner = new TransformNode(o, true);

        /// <summary>
        /// Regenerates the state tree, and reconfigures child nodes as needed.
        /// </summary>
        public void Regenerate()
            => owner = new TransformNode(owner.Transform, true);

        /// <summary>
        /// Applies the entire state tree to the owner Transform and it's children.
        /// </summary>
        public void Apply()
            => owner.Restore(owner.Transform);

        /// <summary>
        /// A node within the Transform state tree.
        /// </summary>
        internal struct TransformNode
        {
            /// <summary>Enabled/disabled state of the node.</summary>
            public bool State;

            /// <summary>Node's transform.</summary>
            public Transform Transform;

            /// <summary>Children of the node's Transform.</summary>
            public QuickList<TransformNode> Children;

            /// <summary>
            /// Creates a new Transform node for use in a state tree.
            /// </summary>
            /// <param name="transform">Node's Transform.</param>
            /// <param name="root">True if the Transform has no parent.</param>
            public TransformNode(Transform transform, bool root = false)
            {
                Transform = transform;
                State = transform.GameObject.Enabled;
                Children = new QuickList<TransformNode>(transform.Children.Count);
                Transform[] transformChildren = transform.Children.Array;
                for (int i = 0; i < transform.Children.Count; i++) {
                    Children.Add(new TransformNode(transformChildren[i]));
                }
            }

            /// <summary>
            /// Restores this node's enabled/disabled state to it's Transform.
            /// </summary>
            /// <param name="root">Should be the original caller of the Restore() function in the state tree that owns this node.</param>
            public void Restore(Transform root)
            {
                bool isRoot = root == Transform;

                if (State && !Transform.GameObject.Enabled) {
                    Transform.GameObject.Enable(isRoot);
                }
                for (int i = 0; i < Children.Count; i++) {
                    Children.Array[i].Restore(root);
                }
            }
        }

    }

}
