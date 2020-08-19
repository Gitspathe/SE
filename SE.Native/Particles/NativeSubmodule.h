#pragma once

#ifndef NATIVESUBMODULE_H
#define NATIVESUBMODULE_H

#include "Particle.h"

namespace Particles {
	class NativeModule;
	class NativeSubmodule {
	protected:
		NativeModule* parent;
		bool isInitialized = false;

	public:
		NativeSubmodule(NativeModule* parent);

		virtual void onInitialize(int particleArrayLength);
		virtual void onParticlesActivated(int* particleIndexArr, Particle* particlesArrPtr, int length);
		virtual void onUpdate(float deltaTime, Particle* particleArrPtr, const Particle* tail, const int length);
		virtual bool isValid();

		virtual ~NativeSubmodule();
	};
}

#endif