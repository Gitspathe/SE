#include "NativeAlphaModule.h"
#include "ParticleMath.h"
#include "Particle.h"
#include "src/Utility/Random.h"

namespace Particles {

	bool NativeAlphaModule::isRandom()
	{
		return transition == AlphaTransition::RandomLerp;
	}

	NativeAlphaModule::NativeAlphaModule() : NativeSubmodule() { }

	void NativeAlphaModule::regenerateRandom()
	{
		if(!isRandom() || !isInitialized)
			return;

		delete[] randEndAlphas;
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
		for(int32_t i = 0; i < length; i++) {
			int32_t pIndex = particleIndexArr[i];
			Particle* particle = &particlesArrPtr[pIndex];
			startAlphasArr[particle->id] = particle->color.w;
			if (!isRandom()) 
				continue;

            randEndAlphas[particle->id] = ParticleMath::between(end1, end2, Random::range(0.0f, 1.0f));
		}
	}

	void NativeAlphaModule::onUpdate(const float deltaTime, Particle* const particleArrPtr, const int32_t length)
	{
		switch(transition) {
			case AlphaTransition::Lerp: {
				#pragma omp simd
				for (int32_t i = 0; i < length; i++) {
					Particle* particle = &particleArrPtr[i];
					particle->color.w = ParticleMath::lerp(startAlphasArr[particle->id], end1, particle->timeAlive / particle->initialLife);
				}
			} break;
			case AlphaTransition::RandomLerp: {
				#pragma omp simd
				for (int32_t i = 0; i < length; i++) {
					Particle* particle = &particleArrPtr[i];
					particle->color.w = ParticleMath::lerp(startAlphasArr[particle->id], randEndAlphas[particle->id], particle->timeAlive / particle->initialLife);
				}
			} break;
			case AlphaTransition::Curve: {
				for (int32_t i = 0; i < length; i++) {
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
		delete[] randEndAlphas;
		delete curve;
	}

	LIB_API(NativeAlphaModule*) nativeModule_AlphaModule_Ctor()
	{
		return new NativeAlphaModule();
	}

	LIB_API(void) nativeModule_AlphaModule_SetNone(NativeAlphaModule* const modulePtr)
	{
		modulePtr->setNone();
	}

	LIB_API(void) nativeModule_AlphaModule_SetLerp(NativeAlphaModule* const modulePtr, float end)
	{
		modulePtr->setLerp(end);
	}

	LIB_API(void) nativeModule_AlphaModule_SetRandomLerp(NativeAlphaModule* const modulePtr, float min, float max)
	{
		modulePtr->setRandomLerp(min, max);
	}

	LIB_API(void) nativeModule_AlphaModule_SetCurve(NativeAlphaModule* const modulePtr, Curve* const curvePtr)
	{
		modulePtr->setCurve(curvePtr);
	}
}