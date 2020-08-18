#pragma once

#ifndef PARTICLEMATH_H
#define PARTICLEMATH_H

namespace Particles {
	class ParticleMath {
	public:
		static float lerp(float value1, float value2, float amount);
		static float between(float min, float max, float ratio);
		static void swap(float* x, float* y);
	};
}

#endif