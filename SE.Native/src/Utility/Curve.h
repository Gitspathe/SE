#pragma once

#ifndef CURVE_H
#define CURVE_H

#include <vector>
#include <src/Utility.h>

namespace Utility {
	namespace CurveLoopType 
	{
		enum CurveLoopType { Constant, Cycle, CycleOffset, Oscillate, Linear };
	}

	namespace CurveTangent
	{
		enum CurveTangent { Flat, Linear, Smooth, };
	}

	namespace CurveContinuity
	{
		enum CurveContinuity { Smooth, Step };
	}

	struct CurveKey {
	public:
		float position;
		float value;
		float tangentIn;
		float tangentOut;
		CurveContinuity::CurveContinuity continuity = CurveContinuity::Smooth;

		CurveKey(const float position, const float value) 
			: CurveKey(position, value, 0, 0, CurveContinuity::Smooth) { }

		CurveKey(const float position, const float value, const float tangentIn, const float tangentOut) 
			: CurveKey(position, value, tangentIn, tangentOut, CurveContinuity::Smooth) { }

		CurveKey(const float position, const float value, const float tangentIn, const float tangentOut, const CurveContinuity::CurveContinuity continuity)
			: position(position), value(value), tangentIn(tangentIn), tangentOut(tangentOut), continuity(continuity) { }

		const bool operator==(const CurveKey& other);
		const bool operator!=(const CurveKey& other);

		const int compareTo(const CurveKey& other);
	};

	struct CurveKeyCollection {
	public:
		std::vector<CurveKey> keys = std::vector<CurveKey>();
		size_t count = 0;
		void add(const float position, const float value);
		void add(const CurveKey item);
		CurveKey& operator [] (const size_t j);
	};


	struct Curve {
	private:
		CurveLoopType::CurveLoopType postLoop = CurveLoopType::Constant;
		CurveLoopType::CurveLoopType preLoop = CurveLoopType::Constant;
		const size_t GetNumberOfCycle(const float position);
		const float GetCurvePosition(const float position);
	public:
		CurveKeyCollection keys;
		float Evaluate(const float position);
		void ComputeTangents(const CurveTangent::CurveTangent tangentType);
		void ComputeTangents(const CurveTangent::CurveTangent tangentInType, const CurveTangent::CurveTangent tangentOutType);
		void ComputeTangent(const size_t keyIndex, const CurveTangent::CurveTangent tangentType);
		void ComputeTangent(const size_t keyIndex, const CurveTangent::CurveTangent tangentInType, const CurveTangent::CurveTangent tangentOutType);
	};

	struct Curve2 {
	public:
		Curve x, y;
		const Vector2 Evaluate(const float position);
		Curve2(Curve x, Curve y) : x(x), y(y) { }
	};

	struct Curve3 {
	public:
		Curve x, y, z;
		Curve3(Curve x, Curve y, Curve z) : x(x), y(y), z(z) { }
	};

	struct Curve4 {
	public:
		Curve x, y, z, w;
		const Vector4 Evaluate(const float position);
		Curve4(Curve x, Curve y, Curve z, Curve w) : x(x), y(y), z(z), w(w) { }
	};
}

#endif