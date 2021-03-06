#include "NativeSubmodule.h"
#include "NativeModule.h"

// TODO: Move all of this code to SE.Particles.Native (from SE.Native.Particles), and include in SE.Particles repo.
namespace Particles {
	NativeSubmodule::NativeSubmodule(NativeModule* parent) : parent(parent)
	{
		parent->addSubmodule(this);
	}

	void NativeSubmodule::onInitialize(int particleArrayLength)	{ }
	void NativeSubmodule::onParticlesActivated(int* particleIndexArr, Particle* particlesArrPtr, int length) { }
	void NativeSubmodule::onUpdate(float deltaTime, Particle* __restrict particleArrPtr, const int length) { }
	bool NativeSubmodule::isValid() { return false; }
	
	NativeSubmodule::~NativeSubmodule()
	{
	}
}