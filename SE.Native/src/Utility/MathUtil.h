#pragma once

#ifndef UTILITY_MATHUTIL_H
#define UTILITY_MATHUTIL_H

namespace Utility { namespace MathUtil {

	const static inline int CompareTo(const float from, const float to)
	{
		if (from < to) return -1;
		if (from > to) return 1;
		if (from == to) return 0;

		return 0; // (What about NaN?)
	}

	const float EPSILON = 1.4e-45;

}}

#endif