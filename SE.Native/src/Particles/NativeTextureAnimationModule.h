#pragma once

#ifndef NATIVETEXTUREANIMATIONSUBMODULE_H
#define NATIVETEXTUREANIMATIONSUBMODULE_H

#include "src/SE.Native.h"
#include "Particle.h"
#include "NativeSubmodule.h"
#include "src/Utility.h"

namespace Particles {

	namespace TextureAnimationMode {
		enum Mode { Life, Loop };
	}

	class NativeTextureAnimationModule final : NativeSubmodule {
	private:
		TextureAnimationMode::Mode loopMode = TextureAnimationMode::Life;
		int32_t sheetRows = 1;
		int32_t sheetColumns = 1;
		Vector2 textureSize = Vector2(512, 512);

	public:
		NativeTextureAnimationModule();

		void onInitialize(const int32_t particleArrayLength) override;
		void onParticlesActivated(const int32_t* const particleIndexArr, Particle* const particlesArrPtr, const int32_t length) override;
		void onUpdate(float deltaTime, Particle* const particleArrPtr, const int32_t length) override;
		const bool isValid() override;

		void setOverLifetime(const int32_t sheetRows, const int32_t sheetColumns);
		void setTextureSize(const Vector2 textureSize);

		~NativeTextureAnimationModule() override;
	};

}

#endif