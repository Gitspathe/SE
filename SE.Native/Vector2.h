#pragma once

#ifndef VECTOR2_H
#define VECTOR2_H

struct Vector2 {
public:
	float x, y;

	Vector2(const float x = 0.0f, const float y = 0.0f) : x(x), y(y) { }

	Vector2 operator+(const Vector2& other);
	Vector2 operator-(const Vector2& other);
	Vector2 operator*(const Vector2& other);
	Vector2 operator/(const Vector2& other);
};

#endif
