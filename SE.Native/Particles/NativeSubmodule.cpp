#include "NativeSubmodule.h"
#include "NativeModule.h"

namespace Particles {
	NativeSubmodule::NativeSubmodule(NativeModule* parent) : parent(parent)
	{
		parent->addSubmodule(this);
	}

	void NativeSubmodule::onInitialize(int particleArrayLength)	{ }
	void NativeSubmodule::onParticlesActivated(int* particleIndexArr, Particle* particlesArrPtr, int length) { }
	void NativeSubmodule::onUpdate(float deltaTime, Particle* particleArrPtr, const Particle* tail, const int length) { }
	bool NativeSubmodule::isValid() { return false; }
	
	NativeSubmodule::~NativeSubmodule()
	{
	}
}