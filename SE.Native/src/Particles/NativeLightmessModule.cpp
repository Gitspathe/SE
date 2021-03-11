#include "NativeModule.h"
#include "NativeLightnessModule.h"
#include "ParticleMath.h"
#include "Particle.h"
#include "src/Utility/Random.h"

namespace Particles {

	bool NativeLightnessModule::isRandom()
	{
		return transition == LightnessTransition::RandomLerp;
	}

	NativeLightnessModule::NativeLightnessModule(NativeModule* const parent) : NativeSubmodule(parent) { }

	void NativeLightnessModule::regenerateRandom()
	{
		if (!isRandom() || !isInitialized)
			return;

		delete[] rand;
		delete[] randEndLightness;
		rand = new float[particlesLength];
		randEndLightness = new float[particlesLength];
	}

	void NativeLightnessModule::onInitialize(const int32_t particleArrayLength)
	{
		particlesLength = particleArrayLength;
		startLightnessArr = new float[particleArrayLength];
		isInitialized = true;

		regenerateRandom();
	}

	void NativeLightnessModule::onParticlesActivated(const int32_t* const particleIndexArr, Particle* const particlesArrPtr, const int32_t length)
	{
		for (int32_t i = 0; i < length; i++) {
			int32_t pIndex = particleIndexArr[i];
			startLightnessArr[pIndex] = particlesArrPtr[pIndex].color.z;
			if (!isRandom())
				continue;

			rand[particleIndexArr[i]] = Random::range(0.0f, 1.0f);
			randEndLightness[i] = ParticleMath::between(end1, end2, rand[i]);
		}
	}

	void NativeLightnessModule::onUpdate(const float deltaTime, Particle* const particleArrPtr, const int32_t length)
	{
		switch (transition) {
			case LightnessTransition::Lerp: {
				#pragma omp simd
				for (int32_t i = 0; i < length; i++) {
					Particle* particle = &particleArrPtr[i];
					particle->color.z = ParticleMath::lerp(startLightnessArr[i], end1, particle->timeAlive / particle->initialLife);
				}
			} break;
			case LightnessTransition::RandomLerp: {
				#pragma omp simd
				for (int32_t i = 0; i < length; i++) {
					Particle* particle = &particleArrPtr[i];
					particle->color.z = ParticleMath::lerp(startLightnessArr[i], randEndLightness[i], particle->timeAlive / particle->initialLife);
				}
			} break;
			case LightnessTransition::Curve: {
				for (int32_t i = 0; i < length; i++) {
					Particle* particle = &particleArrPtr[i];
					particle->color.z = curve->Evaluate(particle->timeAlive / particle->initialLife);
				}
			} break;
			case LightnessTransition::None:
				break;
		}
	}

	const bool NativeLightnessModule::isValid()
	{
		return false; // ???
	}

	void NativeLightnessModule::setNone()
	{
		transition = LightnessTransition::None;
	}

	void NativeLightnessModule::setLerp(float end)
	{
		transition = LightnessTransition::Lerp;
		end1 = end;
	}

	void NativeLightnessModule::setRandomLerp(float min, float max)
	{
		if (min > max)
			ParticleMath::swap(&min, &max);

		transition = LightnessTransition::RandomLerp;
		end1 = min;
		end2 = max;
		regenerateRandom();
	}

	void NativeLightnessModule::setCurve(Curve* const curve)
	{
		transition = LightnessTransition::Curve;
		if (this->curve != nullptr)
			delete this->curve;

		this->curve = curve;
	}

	NativeLightnessModule::~NativeLightnessModule()
	{
		delete[] startLightnessArr;
		delete[] rand;
		delete[] randEndLightness;
		delete curve;
	}

	LIB_API(NativeLightnessModule*) nativeModule_LightnessModule_Ctor(NativeModule* const modulePtr)
	{
		return new NativeLightnessModule(modulePtr);
	}

	LIB_API(void) nativeModule_LightnessModule_SetNone(NativeLightnessModule* const modulePtr)
	{
		modulePtr->setNone();
	}

	LIB_API(void) nativeModule_LightnessModule_SetLerp(NativeLightnessModule* const modulePtr, float end)
	{
		modulePtr->setLerp(end);
	}

	LIB_API(void) nativeModule_LightnessModule_SetRandomLerp(NativeLightnessModule* const modulePtr, float min, float max)
	{
		modulePtr->setRandomLerp(min, max);
	}

	LIB_API(void) nativeModule_LightnessModule_SetCurve(NativeLightnessModule* const modulePtr, Curve* const curvePtr)
	{
		modulePtr->setCurve(curvePtr);
	}

}