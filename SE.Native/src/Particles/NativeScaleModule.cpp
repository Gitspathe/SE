#include "NativeModule.h"
#include "NativeScaleModule.h"
#include "ParticleMath.h"
#include "Particle.h"
#include "src/Utility.h"

namespace Particles {

	bool NativeScaleModule::isRandom()
	{
		return transition == ScaleTransition::RandomCurve;
	}

	NativeScaleModule::NativeScaleModule(NativeModule* const parent) : NativeSubmodule(parent) { }

	void NativeScaleModule::regenerateRandom()
	{
		if (!isRandom() || !isInitialized)
			return;

		delete[] rand;
		rand = new float[particlesLength];
	}

	void NativeScaleModule::onInitialize(const int32_t particleArrayLength)
	{
		particlesLength = particleArrayLength;
		startScalesArr = new Vector2[particleArrayLength];
		isInitialized = true;

		regenerateRandom();
	}

	void NativeScaleModule::onParticlesActivated(const int32_t* const particleIndexArr, Particle* const particlesArrPtr, const int32_t length)
	{
		for (int32_t i = 0; i < length; i++) {
			int32_t pIndex = particleIndexArr[i];
			startScalesArr[pIndex] = particlesArrPtr[pIndex].scale;
			if (!isRandom())
				continue;

			rand[particleIndexArr[i]] = Random::range(0.0f, 1.0f);
		}
	}

	void NativeScaleModule::onUpdate(const float deltaTime, Particle* const particleArrPtr, const int32_t length)
	{
		if (absoluteValue) {
			switch (transition) {
				case ScaleTransition::Lerp: {
					#pragma omp simd
					for (int32_t i = 0; i < length; i++) {
						Particle* particle = &particleArrPtr[i];
						const float scale = ParticleMath::between(start, end, particle->timeAlive / particle->initialLife);
						particle->scale.x = scale;
						particle->scale.y = scale;
					}
				} break;
				case ScaleTransition::Curve: {
					for (int32_t i = 0; i < length; i++) {
						Particle* particle = &particleArrPtr[i];
						const float scale = curve->Evaluate(particle->timeAlive / particle->initialLife);
						particle->scale.x = scale;
						particle->scale.y = scale;
					}
				} break;
				case ScaleTransition::RandomCurve: {
					for (int32_t i = 0; i < length; i++) {
						Particle* particle = &particleArrPtr[i];
						const float scale = curve->Evaluate(particle->timeAlive / particle->initialLife);
						particle->scale.x = scale;
						particle->scale.y = scale;
					}
				} break;
				case ScaleTransition::None:
					break;
			}
		} else {
			switch (transition) {
				case ScaleTransition::Lerp: {
					#pragma omp simd
					for (int32_t i = 0; i < length; i++) {
						Particle* particle = &particleArrPtr[i];
						const float scale = ParticleMath::between(start, end, particle->timeAlive / particle->initialLife);
						particle->scale.x = scale * startScalesArr[i].x;
						particle->scale.y = scale * startScalesArr[i].y;
					}
				} break;
				case ScaleTransition::Curve: {
					for (int32_t i = 0; i < length; i++) {
						Particle* particle = &particleArrPtr[i];
						const float scale = curve->Evaluate(particle->timeAlive / particle->initialLife);
						particle->scale.x = scale * startScalesArr[i].x;
						particle->scale.y = scale * startScalesArr[i].y;
					}
				} break;
				case ScaleTransition::RandomCurve: {
					for (int32_t i = 0; i < length; i++) {
						Particle* particle = &particleArrPtr[i];
						const float scale = curve->Evaluate(particle->timeAlive / particle->initialLife);
						particle->scale.x = scale * startScalesArr[i].x;
						particle->scale.y = scale * startScalesArr[i].y;
					}
				} break;
				case ScaleTransition::None:
					break;
			}
		}
	}

	const bool NativeScaleModule::isValid()
	{
		return false; // ???
	}

	bool NativeScaleModule::getAbsoluteValue() 
	{
		return absoluteValue;
	}

	void NativeScaleModule::setAbsoluteValue(bool val)
	{
		absoluteValue = val;
	}

	void NativeScaleModule::setNone()
	{
		transition = ScaleTransition::None;
	}

	void NativeScaleModule::setLerp(float start, float end)
	{
		transition = ScaleTransition::Lerp;
		this->start = start;
		this->end = end;
	}

	void NativeScaleModule::setCurve(Curve* const curve)
	{
		transition = ScaleTransition::Curve;
		if (this->curve != nullptr)
			delete this->curve;

		this->curve = curve;
	}

	void NativeScaleModule::setRandomCurve(Curve* const curve)
	{
		transition = ScaleTransition::RandomCurve;
		if (this->curve != nullptr)
			delete this->curve;

		this->curve = curve;
	}

	NativeScaleModule::~NativeScaleModule()
	{
		delete[] startScalesArr;
		delete[] rand;
		delete curve;
	}

	LIB_API(NativeScaleModule*) nativeModule_ScaleModule_Ctor(NativeModule* const modulePtr)
	{
		return new NativeScaleModule(modulePtr);
	}

	LIB_API(bool) nativeModule_ScaleModule_GetAbsoluteValue(NativeScaleModule* const modulePtr)
	{
		return modulePtr->getAbsoluteValue();
	}

	LIB_API(void) nativeModule_ScaleModule_SetAbsoluteValue(NativeScaleModule* const modulePtr, bool val)
	{
		modulePtr->setAbsoluteValue(val);
	}

	LIB_API(void) nativeModule_ScaleModule_SetNone(NativeScaleModule* const modulePtr)
	{
		modulePtr->setNone();
	}

	LIB_API(void) nativeModule_ScaleModule_SetLerp(NativeScaleModule* const modulePtr, float start, float end)
	{
		modulePtr->setLerp(start, end);
	}

	LIB_API(void) nativeModule_ScaleModule_SetCurve(NativeScaleModule* const modulePtr, Curve* const curvePtr)
	{
		modulePtr->setCurve(curvePtr);
	}

	LIB_API(void) nativeModule_ScaleModule_SetRandomCurve(NativeScaleModule* const modulePtr, Curve* const curvePtr)
	{
		modulePtr->setRandomCurve(curvePtr);
	}

}