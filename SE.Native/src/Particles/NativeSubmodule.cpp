#include "NativeSubmodule.h"
#include "src/SE.Native.h"

namespace Particles {
	NativeSubmodule::NativeSubmodule(){}

	void NativeSubmodule::onInitialize(const int32_t particleArrayLength) { }
	void NativeSubmodule::onParticlesActivated(const int32_t* const particleIndexArr, Particle* const particlesArrPtr, const int32_t length) { }
	void NativeSubmodule::onUpdate(const float deltaTime, Particle* const __restrict particleArrPtr, const int32_t length) { }
	const bool NativeSubmodule::isValid() { return false; }
	
	NativeSubmodule::~NativeSubmodule()
	{
	}
}