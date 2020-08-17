#include "NativeModule.h"

namespace Particles 
{
	NativeModule::NativeModule()
	{ 
		alphaModule = new AlphaModule();
	}

	void NativeModule::initialize(int particleArrayLength) 
	{
		alphaModule->initialize(particleArrayLength);
	}

	void NativeModule::update(float deltaTime, Particle* particleArrPtr, int length)
	{
		if(alphaModule->isValid()){
			alphaModule->update(deltaTime, particleArrPtr, length);
		}
	}

	NativeModule::~NativeModule()
	{
		delete alphaModule;
	}

	#pragma region SUBMODULES - ALPHA

	AlphaModule::AlphaModule() { }

	void AlphaModule::initialize(int particleArrayLength) 
	{
		startAlphasArr = new float[particleArrayLength];
	}

	void AlphaModule::setNone()
	{
		transition = NONE;
	}

	void AlphaModule::setLerp(float end)
	{
		transition = LERP;
	}

	void AlphaModule::setRandomLerp(float min, float max)
	{
		transition = RANDOM_LERP;
	}

	void AlphaModule::update(float deltaTime, Particle* particleArrPtr, int length)
	{
		for(int i = 0; i < length; i++){
			Particle* p = particleArrPtr + i;
			p->InitialLife = 99999.0f;
		}
	}

	bool AlphaModule::isValid()
	{
		return transition != NONE;
	}

	AlphaModule::~AlphaModule()
	{
		delete[] startAlphasArr;
	}

	#pragma endregion


	#pragma region INTEROP METHODS.
	
	LIB_API(void) nativeModule_Update(NativeModule* modulePtr, float deltaTime, Particle* particleArrPtr, int length) 
	{
		modulePtr->update(deltaTime, particleArrPtr, length);
	}

	LIB_API(NativeModule*) nativeModule_Create() 
	{
		NativeModule *mod = new NativeModule();
		return mod;
	}

	LIB_API(void) nativeModule_Initialize(NativeModule* modulePtr, int particleArrayLength) 
	{
		modulePtr->initialize(particleArrayLength);
	}

	LIB_API(void) nativeModule_Delete(NativeModule* modulePtr) 
	{
		delete modulePtr;
	}

	LIB_API(void) nativeModule_AlphaModule_SetNone(NativeModule* modulePtr)
	{
		modulePtr->alphaModule->setNone();
	}

	LIB_API(void) nativeModule_AlphaModule_SetLerp(NativeModule* modulePtr, float end)
	{
		modulePtr->alphaModule->setLerp(end);
	}

	LIB_API(void) nativeModule_AlphaModule_SetRandomLerp(NativeModule* modulePtr, float min, float max)
	{
		modulePtr->alphaModule->setRandomLerp(min, max);
	}

	#pragma endregion


}