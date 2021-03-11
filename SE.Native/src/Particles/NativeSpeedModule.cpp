#include "NativeModule.h"
#include "NativeSpeedModule.h"
#include "ParticleMath.h"
#include "Particle.h"
#include "src/Utility.h"

namespace Particles {

	bool NativeSpeedModule::isRandom()
	{
		return transition == SpeedTransition::RandomCurve;
	}

	NativeSpeedModule::NativeSpeedModule(NativeModule* const parent) : NativeSubmodule(parent) { }

	void NativeSpeedModule::regenerateRandom()
	{
		if (!isRandom() || !isInitialized)
			return;

		delete[] rand;
		rand = new float[particlesLength];
		for (int i = 0; i < particlesLength; i++) {
			rand[i] = Random::range(0.0f, 1.0f);
		}
	}

	void NativeSpeedModule::onInitialize(const int32_t particleArrayLength)
	{
		particlesLength = particleArrayLength;
		isInitialized = true;

		regenerateRandom();
	}

	void NativeSpeedModule::onParticlesActivated(const int32_t* const particleIndexArr, Particle* const particlesArrPtr, const int32_t length)
	{
		for (int32_t i = 0; i < length; i++) {
			if (!isRandom())
				continue;

			rand[particleIndexArr[i]] = Random::range(0.0f, 1.0f);
		}
	}

	void NativeSpeedModule::onUpdate(const float deltaTime, Particle* const particleArrPtr, const int32_t length)
	{
		if (absoluteValue) {
			switch (transition) {
				case SpeedTransition::Lerp: {
					#pragma omp simd
					for (int32_t i = 0; i < length; i++) {
						Particle* particle = &particleArrPtr[i];
						const float velocity = ParticleMath::lerp(start, end, particle->timeAlive / particle->initialLife);
						particle->speed = velocity;
					}
				} break;
				case SpeedTransition::Curve: {
					for (int32_t i = 0; i < length; i++) {
						Particle* particle = &particleArrPtr[i];
						const float velocity = curve->Evaluate(particle->timeAlive / particle->initialLife);
						particle->speed = velocity;
					}
				} break;
				case SpeedTransition::RandomCurve: {
					for (int32_t i = 0; i < length; i++) {
						Particle* particle = &particleArrPtr[i];
						const float velocity = curve->Evaluate(rand[i]);
						particle->speed = velocity;
					}
				} break;
				case SpeedTransition::None:
					break;
			}
		} else {
			switch (transition) {
				case SpeedTransition::Lerp: {
					#pragma omp simd
					for (int32_t i = 0; i < length; i++) {
						Particle* particle = &particleArrPtr[i];
						const float velocity = ParticleMath::lerp(start, end, particle->timeAlive / particle->initialLife);
						particle->speed = velocity + (velocity * deltaTime);
					}
				} break;
				case SpeedTransition::Curve: {
					for (int32_t i = 0; i < length; i++) {
						Particle* particle = &particleArrPtr[i];
						const float velocity = curve->Evaluate(particle->timeAlive / particle->initialLife);
						particle->speed = velocity + (velocity * deltaTime);
					}
				} break;
				case SpeedTransition::RandomCurve: {
					for (int32_t i = 0; i < length; i++) {
						Particle* particle = &particleArrPtr[i];
						const float velocity = curve->Evaluate(rand[i]);
						particle->speed = velocity + (velocity * deltaTime);
					}
				} break;
				case SpeedTransition::None:
					break;
			}
		}
	}

	const bool NativeSpeedModule::isValid()
	{
		return false; // ???
	}

	bool NativeSpeedModule::getAbsoluteValue()
	{
		return absoluteValue;
	}

	void NativeSpeedModule::setAbsoluteValue(bool val) 
	{
		absoluteValue = val;
	}

	void NativeSpeedModule::setNone()
	{
		transition = SpeedTransition::None;
	}

	void NativeSpeedModule::setLerp(float start, float end)
	{
		transition = SpeedTransition::Lerp;
		this->start = start;
		this->end = end;
	}

	void NativeSpeedModule::setCurve(Curve* const curve)
	{
		transition = SpeedTransition::Curve;
		if (this->curve != nullptr)
			delete this->curve;

		this->curve = curve;
	}

	void NativeSpeedModule::setRandomCurve(Curve* const curve)
	{
		transition = SpeedTransition::RandomCurve;
		if (this->curve != nullptr)
			delete this->curve;

		this->curve = curve;
	}

	NativeSpeedModule::~NativeSpeedModule()
	{
		delete[] rand;
		delete curve;
	}

	LIB_API(NativeSpeedModule*) nativeModule_SpeedModule_Ctor(NativeModule* const modulePtr)
	{
		return new NativeSpeedModule(modulePtr);
	}

	LIB_API(bool) nativeModule_SpeedModule_GetAbsoluteValue(NativeSpeedModule* const modulePtr)
	{
		return modulePtr->getAbsoluteValue();
	}

	LIB_API(void) nativeModule_SpeedModule_SetAbsoluteValue(NativeSpeedModule* const modulePtr, bool val)
	{
		modulePtr->setAbsoluteValue(val);
	}

	LIB_API(void) nativeModule_SpeedModule_SetNone(NativeSpeedModule* const modulePtr)
	{
		modulePtr->setNone();
	}

	LIB_API(void) nativeModule_SpeedModule_SetLerp(NativeSpeedModule* const modulePtr, float start, float end)
	{
		modulePtr->setLerp(start, end);
	}

	LIB_API(void) nativeModule_SpeedModule_SetCurve(NativeSpeedModule* const modulePtr, Curve* const curvePtr)
	{
		modulePtr->setCurve(curvePtr);
	}

	LIB_API(void) nativeModule_SpeedModule_SetRandomCurve(NativeSpeedModule* const modulePtr, Curve* const curvePtr)
	{
		modulePtr->setRandomCurve(curvePtr);
	}

}