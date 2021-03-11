#include "NativeModule.h"
#include "NativeSpriteRotationModule.h"
#include "ParticleMath.h"
#include "Particle.h"
#include "src/Utility.h"

namespace Particles {

	bool NativeSpriteRotationModule::isRandom()
	{
		return (transition == SpriteRotationTransition::RandomCurve) || (transition == SpriteRotationTransition::RandomConstant);
	}

	NativeSpriteRotationModule::NativeSpriteRotationModule(NativeModule* const parent) : NativeSubmodule(parent) { }

	void NativeSpriteRotationModule::regenerateRandom()
	{
		if (!isRandom() || !isInitialized)
			return;

		delete[] rand;
		rand = new float[particlesLength];
		for (int i = 0; i < particlesLength; i++) {
			rand[i] = Random::range(0.0f, 1.0f);
		}
	}

	void NativeSpriteRotationModule::onInitialize(const int32_t particleArrayLength)
	{
		particlesLength = particleArrayLength;
		isInitialized = true;

		regenerateRandom();
	}

	void NativeSpriteRotationModule::onParticlesActivated(const int32_t* const particleIndexArr, Particle* const particlesArrPtr, const int32_t length)
	{
		for (int32_t i = 0; i < length; i++) {
			if (!isRandom())
				continue;

			rand[particleIndexArr[i]] = Random::range(0.0f, 1.0f);
		}
	}

	void NativeSpriteRotationModule::onUpdate(const float deltaTime, Particle* const particleArrPtr, const int32_t length)
	{
		switch (transition) {
			case SpriteRotationTransition::Constant: {
				#pragma omp simd
				for (int32_t i = 0; i < length; i++) {
					Particle* particle = &particleArrPtr[i];
					particle->spriteRotation += start * deltaTime;
				}
			} break;
			case SpriteRotationTransition::Lerp: {
				#pragma omp simd
				for (int32_t i = 0; i < length; i++) {
					Particle* particle = &particleArrPtr[i];
					const float angleDelta = ParticleMath::lerp(start, end, particle->timeAlive / particle->initialLife);
					particle->spriteRotation += angleDelta * deltaTime;
				}
			} break;
			case SpriteRotationTransition::Curve: {
				for (int32_t i = 0; i < length; i++) {
					Particle* particle = &particleArrPtr[i];
					particle->spriteRotation += curve->Evaluate(particle->timeAlive / particle->initialLife);
				}
			} break;
			case SpriteRotationTransition::RandomConstant: {
				#pragma omp simd
				for (int32_t i = 0; i < length; i++) {
					Particle* particle = &particleArrPtr[i];
					particle->spriteRotation += ParticleMath::between(start, end, rand[i]) * deltaTime;
				}
			} break;
			case SpriteRotationTransition::RandomCurve: {
				for (int32_t i = 0; i < length; i++) {
					Particle* particle = &particleArrPtr[i];
					particle->spriteRotation += curve->Evaluate(rand[i]) * deltaTime;
				}
			} break;
			case SpriteRotationTransition::None:
				break;
		}
	}

	const bool NativeSpriteRotationModule::isValid()
	{
		return false; // ???
	}

	void NativeSpriteRotationModule::setNone()
	{
		transition = SpriteRotationTransition::None;
	}

	void NativeSpriteRotationModule::setConstant(float val)
	{
		transition = SpriteRotationTransition::Constant;
		this->start = val;
	}

	void NativeSpriteRotationModule::setLerp(float start, float end)
	{
		transition = SpriteRotationTransition::Lerp;
		this->start = start;
		this->end = end;
	}

	void NativeSpriteRotationModule::setCurve(Curve* const curve)
	{
		transition = SpriteRotationTransition::Curve;
		if (this->curve != nullptr)
			delete this->curve;

		this->curve = curve;
	}

	void NativeSpriteRotationModule::setRandomConstant(float min, float max) 
	{
		transition = SpriteRotationTransition::RandomConstant;
		this->start = min;
		this->end = max;
	}

	void NativeSpriteRotationModule::setRandomCurve(Curve* const curve)
	{
		transition = SpriteRotationTransition::RandomCurve;
		if (this->curve != nullptr)
			delete this->curve;

		this->curve = curve;
	}

	NativeSpriteRotationModule::~NativeSpriteRotationModule()
	{
		delete[] rand;
		delete curve;
	}

	LIB_API(NativeSpriteRotationModule*) nativeModule_SpriteRotationModule_Ctor(NativeModule* const modulePtr)
	{
		return new NativeSpriteRotationModule(modulePtr);
	}

	LIB_API(void) nativeModule_SpriteRotationModule_SetNone(NativeSpriteRotationModule* const modulePtr)
	{
		modulePtr->setNone();
	}

	LIB_API(void) nativeModule_SpriteRotationModule_SetConstant(NativeSpriteRotationModule* const modulePtr, float val)
	{
		modulePtr->setConstant(val);
	}

	LIB_API(void) nativeModule_SpriteRotationModule_SetLerp(NativeSpriteRotationModule* const modulePtr, float start, float end)
	{
		modulePtr->setLerp(start, end);
	}

	LIB_API(void) nativeModule_SpriteRotationModule_SetCurve(NativeSpriteRotationModule* const modulePtr, Curve* const curvePtr)
	{
		modulePtr->setCurve(curvePtr);
	}

	LIB_API(void) nativeModule_SpriteRotationModule_SetRandomConstant(NativeSpriteRotationModule* const modulePtr, float min, float max)
	{
		modulePtr->setRandomConstant(min, max);
	}

	LIB_API(void) nativeModule_SpriteRotationModule_SetRandomCurve(NativeSpriteRotationModule* const modulePtr, Curve* const curvePtr)
	{
		modulePtr->setRandomCurve(curvePtr);
	}

}