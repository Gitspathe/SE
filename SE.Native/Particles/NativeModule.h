#pragma once

#ifndef PARTICLES_H
#define PARTICLES_H

#include "Utility.h"
#include "Particle.h"
#include "SE.Native.h"

namespace Particles 
{
	struct AlphaModule {
	private :
		enum Transition { NONE, LERP, RANDOM_LERP };
		
		Transition transition = NONE;
		float end1, end2;
		float* startAlphasArr;

	public:
		AlphaModule();

		void initialize(int particleArrayLength);
		void onParticlesActivated(int* particleIndexArr, Particle* particlesArrPtr, int length);
		void update(float deltaTime, Particle* particleArrPtr, int length);
		void setNone();
		void setLerp(float end);
		void setRandomLerp(float min, float max);
		bool isValid();

		~AlphaModule();
	};


	struct NativeModule {
	public:
		AlphaModule* alphaModule;

		NativeModule();

		void initialize(int particleArrayLength);
		void onParticlesActivated(int* particleIndexArr, Particle* particlesArrPtr, int length);
		void update(float deltaTime, Particle* particleArrPtr, int length);

		~NativeModule();
	};
}

#endif