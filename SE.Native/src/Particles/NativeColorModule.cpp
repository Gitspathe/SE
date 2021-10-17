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
		randEndColors = new ParticleColor[particlesLength];
	}

	void NativeColorModule::onInitialize(const int32_t particleArrayLength)
	{
		particlesLength = particleArrayLength;
		startColorsArr = new ParticleColor[particleArrayLength];
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

			randEndColors[particle->id] = ParticleColor(
				ParticleMath::between(end1.getHue(), end2.getHue(), Random::range(0.0f, 1.0f)),
				ParticleMath::between(end1.getSaturation(), end2.getSaturation(), Random::range(0.0f, 1.0f)),
				ParticleMath::between(end1.getLightness(), end2.getLightness(), Random::range(0.0f, 1.0f)),
				ParticleMath::between(end1.getAlpha(), end2.getAlpha(), Random::range(0.0f, 1.0f)));
		}
	}

	void NativeColorModule::onUpdate(const float deltaTime, Particle* const particleArrPtr, const int32_t length)
	{
		switch (transition) {
			case ColorTransition::Lerp: {
				for (int32_t i = 0; i < length; i++) {
					Particle* particle = &particleArrPtr[i];
					const float life = particle->timeAlive / particle->initialLife;
					int32_t pId = particleArrPtr[i].id;
					particle->color = ParticleColor::Lerp(startColorsArr[pId], end1, life);
				}
			} break;
			case ColorTransition::RandomLerp: {
				for (int32_t i = 0; i < length; i++) {
					Particle* particle = &particleArrPtr[i];
					const float life = particle->timeAlive / particle->initialLife;
					int32_t pId = particleArrPtr[i].id;
					particle->color = ParticleColor::Lerp(startColorsArr[pId], randEndColors[pId], life);
				}
			} break;
			case ColorTransition::Curve: {
				for (int32_t i = 0; i < length; i++) {
					Particle* particle = &particleArrPtr[i];
					const float life = particle->timeAlive / particle->initialLife;
					Vector4 vec = curve->Evaluate(life);
					particle->color = ParticleColor(vec.x, vec.y, vec.z, vec.w);
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

	void NativeColorModule::setLerp(ParticleColor end)
	{
		transition = ColorTransition::Lerp;
		end1 = end;
	}

	void NativeColorModule::setRandomLerp(ParticleColor min, ParticleColor max)
	{
		//if (min.x > max.x)
		//	ParticleMath::swap(&min.x, &max.x);
		//if (min.y > max.y)
		//	ParticleMath::swap(&min.y, &max.y);
		//if (min.z > max.z)
		//	ParticleMath::swap(&min.z, &max.z);
		//if (min.w > max.w)
		//	ParticleMath::swap(&min.w, &max.w);

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

	LIB_API(void) nativeModule_ColorModule_SetLerp(NativeColorModule* const modulePtr, ParticleColor end)
	{
		modulePtr->setLerp(end);
	}

	LIB_API(void) nativeModule_ColorModule_SetRandomLerp(NativeColorModule* const modulePtr, ParticleColor min, ParticleColor max)
	{
		modulePtr->setRandomLerp(min, max);
	}

	LIB_API(void) nativeModule_ColorModule_SetCurve(NativeColorModule* const modulePtr, Curve4* const curvePtr)
	{
		modulePtr->setCurve(curvePtr);
	}
}