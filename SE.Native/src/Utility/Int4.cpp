#include "Int4.h"
namespace Utility 
{

	Int4 Int4::operator+(const Int4& other)
	{
		return Int4(x + other.x, y + other.y, z + other.z, w + other.w);
	}

	Int4 Int4::operator-(const Int4& other)
	{
		return Int4(x - other.x, y - other.y, z - other.z, w - other.w);
	}

	Int4 Int4::operator*(const Int4& other)
	{
		return Int4(x * other.x, y * other.y, z * other.z, w * other.w);
	}

	Int4 Int4::operator/(const Int4& other)
	{
		return Int4(x / other.x, y / other.y, z / other.z, w / other.w);
	}
}