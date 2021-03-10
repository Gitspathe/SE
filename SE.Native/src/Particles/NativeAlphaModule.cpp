#include "NativeModule.h"
#include "NativeAlphaModule.h"
#include "ParticleMath.h"
#include "Particle.h"
#include "src/Utility/Random.h"

namespace Particles {

	bool NativeAlphaModule::isRandom()
	{
		return transition == AlphaTransition::RandomLerp;
	}

	NativeAlphaModule::NativeAlphaModule(NativeModule* parent) : NativeSubmodule(parent) { }

	void NativeAlphaModule::regenerateRandom()
	{
		if(!isRandom() || !isInitialized)
			return;

		delete[] rand;
		delete[] randEndAlphas;
		rand = new float[particlesLength];
		randEndAlphas = new float[particlesLength];
	}

	void NativeAlphaModule::onInitialize(const int32_t particleArrayLength)
	{
		particlesLength = particleArrayLength;
		startAlphasArr = new float[particleArrayLength];
		isInitialized = true;

		regenerateRandom();
	}

	void NativeAlphaModule::onParticlesActivated(const int32_t* const particleIndexArr, Particle* const particlesArrPtr, const int32_t length)
	{
		for(int i = 0; i < length; i++) {
			int pIndex = particleIndexArr[i];
			startAlphasArr[pIndex] = particlesArrPtr[pIndex].color.w;
			if (!isRandom()) 
				continue;

			rand[particleIndexArr[i]] = Random::range(0.0f, 1.0f);
            randEndAlphas[i] = ParticleMath::between(end1, end2, rand[i]);
		}
	}

	void NativeAlphaModule::onUpdate(const float deltaTime, Particle* const __restrict particleArrPtr, const int32_t length)
	{
		switch(transition) {
			case AlphaTransition::Lerp: {
				for (int i = 0; i < length; i++) {
					Particle* particle = &particleArrPtr[i];
					particle->color.w = ParticleMath::lerp(startAlphasArr[i], end1, particle->timeAlive / particle->initialLife);
				}
			} break;
			case AlphaTransition::RandomLerp: {
				for (int i = 0; i < length; i++) {
					Particle* particle = &particleArrPtr[i];
					particle->color.w = ParticleMath::lerp(startAlphasArr[i], randEndAlphas[i], particle->timeAlive / particle->initialLife);
				}
			} break;
			case AlphaTransition::Curve: {
				for (int i = 0; i < length; i++) {
					Particle* particle = &particleArrPtr[i];
					particle->color.w = curve->Evaluate(particle->timeAlive / particle->initialLife);
				}
			} break;
			case AlphaTransition::None:
				break;
		}
	}

	const bool NativeAlphaModule::isValid()
	{
		return false; // ???
	}

	void NativeAlphaModule::setNone()
	{
		transition = AlphaTransition::None;
	}

	void NativeAlphaModule::setLerp(float end)
	{
		transition = AlphaTransition::Lerp;
		end1 = end;
	}

	void NativeAlphaModule::setRandomLerp(float min, float max)
	{
		if (min > max)
			ParticleMath::swap(&min, &max);

		transition = AlphaTransition::RandomLerp;
		end1 = min;
        end2 = max;
		regenerateRandom();
	}

	void NativeAlphaModule::setCurve(Curve* const curve)
	{
		transition = AlphaTransition::Curve;
		if (this->curve != nullptr)
			delete this->curve;

		this->curve = curve;
	}

	NativeAlphaModule::~NativeAlphaModule()
	{
		delete[] startAlphasArr;
		delete[] rand;
		delete[] randEndAlphas;
		delete curve;
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

	LIB_API(void) nativeModule_AlphaModule_SetCurve(NativeAlphaModule* modulePtr, Curve* const curvePtr)
	{
		modulePtr->setCurve(curvePtr);
	}
}