#include "NativeModule.h"
#include "NativeSaturationModule.h"
#include "ParticleMath.h"
#include "Particle.h"
#include "src/Utility.h"

namespace Particles {

	bool NativeSaturationModule::isRandom()
	{
		return transition == SaturationTransition::RandomLerp;
	}

	NativeSaturationModule::NativeSaturationModule(NativeModule* const parent) : NativeSubmodule(parent) { }

	void NativeSaturationModule::regenerateRandom()
	{
		if (!isRandom() || !isInitialized)
			return;

		delete[] rand;
		delete[] randEndSaturation;
		rand = new float[particlesLength];
		randEndSaturation = new float[particlesLength];
	}

	void NativeSaturationModule::onInitialize(const int32_t particleArrayLength)
	{
		particlesLength = particleArrayLength;
		startSaturationArr = new float[particleArrayLength];
		isInitialized = true;

		regenerateRandom();
	}

	void NativeSaturationModule::onParticlesActivated(const int32_t* const particleIndexArr, Particle* const particlesArrPtr, const int32_t length)
	{
		for (int32_t i = 0; i < length; i++) {
			int32_t pIndex = particleIndexArr[i];
			startSaturationArr[pIndex] = particlesArrPtr[pIndex].color.y;
			if (!isRandom())
				continue;

			rand[particleIndexArr[i]] = Random::range(0.0f, 1.0f);
			randEndSaturation[i] = ParticleMath::between(end1, end2, rand[i]);
		}
	}

	void NativeSaturationModule::onUpdate(const float deltaTime, Particle* const particleArrPtr, const int32_t length)
	{
		switch (transition) {
			case SaturationTransition::Lerp: {
				#pragma omp simd
				for (int32_t i = 0; i < length; i++) {
					Particle* particle = &particleArrPtr[i];
					particle->color.y = ParticleMath::lerp(startSaturationArr[i], end1, particle->timeAlive / particle->initialLife);
				}
			} break;
			case SaturationTransition::RandomLerp: {
				#pragma omp simd
				for (int32_t i = 0; i < length; i++) {
					Particle* particle = &particleArrPtr[i];
					particle->color.y = ParticleMath::lerp(startSaturationArr[i], randEndSaturation[i], particle->timeAlive / particle->initialLife);
				}
			} break;
			case SaturationTransition::Curve: {
				for (int32_t i = 0; i < length; i++) {
					Particle* particle = &particleArrPtr[i];
					particle->color.y = curve->Evaluate(particle->timeAlive / particle->initialLife);
				}
			} break;
			case SaturationTransition::None:
				break;
		}
	}

	const bool NativeSaturationModule::isValid()
	{
		return false; // ???
	}

	void NativeSaturationModule::setNone()
	{
		transition = SaturationTransition::None;
	}

	void NativeSaturationModule::setLerp(float end)
	{
		transition = SaturationTransition::Lerp;
		end1 = end;
	}

	void NativeSaturationModule::setRandomLerp(float min, float max)
	{
		if (min > max)
			ParticleMath::swap(&min, &max);

		transition = SaturationTransition::RandomLerp;
		end1 = min;
		end2 = max;
		regenerateRandom();
	}

	void NativeSaturationModule::setCurve(Curve* const curve)
	{
		transition = SaturationTransition::Curve;
		if (this->curve != nullptr)
			delete this->curve;

		this->curve = curve;
	}

	NativeSaturationModule::~NativeSaturationModule()
	{
		delete[] startSaturationArr;
		delete[] rand;
		delete[] randEndSaturation;
		delete curve;
	}

	LIB_API(NativeSaturationModule*) nativeModule_SaturationModule_Ctor(NativeModule* const modulePtr)
	{
		return new NativeSaturationModule(modulePtr);
	}

	LIB_API(void) nativeModule_SaturationModule_SetNone(NativeSaturationModule* const modulePtr)
	{
		modulePtr->setNone();
	}

	LIB_API(void) nativeModule_SaturationModule_SetLerp(NativeSaturationModule* const modulePtr, float end)
	{
		modulePtr->setLerp(end);
	}

	LIB_API(void) nativeModule_SaturationModule_SetRandomLerp(NativeSaturationModule* const modulePtr, float min, float max)
	{
		modulePtr->setRandomLerp(min, max);
	}

	LIB_API(void) nativeModule_SaturationModule_SetCurve(NativeSaturationModule* const modulePtr, Curve* const curvePtr)
	{
		modulePtr->setCurve(curvePtr);
	}

}