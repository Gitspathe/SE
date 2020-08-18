#include "NativeModule.h"
#include "NativeAlphaModule.h"
#include "ParticleMath.h"
#include <stdexcept>

namespace Particles 
{
	NativeModule::NativeModule()
	{ 
		submodules = new std::vector<NativeSubmodule*>();
	}

	void NativeModule::addSubmodule(NativeSubmodule* submodule)
	{
		for(NativeSubmodule* ptr : *submodules){
			if(ptr == submodule)
				throw std::invalid_argument("Duplicate submodule!");
		}

		submodules->push_back(submodule);
	}

	void NativeModule::removeSubmodule(NativeSubmodule* submodule)
	{
		std::vector<NativeSubmodule*>* newSubmodules = new std::vector<NativeSubmodule*>();
		std::vector<NativeSubmodule*> curSubmodules = *submodules;

		// Create replacement vector.
		for(NativeSubmodule* ptr : curSubmodules) {
			if(ptr != submodule)
				newSubmodules->push_back(ptr);
			else
				delete ptr;
		}

		// Delete current submodules.
		(curSubmodules).clear();
		delete submodules;

		// Replace current submodules with the new vector.
		submodules = newSubmodules;
	}

	void NativeModule::onInitialize(int particleArrayLength) 
	{
		for(NativeSubmodule* ptr : *submodules) {
			ptr->onInitialize(particleArrayLength);
		}
	}

	void NativeModule::onParticlesActivated(int* particleIndexArr, Particle* particlesArrPtr, int length)
	{
		for(NativeSubmodule* ptr : *submodules) {
			ptr->onParticlesActivated(particleIndexArr, particlesArrPtr, length);
		}
	}

	void NativeModule::onUpdate(float deltaTime, Particle* particleArrPtr, int length)
	{
		for(NativeSubmodule* ptr : *submodules) {
			ptr->onUpdate(deltaTime, particleArrPtr, length);
		}
	}

	NativeModule::~NativeModule()
	{
		for(NativeSubmodule* ptr : *submodules) {
			delete ptr;
		}
		submodules->clear();
		delete submodules;
	}

	#pragma region INTEROP METHODS.
	
	LIB_API(void) nativeModule_addSubmodule(NativeModule* modulePtr, NativeSubmodule* submodulePtr) 
	{
		modulePtr->addSubmodule(submodulePtr);
	}

	LIB_API(void) nativeModule_removeSubmodule(NativeModule* modulePtr, NativeSubmodule* submodulePtr) 
	{
		modulePtr->removeSubmodule(submodulePtr);
	}

	LIB_API(void) nativeModule_OnParticlesActivated(NativeModule* modulePtr, int* particleIndexArr, Particle* particleArrPtr, int length) 
	{
		modulePtr->onParticlesActivated(particleIndexArr, particleArrPtr, length);
	}

	LIB_API(void) nativeModule_OnUpdate(NativeModule* modulePtr, float deltaTime, Particle* particleArrPtr, int length) 
	{
		modulePtr->onUpdate(deltaTime, particleArrPtr, length);
	}

	LIB_API(NativeModule*) nativeModule_Create()
	{
		return new NativeModule();
	}

	LIB_API(void) nativeModule_OnInitialize(NativeModule* modulePtr, int particleArrayLength) 
	{
		modulePtr->onInitialize(particleArrayLength);
	}

	LIB_API(void) nativeModule_Delete(NativeModule* modulePtr) 
	{
		delete modulePtr;
	}

	#pragma endregion


}