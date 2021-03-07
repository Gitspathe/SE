#pragma once

#ifndef PARTICLEMATH_H
#define PARTICLEMATH_H

namespace Particles {
	class ParticleMath {
	public:
		static const float lerp(const float value1, const float value2, const float amount);
		static const float between(const float min, const float max, const float ratio);
		static void swap(float* x, float* y);
	};
}

#endif