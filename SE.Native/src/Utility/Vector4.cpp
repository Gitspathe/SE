#include "Vector4.h"
namespace Utility 
{

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