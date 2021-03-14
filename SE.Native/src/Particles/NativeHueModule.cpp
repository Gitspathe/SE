#include "NativeHueModule.h"
#include "ParticleMath.h"
#include "Particle.h"
#include "src/Utility/Random.h"

namespace Particles {

	bool NativeHueModule::isRandom()
	{
		return transition == HueTransition::RandomLerp;
	}

	NativeHueModule::NativeHueModule() : NativeSubmodule() { }

	void NativeHueModule::regenerateRandom()
	{
		if (!isRandom() || !isInitialized)
			return;

		delete[] randEndHues;
		randEndHues = new float[particlesLength];
	}

	void NativeHueModule::onInitialize(const int32_t particleArrayLength)
	{
		particlesLength = particleArrayLength;
		startHuesArr = new float[particlesLength];
		isInitialized = true;

		regenerateRandom();
	}

	void NativeHueModule::onParticlesActivated(const int32_t* const particleIndexArr, Particle* const particlesArrPtr, const int32_t length)
	{
		for (int32_t i = 0; i < length; i++) {
			int32_t pIndex = particleIndexArr[i];
			Particle* particle = &particlesArrPtr[pIndex];
			startHuesArr[particle->id] = particle->color.x;
			if (!isRandom())
				continue;

			randEndHues[particle->id] = ParticleMath::between(end1, end2, Random::range(0.0f, 1.0f));
		}
	}

	void NativeHueModule::onUpdate(const float deltaTime, Particle* const particleArrPtr, const int32_t length)
	{
		switch (transition) {
			case HueTransition::Lerp: {
				#pragma omp simd
				for (int32_t i = 0; i < length; i++) {
					Particle* particle = &particleArrPtr[i];
					particle->color.x = ParticleMath::lerp(startHuesArr[particle->id], end1, particle->timeAlive / particle->initialLife);
				}
			} break;
			case HueTransition::RandomLerp: {
				#pragma omp simd
				for (int32_t i = 0; i < length; i++) {
					Particle* particle = &particleArrPtr[i];
					particle->color.x = ParticleMath::lerp(startHuesArr[particle->id], randEndHues[particle->id], particle->timeAlive / particle->initialLife);
				}
			} break;
			case HueTransition::Curve: {
				for (int32_t i = 0; i < length; i++) {
					Particle* particle = &particleArrPtr[i];
					particle->color.x = curve->Evaluate(particle->timeAlive / particle->initialLife);
				}
			} break;
			case HueTransition::None:
				break;
		}
	}

	const bool NativeHueModule::isValid()
	{
		return false; // ???
	}

	void NativeHueModule::setNone()
	{
		transition = HueTransition::None;
	}

	void NativeHueModule::setLerp(float end)
	{
		transition = HueTransition::Lerp;
		end1 = end;
	}

	void NativeHueModule::setRandomLerp(float min, float max)
	{
		if (min > max)
			ParticleMath::swap(&min, &max);

		transition = HueTransition::RandomLerp;
		end1 = min;
		end2 = max;
		regenerateRandom();
	}

	void NativeHueModule::setCurve(Curve* const curve)
	{
		transition = HueTransition::Curve;
		if (this->curve != nullptr)
			delete this->curve;

		this->curve = curve;
	}

	NativeHueModule::~NativeHueModule()
	{
		delete[] startHuesArr;
		delete[] randEndHues;
		delete curve;
	}

	LIB_API(NativeHueModule*) nativeModule_HueModule_Ctor()
	{
		return new NativeHueModule();
	}

	LIB_API(void) nativeModule_HueModule_SetNone(NativeHueModule* const modulePtr)
	{
		modulePtr->setNone();
	}

	LIB_API(void) nativeModule_HueModule_SetLerp(NativeHueModule* const modulePtr, float end)
	{
		modulePtr->setLerp(end);
	}

	LIB_API(void) nativeModule_HueModule_SetRandomLerp(NativeHueModule* const modulePtr, float min, float max)
	{
		modulePtr->setRandomLerp(min, max);
	}

	LIB_API(void) nativeModule_HueModule_SetCurve(NativeHueModule* const modulePtr, Curve* const curvePtr)
	{
		modulePtr->setCurve(curvePtr);
	}

}