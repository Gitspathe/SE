#pragma once

#ifndef VECTORS_H
#define VECTORS_H

namespace Utility
{
	struct Vector2 {
	public:
		float x, y;

		Vector2(const float x = 0.0f, const float y = 0.0f) : x(x), y(y) { }

		const Vector2 operator+(const Vector2& other);
		const Vector2 operator-(const Vector2& other);
		const Vector2 operator*(const Vector2& other);
		const Vector2 operator/(const Vector2& other);
	};

	struct Vector4 {
	public:
		float x, y, z, w;

#pragma omp declare simd
		Vector4(const float x = 0.0f, const float y = 0.0f, const float z = 0.0f, const float w = 0.0f) : x(x), y(y), z(z), w(w) { }

		Vector4 operator+(const float& other);
		Vector4 operator-(const float& other);
		Vector4 operator*(const float& other);
		Vector4 operator/(const float& other);

		Vector4 operator+(const Vector4& other);
		Vector4 operator-(const Vector4& other);
		Vector4 operator*(const Vector4& other);
		Vector4 operator/(const Vector4& other);

		static inline Vector4 lerp(Vector4& value1, Vector4& value2, float amount) {
			return (value1 * (1.0f - amount)) + (value2 * amount);
		}
	};

}

#endif