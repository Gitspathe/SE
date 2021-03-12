#include "Random.h"
#include <iostream>
#include <chrono>

namespace Utility {

	const float Utility::Random::range(const float& min, const float& max) {
		static thread_local std::mt19937 generator;
		std::uniform_real_distribution<float> distribution(min,max);
		return distribution(generator);
	}

}
