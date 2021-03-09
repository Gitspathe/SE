# CMakeList.txt : CMake project for SE.Native, include source and define
# project specific logic here.
#
cmake_minimum_required (VERSION 3.9)

project ("SE.Native")

set(CMAKE_CXX_STANDARD 17)

include_directories("${CMAKE_SOURCE_DIR}")

add_compile_definitions(DLL_EXPORTS)

# Add source to this project's executable.
add_library(SE.Native SHARED "src/SE.Native.cpp" "src/SE.Native.h" "src/Utility/Vector2.h" "src/Utility/Vector2.cpp" "src/Particles/Particle.cpp" "src/Particles/Particle.h" "src/Utility/Vector4.cpp" "src/Utility/Vector4.h" "src/Utility/Int4.cpp" "src/Utility/Int4.h" "src/Utility.h"  "src/Particles/NativeModule.h" "src/Particles/NativeModule.cpp"  "src/Particles/ParticleMath.h" "src/Particles/NativeSubmodule.h" "src/Particles/NativeSubmodule.cpp" "src/Particles/NativeAlphaModule.h" "src/Particles/NativeAlphaModule.cpp" "src/Utility/Random.h" "src/Utility/Random.cpp")

# TODO: Add tests and install targets if needed.
find_package(OpenMP)
if(OpenMP_CXX_FOUND)
    target_link_libraries(SE.Native PUBLIC OpenMP::OpenMP_CXX)
endif()
