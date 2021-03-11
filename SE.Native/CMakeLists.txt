# CMakeList.txt : CMake project for SE.Native, include source and define
# project specific logic here.
#
cmake_minimum_required (VERSION 3.9)

project ("SE.Native")

set(CMAKE_CXX_STANDARD 11)

include_directories("${CMAKE_SOURCE_DIR}")

add_compile_definitions(DLL_EXPORTS)

# Add source to this project's executable.
add_library(SE.Native SHARED "src/SE.Native.cpp" "src/SE.Native.h"    "src/Particles/Particle.h"   "src/Utility/Int4.cpp" "src/Utility/Int4.h" "src/Utility.h"  "src/Particles/NativeModule.h" "src/Particles/NativeModule.cpp"  "src/Particles/ParticleMath.h" "src/Particles/NativeSubmodule.h" "src/Particles/NativeSubmodule.cpp" "src/Particles/NativeAlphaModule.h" "src/Particles/NativeAlphaModule.cpp" "src/Utility/Random.h" "src/Utility/Random.cpp" "src/Utility/Curve.h" "src/Utility/Curve.cpp" "src/Utility/MathUtil.h" "src/Particles/NativeHueModule.h" "src/Particles/NativeHueModule.cpp" "src/Particles/NativeLightnessModule.h" "src/Particles/NativeLightmessModule.cpp" "src/Particles/NativeSaturationModule.h" "src/Particles/NativeSaturationModule.cpp" "src/Particles/NativeColorModule.h" "src/Utility/Vectors.h" "src/Utility/Vectors.cpp" "src/Particles/NativeColorModule.cpp")

# Setup compiler flags.
if(CMAKE_BUILD_TYPE STREQUAL "Debug")
    # Debug mode.

else()
    # Release mode.

    if (CMAKE_CXX_COMPILER_ID STREQUAL "Clang")
        # Using Clang
        SET(CMAKE_CXX_FLAGS "-O2 -openmp-simd ${CMAKE_CXX_FLAGS}")

    elseif (CMAKE_CXX_COMPILER_ID STREQUAL "GNU") 
        # Using GCC
        SET(CMAKE_CXX_FLAGS "-O3 -fopenmp-simd ${CMAKE_CXX_FLAGS}")

    elseif (CMAKE_CXX_COMPILER_ID STREQUAL "Intel")
        # Using Intel C++

    elseif (CMAKE_CXX_COMPILER_ID STREQUAL "MSVC")
        # Using Visual Studio C++
        SET(CMAKE_CXX_FLAGS "/O2 /Oi /Ot ${CMAKE_CXX_FLAGS}")

    endif()

endif()


message("C++ compiler flags: ${CMAKE_CXX_FLAGS}")

# TODO: Add tests and install targets if needed.
find_package(OpenMP)
if(OpenMP_CXX_FOUND)
    target_link_libraries(SE.Native PUBLIC OpenMP::OpenMP_CXX)
endif()
