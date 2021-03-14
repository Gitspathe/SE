#include "NativeColorModule.h"
#include "ParticleMath.h"
#include "Particle.h"
#include "src/Utility/Random.h"

namespace Particles {

	bool NativeColorModule::isRandom()
	{
		return transition == ColorTransition::RandomLerp;
	}

	NativeColorModule::NativeColorModule() : NativeSubmodule() { }

	void NativeColorModule::regenerateRandom()
	{
		if (!isRandom() || !isInitialized)
			return;

		delete[] randEndColors;
		randEndColors = new Vector4[particlesLength];
	}

	void NativeColorModule::onInitialize(const int32_t particleArrayLength)
	{
		particlesLength = particleArrayLength;
		startColorsArr = new Vector4[particleArrayLength];
		isInitialized = true;

		regenerateRandom();
	}

	void NativeColorModule::onParticlesActivated(const int32_t* const particleIndexArr, Particle* const particlesArrPtr, const int32_t length)
	{
		for (int32_t i = 0; i < length; i++) {
			int32_t pIndex = particleIndexArr[i];
			Particle* particle = &particlesArrPtr[pIndex];
			startColorsArr[particle->id] = particle->color;
			if (!isRandom())
				continue;

			randEndColors[particle->id] = Vector4(
				ParticleMath::between(end1.x, end2.x, Random::range(0.0f, 1.0f)),
				ParticleMath::between(end1.y, end2.y, Random::range(0.0f, 1.0f)),
				ParticleMath::between(end1.z, end2.z, Random::range(0.0f, 1.0f)),
				ParticleMath::between(end1.w, end2.w, Random::range(0.0f, 1.0f)));
		}
	}

	void NativeColorModule::onUpdate(const float deltaTime, Particle* const particleArrPtr, const int32_t length)
	{
		switch (transition) {
			case ColorTransition::Lerp: {
				#pragma omp simd
				for (int32_t i = 0; i < length; i++) {
					Particle* particle = &particleArrPtr[i];
					const float life = particle->timeAlive / particle->initialLife;
					int32_t pId = particleArrPtr[i].id;
					particle->color.x = ParticleMath::lerp(startColorsArr[pId].x, end1.x, life);
					particle->color.y = ParticleMath::lerp(startColorsArr[pId].y, end1.y, life);
					particle->color.z = ParticleMath::lerp(startColorsArr[pId].z, end1.z, life);
					particle->color.w = ParticleMath::lerp(startColorsArr[pId].w, end1.w, life);
				}
			} break;
			case ColorTransition::RandomLerp: {
				#pragma omp simd
				for (int32_t i = 0; i < length; i++) {
					Particle* particle = &particleArrPtr[i];
					const float life = particle->timeAlive / particle->initialLife;
					int32_t pId = particleArrPtr[i].id;
					particle->color.x = ParticleMath::lerp(startColorsArr[pId].x, randEndColors[pId].x, life);
					particle->color.y = ParticleMath::lerp(startColorsArr[pId].y, randEndColors[pId].y, life);
					particle->color.z = ParticleMath::lerp(startColorsArr[pId].z, randEndColors[pId].z, life);
					particle->color.w = ParticleMath::lerp(startColorsArr[pId].w, randEndColors[pId].w, life);
				}
			} break;
			case ColorTransition::Curve: {
				for (int32_t i = 0; i < length; i++) {
					Particle* particle = &particleArrPtr[i];
					particle->color = curve->Evaluate(particle->timeAlive / particle->initialLife);
				}
			} break;
			case ColorTransition::None:
				break;
		}
	}

	const bool NativeColorModule::isValid()
	{
		return false; // ???
	}

	void NativeColorModule::setNone()
	{
		transition = ColorTransition::None;
	}

	void NativeColorModule::setLerp(Vector4 end)
	{
		transition = ColorTransition::Lerp;
		end1 = end;
	}

	void NativeColorModule::setRandomLerp(Vector4 min, Vector4 max)
	{
		if (min.x > max.x)
			ParticleMath::swap(&min.x, &max.x);
		if (min.y > max.y)
			ParticleMath::swap(&min.y, &max.y);
		if (min.z > max.z)
			ParticleMath::swap(&min.z, &max.z);
		if (min.w > max.w)
			ParticleMath::swap(&min.w, &max.w);

		transition = ColorTransition::RandomLerp;
		end1 = min;
		end2 = max;
		regenerateRandom();
	}

	void NativeColorModule::setCurve(Curve4* const curve)
	{
		transition = ColorTransition::Curve;
		if (this->curve != nullptr)
			delete this->curve;

		this->curve = curve;
	}

	NativeColorModule::~NativeColorModule()
	{
		delete[] startColorsArr;
		delete[] randEndColors;
		delete curve;
	}

	LIB_API(NativeColorModule*) nativeModule_ColorModule_Ctor()
	{
		return new NativeColorModule();
	}

	LIB_API(void) nativeModule_ColorModule_SetNone(NativeColorModule* const modulePtr)
	{
		modulePtr->setNone();
	}

	LIB_API(void) nativeModule_ColorModule_SetLerp(NativeColorModule* const modulePtr, Vector4 end)
	{
		modulePtr->setLerp(end);
	}

	LIB_API(void) nativeModule_ColorModule_SetRandomLerp(NativeColorModule* const modulePtr, Vector4 min, Vector4 max)
	{
		modulePtr->setRandomLerp(min, max);
	}

	LIB_API(void) nativeModule_ColorModule_SetCurve(NativeColorModule* const modulePtr, Curve4* const curvePtr)
	{
		modulePtr->setCurve(curvePtr);
	}
}