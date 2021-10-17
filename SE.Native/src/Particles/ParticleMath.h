#pragma once

#ifndef PARTICLEMATH_H
#define PARTICLEMATH_H

#include <stdint.h>

namespace Particles {
	class ParticleMath {
	public:
		
		#pragma omp declare simd
		static inline const float lerp(const float value1, const float value2, const float amount) 
		{
			return value1 + (value2 - value1) * amount; 
		}

		#pragma omp declare simd
		static inline const uint8_t lerpByte(const uint8_t value1, const uint8_t value2, const float amount)
		{
			return value1 + (value2 - value1) * amount;
		}

		#pragma omp declare simd
		static inline const float between(const float min, const float max, const float ratio) 
		{ 
			return (min + ((max - min) * ratio)); 
		}
		
		#pragma omp declare simd
		static inline void swap(float* const x, float* const y) 
		{
			float tmpX = *x;
			*x = *y;
			*y = tmpX;
		}

		#pragma omp declare simd
		static inline const float clamp(const float val, const float min, const float max)
		{
			if (val < min)
				return min;
			if (val > max)
				return max;

			return val;
		}

		#pragma omp declare simd
		static inline const uint8_t clamp(const uint8_t val, const uint8_t min, const uint8_t max)
		{
			if (val < min)
				return min;
			if (val > max)
				return max;

			return val;
		}
	};
}

#endif