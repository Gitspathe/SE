#include "NativeSubmodule.h"
#include "NativeModule.h"

namespace Particles {
	NativeSubmodule::NativeSubmodule(NativeModule* parent) : parent(parent)
	{
		parent->addSubmodule(this);
	}

	void NativeSubmodule::onInitialize(int32_t particleArrayLength)	{ }
	void NativeSubmodule::onParticlesActivated(int32_t* particleIndexArr, Particle* particlesArrPtr, const int32_t length) { }
	void NativeSubmodule::onUpdate(float deltaTime, Particle* __restrict particleArrPtr, const int32_t length) { }
	bool NativeSubmodule::isValid() { return false; }
	
	NativeSubmodule::~NativeSubmodule()
	{
	}
}