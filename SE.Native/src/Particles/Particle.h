#pragma once

#ifndef PARTICLE_H
#define PARTICLE_H

#include "src/Utility.h"

using namespace Utility;

namespace Particles
{
	struct Particle {
	public:
		Vector2 position;
		Vector2 scale;
		int id;
		Vector2 direction;
		Vector4 color;
		float mass;
		float speed;
		float spriteRotation;
		float initialLife;
		float timeAlive;
		float layerDepth;
		Int4 sourceRectangle;
	};
}

#endif