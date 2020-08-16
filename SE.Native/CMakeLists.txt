# CMakeList.txt : CMake project for SE.Native, include source and define
# project specific logic here.
#
cmake_minimum_required (VERSION 3.8)

project ("SE.Native")

set(CMAKE_CXX_STANDARD 17)

add_compile_definitions(DLL_EXPORTS)

# Add source to this project's executable.
add_library(SE.Native SHARED "SE.Native.cpp" "SE.Native.h" "Vector2.h" "Vector2.cpp")

# TODO: Add tests and install targets if needed.
