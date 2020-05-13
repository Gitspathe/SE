# DeeZEngine

DeeZEngine is an amateur's attempt at creating a 2D game engine using the MonoGame framework.

## Features

### Current

* Gameobjects, components and transform hierarchies
* Grid-based culling and spatial partitioning
* Advanced physics with Aether.Physics2D
* Level system and map editor
* UI system
* Input system
* Stereo audio
* Modding support
* Networking/multiplayer with LiteNetLib
* Lighting system with Penumbra
* Multithreaded particle system with modules / extension options

### Planned

* Full editor
* AI and pathfinding
* Better API

## Software Used
These are either NuGet packages, or assemblies included within the project. So the engine should -mostly- work out of the box.
* Newtonsoft.Json
* Modified Aether.Physics2D assembly (https://github.com/tainicom/Aether.Physics2D)
* Modified MonoGame assembly (https://github.com/MonoGame/MonoGame)
* Modified Penumbra assembly (https://github.com/discosultan/penumbra)
* LiteNetLib (https://github.com/RevenantX/LiteNetLib)
* Open.NAT (https://github.com/lontivero/Open.NAT)

NOTE: On first build you will actually need to manually create a level in code, this will be improved in the near future.
