#include "Vector2.h"
namespace Utility {
	Vector2 Vector2::operator+(const Vector2& other)
	{
		return Vector2(x + other.x, y + other.y);
	}

	Vector2 Vector2::operator-(const Vector2& other)
	{
		return Vector2(x - other.x, y - other.y);
	}

	Vector2 Vector2::operator*(const Vector2& other)
	{
		return Vector2(x * other.x, y * other.y);
	}

	Vector2 Vector2::operator/(const Vector2& other)
	{
		return Vector2(x / other.x, y / other.y);
	}
}