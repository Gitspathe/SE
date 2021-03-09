#pragma once

#ifndef INT4_H
#define INT4_H

#include <stdint.h>

namespace Utility 
{
	struct Int4 {
	public:
		int32_t x, y, z, w;

		Int4(const int32_t x = 0, const int32_t y = 0, const int32_t z = 0, const int32_t w = 0) : x(x), y(y), z(z), w(w) { }

		const Int4 operator+(const Int4& other);
		const Int4 operator-(const Int4& other);
		const Int4 operator*(const Int4& other);
		const Int4 operator/(const Int4& other);
	};
}

#endif