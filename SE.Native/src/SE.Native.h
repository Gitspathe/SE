// SE.Native.h : Include file for standard system include files,
// or project specific include files.

#pragma once

#include <iostream>

// TODO: Reference additional headers your program requires here.

#ifndef TESTLIB_LIBRARY_H
#define TESTLIB_LIBRARY_H

#if defined DLL_EXPORTS
    #if defined WIN32
        #define LIB_API(RetType) extern "C" __declspec(dllexport) RetType
    #else
        #define LIB_API(RetType) extern "C" RetType __attribute__((visibility("default")))
    #endif
#else
    #if defined WIN32
        #define LIB_API(RetType) extern "C" __declspec(dllimport) RetType
    #else
        #define LIB_API(RetType) extern "C" RetType
    #endif
#endif

#endif //TESTLIB_LIBRARY_H