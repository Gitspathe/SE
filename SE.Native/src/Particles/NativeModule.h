#pragma once

#ifndef NATIVEMODULE_H
#define NATIVEMODULE_H

#include "src/Utility.h"
#include "src/SE.Native.h"
#include "Particle.h"
#include <vector>

namespace Particles 
{
	class NativeSubmodule;
	class NativeModule {
	public:
		std::vector<NativeSubmodule*>* submodules;

		NativeModule();

		void addSubmodule(NativeSubmodule* const submodule);
		void removeSubmodule(NativeSubmodule* const submodule);
		void onInitialize(NativeSubmodule* const submodulePtr, const int32_t particleArrayLength);
		void onParticlesActivated(const int32_t* const particleIndexArr, Particle* const particlesArrPtr, const int32_t length);
		void onUpdate(const float deltaTime, Particle* const particleArrPtr, const int32_t length);

		~NativeModule();
	};
}

#endif