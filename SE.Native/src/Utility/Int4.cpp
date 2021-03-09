#include "Int4.h"
namespace Utility 
{

	const Int4 Int4::operator+(const Int4& other)
	{
		return Int4(x + other.x, y + other.y, z + other.z, w + other.w);
	}

	const Int4 Int4::operator-(const Int4& other)
	{
		return Int4(x - other.x, y - other.y, z - other.z, w - other.w);
	}

	const Int4 Int4::operator*(const Int4& other)
	{
		return Int4(x * other.x, y * other.y, z * other.z, w * other.w);
	}

	const Int4 Int4::operator/(const Int4& other)
	{
		return Int4(x / other.x, y / other.y, z / other.z, w / other.w);
	}
}