#pragma once

#ifndef PARTICLES_H
#define PARTICLES_H

#include "Utility.h"
#include "Particle.h"
#include "SE.Native.h"

namespace Particles 
{
	struct NativeModule {
	public:
		float end;

		void Update(float deltaTime, Particle* particleArrPtr, int length);
	};
}

#endif