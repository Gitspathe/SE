#pragma once

#ifndef NATIVEMODULE_H
#define NATIVEMODULE_H

#include "src/Utility.h"
#include "src/SE.Native.h"
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
		void onInitialize(int32_t particleArrayLength);
		void onParticlesActivated(int32_t* particleIndexArr, Particle* particlesArrPtr, const int32_t length);
		void onUpdate(float deltaTime, Particle* particleArrPtr, const int32_t length);

		~NativeModule();
	};
}

#endif