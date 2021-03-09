#pragma once

#ifndef UTILITYRANDOM_58927_H
#define UTILITYRANDOM_58927_H

#include <random>

namespace Utility {
	class Random {
	public:
		static const float range(const float& min = 0.0f, const float& max = 0.0f);
	};
}

#endif