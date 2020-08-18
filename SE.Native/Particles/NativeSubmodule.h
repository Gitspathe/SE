#pragma once

#ifndef NATIVESUBMODULE_H
#define NATIVESUBMODULE_H

#include "Particle.h"

namespace Particles {
	class NativeModule;
	class NativeSubmodule {
	public:
		NativeSubmodule(NativeModule* parent);

		virtual void onInitialize(int particleArrayLength);
		virtual void onParticlesActivated(int* particleIndexArr, Particle* particlesArrPtr, int length);
		virtual void onUpdate(float deltaTime, Particle* particleArrPtr, int length);
		virtual bool isValid();

		~NativeSubmodule();
	};
}

#endif