#include "NativeModule.h"
#include "NativeAlphaModule.h"
#include "ParticleMath.h"
#include "Particle.h"
#include "Utility/Random.h"

namespace Particles {

	bool NativeAlphaModule::isRandom()
	{
		return transition == RANDOM_LERP;
	}

	NativeAlphaModule::NativeAlphaModule(NativeModule* parent) : NativeSubmodule(parent) { }

	void NativeAlphaModule::regenerateRandom()
	{
		if(!isRandom() || !isInitialized)
			return;

		if(rand == NULL) {
			delete[] rand;
			delete[] randEndAlphas;
		}

		rand = new float[particlesLength];
		randEndAlphas = new float[particlesLength];
	}

	void NativeAlphaModule::onInitialize(int particleArrayLength)
	{
		particlesLength = particleArrayLength;
		startAlphasArr = new float[particleArrayLength];
		isInitialized = true;

		regenerateRandom();
	}

	void NativeAlphaModule::onParticlesActivated(int* particleIndexArr, Particle* particlesArrPtr, int length)
	{
		for(int i = 0; i < length; i++) {
			int pIndex = particleIndexArr[i];
			startAlphasArr[pIndex] = particlesArrPtr[pIndex].Color.w;
			if (!isRandom()) 
				continue;

			rand[particleIndexArr[i]] = Random::range(0.0f, 1.0f);
            randEndAlphas[i] = ParticleMath::between(end1, end2, rand[i]);
		}
	}

	void NativeAlphaModule::onUpdate(float deltaTime, Particle* particleArrPtr, int length)
	{
		const Particle* tail = particleArrPtr + length;
		int i = 0;

		switch(transition) {
			case LERP: {
				for(Particle* particle = particleArrPtr; particle < tail; particle++, i++) {
					particle->Color.w = ParticleMath::lerp(startAlphasArr[i], end1, particle->TimeAlive / particle->InitialLife);
				}
			} break;
			case RANDOM_LERP: {
				for(Particle* particle = particleArrPtr; particle < tail; particle++, i++) {
					particle->Color.w = ParticleMath::lerp(startAlphasArr[i], randEndAlphas[i], particle->TimeAlive / particle->InitialLife);
				}
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
		if (min > max)
			ParticleMath::swap(&min, &max);

		transition = RANDOM_LERP;
		end1 = min;
        end2 = max;
		regenerateRandom();
	}

	NativeAlphaModule::~NativeAlphaModule()
	{
		delete[] startAlphasArr;
		delete[] rand;
		delete[] randEndAlphas;
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