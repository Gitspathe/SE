#include "Random.h"
#include <iostream>
#include <chrono>

namespace Utility {

	static thread_local std::mt19937 generator;

	float Utility::Random::range(const float& min, const float& max) {
		std::uniform_real_distribution<float> distribution(min,max);
		return distribution(generator);
	}

}
