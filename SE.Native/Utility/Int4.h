#pragma once

#ifndef INT4_H
#define INT4_H

namespace Utility 
{

	struct Int4 {
	public:
		int x, y, z, w;

		Int4(const int x = 0.0f, const int y = 0.0f, const int z = 0.0f, const int w = 0.0f) : x(x), y(y), z(z), w(w) { }

		Int4 operator+(const Int4& other);
		Int4 operator-(const Int4& other);
		Int4 operator*(const Int4& other);
		Int4 operator/(const Int4& other);
	};
}

#endif