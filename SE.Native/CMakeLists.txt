# CMakeList.txt : CMake project for SE.Native, include source and define
# project specific logic here.
#
cmake_minimum_required (VERSION 3.8)

project ("SE.Native")

set(CMAKE_CXX_STANDARD 17)

include_directories("${CMAKE_SOURCE_DIR}")

add_compile_definitions(DLL_EXPORTS)

# Add source to this project's executable.
add_library(SE.Native SHARED "SE.Native.cpp" "SE.Native.h" "Utility/Vector2.h" "Utility/Vector2.cpp" "Particles/Particle.cpp" "Particles/Particle.h" "Utility/Vector4.cpp" "Utility/Vector4.h" "Utility/Int4.cpp" "Utility/Int4.h" "Utility.h" "Particles.h" "Particles/NativeModule.h" "Particles/NativeModule.cpp" "Particles/ParticleMath.cpp" "Particles/ParticleMath.h")

# TODO: Add tests and install targets if needed.
