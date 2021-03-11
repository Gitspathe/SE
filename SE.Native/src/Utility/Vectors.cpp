#include "src/Utility.h"

namespace Utility 
{

	Vector4 Vector4::operator+(const float& other)
	{
		return Vector4(x + other, y + other, z + other, w + other);
	}

	Vector4 Vector4::operator-(const float& other)
	{
		return Vector4(x - other, y - other, z - other, w - other);
	}

	Vector4 Vector4::operator*(const float& other)
	{
		return Vector4(x * other, y * other, z * other, w * other);
	}

	Vector4 Vector4::operator/(const float& other)
	{
		return Vector4(x / x, y / y, z / z, w / w);
	}

	Vector4 Vector4::operator+(const Vector4& other)
	{
		return Vector4(x + other.x, y + other.y, z + other.z, w + other.w);
	}

	Vector4 Vector4::operator-(const Vector4& other)
	{
		return Vector4(x - other.x, y - other.y, z - other.z, w - other.w);
	}

	Vector4 Vector4::operator*(const Vector4& other)
	{
		return Vector4(x * other.x, y * other.y, z * other.z, w * other.w);
	}

	Vector4 Vector4::operator/(const Vector4& other)
	{
		return Vector4(x / other.x, y / other.y, z / other.z, w / other.w);
	}

}