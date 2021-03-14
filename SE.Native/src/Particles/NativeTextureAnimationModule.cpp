#include "NativeTextureAnimationModule.h"
#include "ParticleMath.h"
#include "Particle.h"
#include "src/Utility.h"

namespace Particles {

	NativeTextureAnimationModule::NativeTextureAnimationModule() : NativeSubmodule() { }

	void Particles::NativeTextureAnimationModule::onInitialize(const int32_t particleArrayLength)
	{
		particlesLength = particleArrayLength;
		isInitialized = true;
	}

	void NativeTextureAnimationModule::onParticlesActivated(const int32_t* const particleIndexArr, Particle* const particlesArrPtr, const int32_t length) { }

	void NativeTextureAnimationModule::onUpdate(float deltaTime, Particle* const particleArrPtr, const int32_t length)
	{
		int totalFrames = sheetRows * sheetColumns;
		int frameSize = (int)textureSize.x / sheetRows;
		switch (loopMode) {
			case TextureAnimationMode::Life: {
				#pragma omp simd
				for (size_t i = 0; i < length; i++) {
					Particle* particle = &particleArrPtr[i];
					int frame = (int)ParticleMath::between(0.0f, totalFrames, particle->timeAlive / particle->initialLife);
					int frameX = floor(frame % sheetRows);
					int frameY = floor(frame / sheetRows);
					particle->sourceRectangle = Int4(
						frameX * frameSize,
						frameY * frameSize,
						frameSize,
						frameSize);
				}
			} break;
			case TextureAnimationMode::Loop: {
				// TODO.
			} break;
		}
	}

	const bool NativeTextureAnimationModule::isValid()
	{
		return false;
	}

	void NativeTextureAnimationModule::setOverLifetime(const int32_t sheetRows, const int32_t sheetColumns)
	{
		this->sheetRows = sheetRows;
		this->sheetColumns = sheetColumns;
	}

	void NativeTextureAnimationModule::setTextureSize(const Vector2 textureSize)
	{
		this->textureSize = textureSize;
	}

	NativeTextureAnimationModule::~NativeTextureAnimationModule() { }

	LIB_API(NativeTextureAnimationModule*) nativeModule_TextureAnimationModule_Ctor()
	{
		return new NativeTextureAnimationModule();
	}

	LIB_API(void) nativeModule_TextureAnimationModule_SetTextureSize(NativeTextureAnimationModule* const modulePtr, const Vector2 textureSize)
	{
		modulePtr->setTextureSize(textureSize);
	}

	LIB_API(void) nativeModule_TextureAnimationModule_SetOverLifetime(NativeTextureAnimationModule* const modulePtr, int32_t sheetRows, int32_t sheetColumns)
	{
		modulePtr->setOverLifetime(sheetRows, sheetColumns);
	}

}