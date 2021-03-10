#include "Curve.h"
#include "MathUtil.h"
#include <cmath>
#include "src/SE.Native.h"
using namespace Utility;

namespace Utility {
	const bool CurveKey::operator==(const CurveKey& other) 
	{
		return (position == other.position)
			&& (value == other.value)
			&& (tangentIn == other.tangentIn)
			&& (tangentOut == other.tangentOut)
			&& (continuity == other.continuity);
	}
	
	const bool CurveKey::operator!=(const CurveKey& other) 
	{
		return !(*this == other);
	}

	const int CurveKey::compareTo(const CurveKey& other)
	{
		return MathUtil::CompareTo(position, other.position);
	}

	size_t CurveKeyCollection::getCount()
	{
		return keys.size();
	}

	void CurveKeyCollection::add(const float position, const float value)
	{
		add(CurveKey(position, value));
	}

	void CurveKeyCollection::add(const CurveKey item)
	{
		if (getCount() <= 0) {
			keys.push_back(item);
			return;
		}

		size_t count = getCount();
		for (size_t i = 0; i < count; i++) {
			CurveKey* key = &(keys)[i];
			if (item.position < key->position) {
				keys.insert(keys.begin()+i, item);
				return;
			}
		}
		keys.push_back(item);
	}

	CurveKey& CurveKeyCollection::operator[](const size_t i)
	{
		return keys[i];
	}

	float Curve::Evaluate(const float position)
	{
		size_t keysCount = keys.getCount();
		CurveKey& first = keys[0];
		CurveKey& last = keys[keysCount - 1];

		if (position < first.position) {
			switch (preLoop) {
				case CurveLoopType::Constant: {
					//constant
					return first.value;
				} break;

				case CurveLoopType::Linear: {
					// linear y = a*x +b with a tangeant of last point
					return first.value - first.tangentIn * (first.position - position);
				} break;

				case CurveLoopType::Cycle: {
					//start -> end / start -> end
					size_t cycle = GetNumberOfCycle(position);
					float virtualPos = position - (cycle * (last.position - first.position));
					return GetCurvePosition(virtualPos);
				} break;

				case CurveLoopType::CycleOffset: {
					//make the curve continue (with no step) so must up the curve each cycle of delta(value)
					size_t cycle = GetNumberOfCycle(position);
					float virtualPos = position - (cycle * (last.position - first.position));
					return (GetCurvePosition(virtualPos) + cycle * (last.value - first.value));
				} break;

				case CurveLoopType::Oscillate: {
					//go back on curve from end and target start 
					// start-> end / end -> start
					size_t cycle = GetNumberOfCycle(position);
					float virtualPos;
					if (0 == std::fmod(cycle, 2.0f))//if pair
						virtualPos = position - (cycle * (last.position - first.position));
					else
						virtualPos = last.position - position + first.position + (cycle * (last.position - first.position));
					return GetCurvePosition(virtualPos);
				} break;
			}
		} else if(position > last.position) {
			switch (postLoop) {
				case CurveLoopType::Constant: {
					//constant
					return last.value;
				} break;

				case CurveLoopType::Linear: {
					// linear y = a*x +b with a tangent of last point
					return last.value + first.tangentOut * (position - last.position);
				} break;

				case CurveLoopType::Cycle: {
					//start -> end / start -> end
					size_t cycle = GetNumberOfCycle(position);
					float virtualPos = position - (cycle * (last.position - first.position));
					return GetCurvePosition(virtualPos);
				} break;

				case CurveLoopType::CycleOffset: {
					//make the curve continue (with no step) so must up the curve each cycle of delta(value)
					size_t cycle = GetNumberOfCycle(position);
					float virtualPos = position - (cycle * (last.position - first.position));
					return (GetCurvePosition(virtualPos) + cycle * (last.value - first.value));
				} break;
				case CurveLoopType::Oscillate: {
					//go back on curve from end and target start 
					// start-> end / end -> start
					size_t cycle = GetNumberOfCycle(position);
					float virtualPos = position - (cycle * (last.position - first.position));
					if (0 == std::fmod(cycle, 2.0f))//if pair
						virtualPos = position - (cycle * (last.position - first.position));
					else
						virtualPos = last.position - position + first.position + (cycle * (last.position - first.position));
					return GetCurvePosition(virtualPos);
				} break;
			}
		}

		//in curve
		return GetCurvePosition(position);
	}

	const size_t Curve::GetNumberOfCycle(const float position)
	{
		float cycle = (position - (keys[0]).position) / ((keys[keys.getCount() - 1]).position - (keys[0]).position);
		if (cycle < 0.0f)
			cycle--;
		return (size_t)cycle;
	}

	const float Curve::GetCurvePosition(const float position)
	{
		size_t keysCount = keys.getCount();
		CurveKey& prev = keys[0];
		for (size_t i = 1; i < keysCount; ++i) {
			CurveKey& next = keys[i];
			if (next.position >= position) {
				if (prev.continuity == CurveContinuity::Step) {
					return position >= 1.0f ? next.value : prev.value;
				}

				float t = (position - prev.position) / (next.position - prev.position);//to have t in [0,1]
				float ts = t * t;
				float tss = ts * t;

				//After a lot of search on internet I have found all about spline function
				// and bezier (phi'sss ancien) but finally use hermite curve 
				//http://en.wikipedia.org/wiki/Cubic_Hermite_spline
				//P(t) = (2*t^3 - 3t^2 + 1)*P0 + (t^3 - 2t^2 + t)m0 + (-2t^3 + 3t^2)P1 + (t^3-t^2)m1
				//with P0.value = prev.value , m0 = prev.tangentOut, P1= next.value, m1 = next.TangentIn
				return (2 * tss - 3 * ts + 1.0f) * prev.value + (tss - 2 * ts + t) * prev.tangentOut + (3 * ts - 2 * tss) * next.value + (tss - ts) * next.tangentIn;
			}
			prev = next;
		}
		return 0.0f;
	}

	void Curve::ComputeTangents(const CurveTangent::CurveTangent tangentType)
	{
		ComputeTangents(tangentType, tangentType);
	}

	void Curve::ComputeTangents(const CurveTangent::CurveTangent tangentInType, const CurveTangent::CurveTangent tangentOutType)
	{
		size_t keysCount = keys.getCount();
		for (size_t i = 0; i < keysCount; ++i) {
			ComputeTangent(i, tangentInType, tangentOutType);
		}
	}

	void Curve::ComputeTangent(const size_t keyIndex, const CurveTangent::CurveTangent tangentType)
	{
		ComputeTangent(keyIndex, tangentType, tangentType);
	}

	void Curve::ComputeTangent(const size_t keyIndex, const CurveTangent::CurveTangent tangentInType, const CurveTangent::CurveTangent tangentOutType)
	{
		size_t keysCount = keys.getCount();

		CurveKey& key = keys[keyIndex];
		float p0, p, p1;
		p0 = p = p1 = key.position;

		float v0, v, v1;
		v0 = v = v1 = key.value;

		if (keyIndex > 0) {
			CurveKey& prev = keys[keyIndex - 1];
			p0 = prev.position;
			v0 = prev.value;
		}

		if (keyIndex < keysCount - 1) {
			CurveKey& next = keys[keyIndex + 1];
			p1 = next.position;
			v1 = next.value;
		}

		switch (tangentInType) {
			case CurveTangent::Flat:
				key.tangentIn = 0;
				break;
			case CurveTangent::Linear:
				key.tangentIn = v - v0;
				break;
			case CurveTangent::Smooth:
				float pn = p1 - p0;
				if (abs(pn) < MathUtil::EPSILON) {
					key.tangentIn = 0;
				} else {
					key.tangentIn = (v1 - v0) * ((p - p0) / pn);
				}
				break;
		}

		switch (tangentOutType) {
			case CurveTangent::Flat:
				key.tangentOut = 0;
				break;
			case CurveTangent::Linear:
				key.tangentOut = v1 - v;
				break;
			case CurveTangent::Smooth:
				float pn = p1 - p0;
				if (abs(pn) < MathUtil::EPSILON) {
					key.tangentOut = 0;
				} else {
					key.tangentOut = (v1 - v0) * ((p1 - p) / pn);
				} break;
		}
	}

	LIB_API(Curve*) util_Curve_Ctor()
	{
		return new Curve();
	}

	LIB_API(void) util_Curve_Add(Curve* curvePtr, float position, float value)
	{
		curvePtr->keys.add(position, value);
	}
}