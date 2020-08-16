// SE.Native.cpp : Defines the entry point for the application.
//

#include "SE.Native.h"
#include <iostream>

using namespace Particles;

#if defined(_WIN32)
#define OS "Windows"
#elif defined(__linux__)
#define OS "Linux"
#elif defined(__APPLE__)
#define OS "MacOS"
#else
#define OS "Unknown OS"
#endif

LIB_API(float) hello(const Vector2 vector)
{
	return vector.x + vector.y;
}

LIB_API(void) particleTest(Particle* particleArrPtr, int len)
{
	for(int i = 0; i < len; i++)
	{
		Particle* p = particleArrPtr + i;
		p->InitialLife = 999999.0f;
	}
}
