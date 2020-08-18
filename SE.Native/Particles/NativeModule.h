#pragma once

#ifndef NATIVEMODULE_H
#define NATIVEMODULE_H

#include "Utility.h"
#include "SE.Native.h"
#include "Particle.h"
#include <vector>

namespace Particles 
{
	struct NativeSubmodule;
	struct NativeModule {
	public:
		std::vector<NativeSubmodule*>* submodules;

		NativeModule();

		void addSubmodule(NativeSubmodule* submodule);
		void removeSubmodule(NativeSubmodule* submodule);
		void onInitialize(int particleArrayLength);
		void onParticlesActivated(int* particleIndexArr, Particle* particlesArrPtr, int length);
		void onUpdate(float deltaTime, Particle* particleArrPtr, int length);

		~NativeModule();
	};
}

#endif