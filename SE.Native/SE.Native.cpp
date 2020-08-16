﻿// SE.Native.cpp : Defines the entry point for the application.
//

#include "SE.Native.h"
#include "Vector2.h"
#include <iostream>

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
