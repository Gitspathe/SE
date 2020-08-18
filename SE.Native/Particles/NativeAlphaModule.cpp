#include "NativeModule.h"
#include "NativeAlphaModule.h"
#include "ParticleMath.h"

namespace Particles {

	NativeAlphaModule::NativeAlphaModule(NativeModule* parent) : NativeSubmodule(parent) { }

	void NativeAlphaModule::onInitialize(int particleArrayLength)
	{
		startAlphasArr = new float[particleArrayLength];
	}

	void NativeAlphaModule::onParticlesActivated(int* particleIndexArr, Particle* particlesArrPtr, int length)
	{
		for(int i = 0; i < length; i++) {
			int pIndex = particleIndexArr[i];
			startAlphasArr[pIndex] = particlesArrPtr[pIndex].Color.w;
		}
	}

	void NativeAlphaModule::onUpdate(float deltaTime, Particle* particleArrPtr, int length)
	{
		Particle* tail = particleArrPtr + length;
		int i = 0;

		switch(transition) {
			case LERP: {
				for(Particle* particle = particleArrPtr; particle < tail; particle++, i++) {
					particle->Color.w = ParticleMath::Lerp(startAlphasArr[i], end1, particle->TimeAlive / particle->InitialLife);
				}
			} break;
			case RANDOM_LERP: {
		
			} break;
		}
	}

	bool NativeAlphaModule::isValid()
	{
		return false;
	}

	void NativeAlphaModule::setNone()
	{
		transition = NONE;
	}

	void NativeAlphaModule::setLerp(float end)
	{
		transition = LERP;
		end1 = end;
	}

	void NativeAlphaModule::setRandomLerp(float min, float max)
	{
		transition = RANDOM_LERP;
	}

	NativeAlphaModule::~NativeAlphaModule()
	{
		delete[] startAlphasArr;
	}

	LIB_API(NativeAlphaModule*) nativeModule_AlphaModule_Ctor(NativeModule* modulePtr)
	{
		return new NativeAlphaModule(modulePtr);
	}

	LIB_API(void) nativeModule_AlphaModule_SetNone(NativeAlphaModule* modulePtr)
	{
		modulePtr->setNone();
	}

	LIB_API(void) nativeModule_AlphaModule_SetLerp(NativeAlphaModule* modulePtr, float end)
	{
		modulePtr->setLerp(end);
	}

	LIB_API(void) nativeModule_AlphaModule_SetRandomLerp(NativeAlphaModule* modulePtr, float min, float max)
	{
		modulePtr->setRandomLerp(min, max);
	}

}