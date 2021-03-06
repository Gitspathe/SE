#pragma once

#ifndef VECTOR4_H
#define VECTOR4_H

namespace Utility 
{

	struct Vector4 {
	public:
		float x, y, z, w;

		Vector4(const float x = 0.0f, const float y = 0.0f, const float z = 0.0f, const float w = 0.0f) : x(x), y(y), z(z), w(w) { }

		Vector4 operator+(const Vector4& other);
		Vector4 operator-(const Vector4& other);
		Vector4 operator*(const Vector4& other);
		Vector4 operator/(const Vector4& other);
	};

}
#endif
