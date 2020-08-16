#pragma once

#ifndef PARTICLE_H
#define PARTICLE_H

#include "Utility.h"

using namespace Utility;

namespace Particles 
{
	struct Particle {
	public:
		Vector2 Position;
		Vector2 Scale;
		Vector2 Direction;
		Vector4 Color;
		float Mass;
		float Speed;
		float SpriteRotation;
		float InitialLife;
		float TimeAlive;
		float LayerDepth;
		Int4 SourceRectangle;
	};
}

#endif