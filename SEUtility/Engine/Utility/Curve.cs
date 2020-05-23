// MIT License - Copyright (C) The Mono.Xna Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace SE.Utility
{
    /// <summary>
    /// Contains a collection of <see cref="CurveKey"/> points in 2D space and provides methods for evaluating features of the curve they define.
    /// </summary>
    [DataContract]
    public class Curve
    {
        #region Private Fields

        private CurveKeyCollection keys;
        private CurveLoopType postLoop;
        private CurveLoopType preLoop;

        #endregion

        #region Public Properties

        /// <summary>
        /// Returns <c>true</c> if this curve is constant (has zero or one points); <c>false</c> otherwise.
        /// </summary>
        [DataMember]
        public bool IsConstant => keys.Count <= 1;

        /// <summary>
        /// Gets the collection of curve keys.
        /// </summary>
        [DataMember]
        public CurveKeyCollection Keys => keys;

        /// <summary>
        /// Gets or sets how to handle weighting values that are greater than the last control point in the curve.
        /// </summary>
        [DataMember]
        public CurveLoopType PostLoop {
            get => postLoop;
            set => postLoop = value;
        }

        /// <summary>
        /// Gets or sets how to handle weighting values that are less than the first control point in the curve.
        /// </summary>
        [DataMember]
        public CurveLoopType PreLoop {
            get => preLoop;
            set => preLoop = value;
        }

        #endregion

        #region Public Constructors

        /// <summary>
        /// Creates a new instance of <see cref="Curve"/> class.
        /// </summary>
        public Curve()
        {
            keys = new CurveKeyCollection();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Creates a copy of this curve.
        /// </summary>
        /// <returns>A copy of this curve.</returns>
        public Curve Clone()
        {
            Curve curve = new Curve {
                keys = keys.Clone(), 
                preLoop = preLoop, 
                postLoop = postLoop
            };
            return curve;
        }

        /// <summary>
        /// Evaluate the value at a position of this <see cref="Curve"/>.
        /// </summary>
        /// <param name="position">The position on this <see cref="Curve"/>.</param>
        /// <returns>Value at the position on this <see cref="Curve"/>.</returns>
        public float Evaluate(float position)
        {
            int keysCount = keys.Count;
            CurveKey[] array = keys.Keys.Array;

            CurveKey first = array[0];
            CurveKey last = array[keysCount - 1];

            if (position < first.Position) {
                switch (PreLoop) {
                    case CurveLoopType.Constant:
                        //constant
                        return first.Value;

                    case CurveLoopType.Linear:
                        // linear y = a*x +b with a tangeant of last point
                        return first.Value - first.TangentIn * (first.Position - position);

                    case CurveLoopType.Cycle:
                        //start -> end / start -> end
                        int cycle = GetNumberOfCycle(position);
                        float virtualPos = position - (cycle * (last.Position - first.Position));
                        return GetCurvePosition(virtualPos);

                    case CurveLoopType.CycleOffset:
                        //make the curve continue (with no step) so must up the curve each cycle of delta(value)
                        cycle = GetNumberOfCycle(position);
                        virtualPos = position - (cycle * (last.Position - first.Position));
                        return (GetCurvePosition(virtualPos) + cycle * (last.Value - first.Value));

                    case CurveLoopType.Oscillate:
                        //go back on curve from end and target start 
                        // start-> end / end -> start
                        cycle = GetNumberOfCycle(position);
                        if (0 == cycle % 2f)//if pair
                            virtualPos = position - (cycle * (last.Position - first.Position));
                        else
                            virtualPos = last.Position - position + first.Position + (cycle * (last.Position - first.Position));
                        return GetCurvePosition(virtualPos);
                }
            }
            else if (position > last.Position) {
                int cycle;
                switch (PostLoop) {
                    case CurveLoopType.Constant:
                        //constant
                        return last.Value;

                    case CurveLoopType.Linear:
                        // linear y = a*x +b with a tangeant of last point
                        return last.Value + first.TangentOut * (position - last.Position);

                    case CurveLoopType.Cycle:
                        //start -> end / start -> end
                        cycle = GetNumberOfCycle(position);
                        float virtualPos = position - (cycle * (last.Position - first.Position));
                        return GetCurvePosition(virtualPos);

                    case CurveLoopType.CycleOffset:
                        //make the curve continue (with no step) so must up the curve each cycle of delta(value)
                        cycle = GetNumberOfCycle(position);
                        virtualPos = position - (cycle * (last.Position - first.Position));
                        return (GetCurvePosition(virtualPos) + cycle * (last.Value - first.Value));

                    case CurveLoopType.Oscillate:
                        //go back on curve from end and target start 
                        // start-> end / end -> start
                        cycle = GetNumberOfCycle(position);
                        virtualPos = position - (cycle * (last.Position - first.Position));
                        if (0 == cycle % 2f)//if pair
                            virtualPos = position - (cycle * (last.Position - first.Position));
                        else
                            virtualPos = last.Position - position + first.Position + (cycle * (last.Position - first.Position));
                        return GetCurvePosition(virtualPos);
                }
            }

            //in curve
            return GetCurvePosition(position);
        }

        /// <summary>
        /// Computes tangents for all keys in the collection.
        /// </summary>
        /// <param name="tangentType">The tangent type for both in and out.</param>
		public void ComputeTangents(CurveTangent tangentType)
        {
            ComputeTangents(tangentType, tangentType);
        }

        /// <summary>
        /// Computes tangents for all keys in the collection.
        /// </summary>
        /// <param name="tangentInType">The tangent in-type. <see cref="CurveKey.TangentIn"/> for more details.</param>
        /// <param name="tangentOutType">The tangent out-type. <see cref="CurveKey.TangentOut"/> for more details.</param>
        public void ComputeTangents(CurveTangent tangentInType, CurveTangent tangentOutType)
        {
            int keysCount = keys.Count;
            for (var i = 0; i < keysCount; ++i) {
                ComputeTangent(i, tangentInType, tangentOutType);
            }
        }

        /// <summary>
        /// Computes tangent for the specific key in the collection.
        /// </summary>
        /// <param name="keyIndex">The index of a key in the collection.</param>
        /// <param name="tangentType">The tangent type for both in and out.</param>
        public void ComputeTangent(int keyIndex, CurveTangent tangentType)
        {
            ComputeTangent(keyIndex, tangentType, tangentType);
        }

        /// <summary>
        /// Computes tangent for the specific key in the collection.
        /// </summary>
        /// <param name="keyIndex">The index of key in the collection.</param>
        /// <param name="tangentInType">The tangent in-type. <see cref="CurveKey.TangentIn"/> for more details.</param>
        /// <param name="tangentOutType">The tangent out-type. <see cref="CurveKey.TangentOut"/> for more details.</param>
        public void ComputeTangent(int keyIndex, CurveTangent tangentInType, CurveTangent tangentOutType)
        {
            int keysCount = keys.Count;
            // See http://msdn.microsoft.com/en-us/library/microsoft.xna.framework.curvetangent.aspx

            CurveKey[] array = keys.Keys.Array;

            var key = array[keyIndex];

            float p0, p, p1;
            p0 = p = p1 = key.Position;

            float v0, v, v1;
            v0 = v = v1 = key.Value;

            if (keyIndex > 0) {
                p0 = array[keyIndex - 1].Position;
                v0 = array[keyIndex - 1].Value;
            }

            if (keyIndex < keysCount - 1) {
                p1 = array[keyIndex + 1].Position;
                v1 = array[keyIndex + 1].Value;
            }

            switch (tangentInType) {
                case CurveTangent.Flat:
                    key.TangentIn = 0;
                    break;
                case CurveTangent.Linear:
                    key.TangentIn = v - v0;
                    break;
                case CurveTangent.Smooth:
                    var pn = p1 - p0;
#if NETSTANDARD2_1
                    if (MathF.Abs(pn) < float.Epsilon)
#else
                    if (Math.Abs(pn) < float.Epsilon)
#endif
                        key.TangentIn = 0;
                    else
                        key.TangentIn = (v1 - v0) * ((p - p0) / pn);
                    break;
            }

            switch (tangentOutType) {
                case CurveTangent.Flat:
                    key.TangentOut = 0;
                    break;
                case CurveTangent.Linear:
                    key.TangentOut = v1 - v;
                    break;
                case CurveTangent.Smooth:
                    var pn = p1 - p0;
#if NETSTANDARD2_1
                    if (MathF.Abs(pn) < float.Epsilon)
#else
                    if (Math.Abs(pn) < float.Epsilon)
#endif
                        key.TangentOut = 0;
                    else
                        key.TangentOut = (v1 - v0) * ((p1 - p) / pn);
                    break;
            }
        }

        #endregion

        #region Private Methods

        private int GetNumberOfCycle(float position)
        {
            float cycle = (position - keys[0].Position) / (keys[keys.Count - 1].Position - keys[0].Position);
            if (cycle < 0f)
                cycle--;
            return (int)cycle;
        }

        private float GetCurvePosition(float position)
        {
            //only for position in curve
            int keysCount = keys.Count;
            CurveKey[] array = keys.Keys.Array;
            CurveKey prev = array[0];
            for (int i = 1; i < keysCount; ++i) {
                CurveKey next = array[i];
                if (next.Position >= position) {
                    if (prev.Continuity == CurveContinuity.Step) {
                        return position >= 1f ? next.Value : prev.Value;
                    }
                    float t = (position - prev.Position) / (next.Position - prev.Position);//to have t in [0,1]
                    float ts = t * t;
                    float tss = ts * t;
                    //After a lot of search on internet I have found all about spline function
                    // and bezier (phi'sss ancien) but finaly use hermite curve 
                    //http://en.wikipedia.org/wiki/Cubic_Hermite_spline
                    //P(t) = (2*t^3 - 3t^2 + 1)*P0 + (t^3 - 2t^2 + t)m0 + (-2t^3 + 3t^2)P1 + (t^3-t^2)m1
                    //with P0.value = prev.value , m0 = prev.tangentOut, P1= next.value, m1 = next.TangentIn
                    return (2 * tss - 3 * ts + 1f) * prev.Value + (tss - 2 * ts + t) * prev.TangentOut + (3 * ts - 2 * tss) * next.Value + (tss - ts) * next.TangentIn;
                }
                prev = next;
            }
            return 0f;
        }

        #endregion
    }

    public enum CurveLoopType
    {
        Constant,
        Cycle,
        CycleOffset,
        Oscillate,
        Linear
    }

    public enum CurveTangent
    {
        Flat,   // A Flat tangent always has a value equal to zero. 
        Linear, // A Linear tangent at a CurveKey is equal to the difference between its Value and the Value of the preceding or succeeding CurveKey.
        Smooth, // A Smooth tangent smooths the inflection between a TangentIn and TangentOut by taking into account the values of both neighbors of the CurveKey.
    }

    public enum CurveContinuity
    {
        Smooth,
        Step
    }

    /// <summary>
    /// Key point on the <see cref="Curve"/>.
    /// </summary>
    // TODO : [TypeConverter(typeof(ExpandableObjectConverter))]
    [DataContract]
    public class CurveKey : IEquatable<CurveKey>, IComparable<CurveKey>
    {
        #region Properties

        /// <summary>
        /// Gets or sets the indicator whether the segment between this point and the next point on the curve is discrete or continuous.
        /// </summary>
        [DataMember] public CurveContinuity Continuity;

        /// <summary>
        /// Gets a position of the key on the curve.
        /// </summary>
        [DataMember] public float Position;

        /// <summary>
        /// Gets or sets a tangent when approaching this point from the previous point on the curve.
        /// </summary>
        [DataMember] public float TangentIn;

        /// <summary>
        /// Gets or sets a tangent when leaving this point to the next point on the curve.
        /// </summary>
        [DataMember] public float TangentOut;

        /// <summary>
        /// Gets a value of this point.
        /// </summary>
        [DataMember] public float Value;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of <see cref="CurveKey"/> class with position: 0 and value: 0.
        /// </summary>
        public CurveKey() : this(0, 0) { }

        /// <summary>
        /// Creates a new instance of <see cref="CurveKey"/> class.
        /// </summary>
        /// <param name="position">Position on the curve.</param>
        /// <param name="value">Value of the control point.</param>
        public CurveKey(float position, float value)
            : this(position, value, 0, 0, CurveContinuity.Smooth) { }

        /// <summary>
        /// Creates a new instance of <see cref="CurveKey"/> class.
        /// </summary>
        /// <param name="position">Position on the curve.</param>
        /// <param name="value">Value of the control point.</param>
        /// <param name="tangentIn">Tangent approaching point from the previous point on the curve.</param>
        /// <param name="tangentOut">Tangent leaving point toward next point on the curve.</param>
        public CurveKey(float position, float value, float tangentIn, float tangentOut)
            : this(position, value, tangentIn, tangentOut, CurveContinuity.Smooth) { }

        /// <summary>
        /// Creates a new instance of <see cref="CurveKey"/> class.
        /// </summary>
        /// <param name="position">Position on the curve.</param>
        /// <param name="value">Value of the control point.</param>
        /// <param name="tangentIn">Tangent approaching point from the previous point on the curve.</param>
        /// <param name="tangentOut">Tangent leaving point toward next point on the curve.</param>
        /// <param name="continuity">Indicates whether the curve is discrete or continuous.</param>
        public CurveKey(float position, float value, float tangentIn, float tangentOut, CurveContinuity continuity)
        {
            Position = position;
            Value = value;
            TangentIn = tangentIn;
            TangentOut = tangentOut;
            Continuity = continuity;
        }

        #endregion

        /// <summary>
        /// 
        /// Compares whether two <see cref="CurveKey"/> instances are not equal.
        /// </summary>
        /// <param name="value1"><see cref="CurveKey"/> instance on the left of the not equal sign.</param>
        /// <param name="value2"><see cref="CurveKey"/> instance on the right of the not equal sign.</param>
        /// <returns><c>true</c> if the instances are not equal; <c>false</c> otherwise.</returns>	
        public static bool operator !=(CurveKey value1, CurveKey value2) => !(value1 == value2);

        /// <summary>
        /// Compares whether two <see cref="CurveKey"/> instances are equal.
        /// </summary>
        /// <param name="value1"><see cref="CurveKey"/> instance on the left of the equal sign.</param>
        /// <param name="value2"><see cref="CurveKey"/> instance on the right of the equal sign.</param>
        /// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
        public static bool operator ==(CurveKey value1, CurveKey value2)
        {
            if (Equals(value1, null))
                return Equals(value2, null);

            if (Equals(value2, null))
                return Equals(value1, null);

            return (value1.Position == value2.Position)
                && (value1.Value == value2.Value)
                && (value1.TangentIn == value2.TangentIn)
                && (value1.TangentOut == value2.TangentOut)
                && (value1.Continuity == value2.Continuity);
        }

        /// <summary>
        /// Creates a copy of this key.
        /// </summary>
        /// <returns>A copy of this key.</returns>
        public CurveKey Clone() => new CurveKey(Position, Value, TangentIn, TangentOut, Continuity);

        #region Inherited Methods

        public int CompareTo(CurveKey other) => Position.CompareTo(other.Position);

        public bool Equals(CurveKey other) => (this == other);

        public override bool Equals(object obj) => (obj as CurveKey) != null && Equals((CurveKey)obj);

        public override int GetHashCode() =>
            Position.GetHashCode() ^ Value.GetHashCode() ^ TangentIn.GetHashCode() ^
            TangentOut.GetHashCode() ^ Continuity.GetHashCode();

        #endregion
    }

    /// <summary>
    /// The collection of the <see cref="CurveKey"/> elements and a part of the <see cref="Curve"/> class.
    /// </summary>
    // TODO : [TypeConverter(typeof(ExpandableObjectConverter))]
    [DataContract]
    public class CurveKeyCollection : IEnumerable<CurveKey>
    {
        #region Private Fields

        public QuickList<CurveKey> Keys;

        #endregion

        #region Properties

        /// <summary>
        /// Indexer.
        /// </summary>
        /// <param name="index">The index of key in this collection.</param>
        /// <returns><see cref="CurveKey"/> at <paramref name="index"/> position.</returns>
        [DataMember(Name = "Items")]
        public CurveKey this[int index] {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Keys.Array[index];
            set {
                if (value == null)
                    throw new ArgumentNullException();
                if (index >= Keys.Count)
                    throw new IndexOutOfRangeException();

                if (Keys.Array[index].Position == value.Position) {
                    Keys.Array[index] = value;
                } else {
                    Keys.RemoveAt(index);
                    Keys.Add(value);
                }
            }
        }

        /// <summary>
        /// Returns the count of keys in this collection.
        /// </summary>
        [DataMember]
        public int Count {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Keys.Count;
        }

        /// <summary>
        /// Returns false because it is not a read-only collection.
        /// </summary>
        [DataMember]
        public bool IsReadOnly => false;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of <see cref="CurveKeyCollection"/> class.
        /// </summary>
        public CurveKeyCollection()
        {
            Keys = new QuickList<CurveKey>();
        }

        #endregion

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Keys.GetEnumerator();
        }

        public void Add(float position, float value)
        {
            Add(new CurveKey(position, value));
        }

        /// <summary>
        /// Adds a key to this collection.
        /// </summary>
        /// <param name="item">New key for the collection.</param>
        /// <exception cref="ArgumentNullException">Throws if <paramref name="item"/> is null.</exception>
        /// <remarks>The new key would be added respectively to a position of that key and the position of other keys.</remarks>
        public void Add(CurveKey item)
        {
            if (item == null)
                throw new ArgumentNullException("item");

            if (Keys.Count == 0) {
                Keys.Add(item);
                return;
            }

            for (int i = 0; i < Keys.Count; i++) {
                if (item.Position < Keys.Array[i].Position) {
                    Keys.Insert(i, item);
                    return;
                }
            }

            Keys.Add(item);
        }

        /// <summary>
        /// Removes all keys from this collection.
        /// </summary>
        public void Clear()
        {
            Keys.Clear();
        }

        /// <summary>
        /// Creates a copy of this collection.
        /// </summary>
        /// <returns>A copy of this collection.</returns>
        public CurveKeyCollection Clone()
        {
            CurveKeyCollection ckc = new CurveKeyCollection();
            foreach (CurveKey key in Keys)
                ckc.Add(key);
            return ckc;
        }

        /// <summary>
        /// Determines whether this collection contains a specific key.
        /// </summary>
        /// <param name="item">The key to locate in this collection.</param>
        /// <returns><c>true</c> if the key is found; <c>false</c> otherwise.</returns>
        public bool Contains(CurveKey item) => Keys.Contains(item);

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator for the <see cref="CurveKeyCollection"/>.</returns>
        public IEnumerator<CurveKey> GetEnumerator() => Keys.GetEnumerator();

        /// <summary>
        /// Finds element in the collection and returns its index.
        /// </summary>
        /// <param name="item">Element for the search.</param>
        /// <returns>Index of the element; or -1 if item is not found.</returns>
        public int IndexOf(CurveKey item) => Keys.IndexOf(item);

        /// <summary>
        /// Removes element at the specified index.
        /// </summary>
        /// <param name="index">The index which element will be removed.</param>
        public void RemoveAt(int index)
        {
            Keys.RemoveAt(index);
        }

        /// <summary>
        /// Removes specific element.
        /// </summary>
        /// <param name="item">The element</param>
        /// <returns><c>true</c> if item is successfully removed; <c>false</c> otherwise. This method also returns <c>false</c> if item was not found.</returns>
        public bool Remove(CurveKey item) => Keys.Remove(item);
    }
}
