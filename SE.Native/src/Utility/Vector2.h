#pragma once

#ifndef VECTOR2_H
#define VECTOR2_H

namespace Utility {
	struct Vector2 {
	public:
		float x, y;

		Vector2(const float x = 0.0f, const float y = 0.0f) : x(x), y(y) { }

		const Vector2 operator+(const Vector2& other);
		const Vector2 operator-(const Vector2& other);
		const Vector2 operator*(const Vector2& other);
		const Vector2 operator/(const Vector2& other);
	};
}
#endif
