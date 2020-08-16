#include "NativeModule.h"

namespace Particles 
{
	void NativeModule::Update(float deltaTime, Particle* particleArrPtr, int length)
	{
		for(int i = 0; i < length; i++)
		{
			Particle* p = particleArrPtr + i;
			p->InitialLife = 999999.0f;
		}
	}

	LIB_API(void) NativeModule_Update(NativeModule* modulePtr, float deltaTime, Particle* particleArrPtr, int length) 
	{
		modulePtr->Update(deltaTime, particleArrPtr, length);
	}

	LIB_API(NativeModule*) NativeModule_Create() 
	{
		NativeModule mod = NativeModule();
		return &mod;
	}
}