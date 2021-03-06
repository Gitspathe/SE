#pragma once

#ifndef PARTICLEMATH_H
#define PARTICLEMATH_H

namespace Particles {
	class ParticleMath {
	public:
		
		#pragma omp declare simd
		static inline const float lerp(const float value1, const float value2, const float amount) 
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
	};
}

#endif