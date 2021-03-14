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

	void NativeModule::addSubmodule(NativeSubmodule* const submodule)
	{
		for(NativeSubmodule* ptr : *submodules){
			if(ptr == submodule)
				throw std::invalid_argument("Duplicate submodule!");
		}

		submodules->push_back(submodule);
	}

	void NativeModule::removeSubmodule(NativeSubmodule* const submodule)
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

	void NativeModule::onInitialize(NativeSubmodule* const submodulePtr, const int32_t particleArrayLength)
	{
		submodulePtr->onInitialize(particleArrayLength);
	}

	void NativeModule::onParticlesActivated(const int32_t* const particleIndexArr, Particle* const particlesArrPtr, const int32_t length)
	{
		for(NativeSubmodule* ptr : *submodules) {
			ptr->onParticlesActivated(particleIndexArr, particlesArrPtr, length);
		}
	}

	void NativeModule::onUpdate(const float deltaTime, Particle* const particleArrPtr, const int32_t length)
	{
		for(NativeSubmodule* ptr : *submodules){
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
	
	LIB_API(void) nativeModule_addSubmodule(NativeModule* const modulePtr, NativeSubmodule* const submodulePtr)
	{
		modulePtr->addSubmodule(submodulePtr);
	}

	LIB_API(void) nativeModule_removeSubmodule(NativeModule* const modulePtr, NativeSubmodule* const submodulePtr)
	{
		modulePtr->removeSubmodule(submodulePtr);
	}

	LIB_API(void) nativeModule_OnParticlesActivated(NativeModule* const modulePtr, const int32_t* const particleIndexArr, Particle* const particleArrPtr, const int32_t length)
	{
		modulePtr->onParticlesActivated(particleIndexArr, particleArrPtr, length);
	}

	LIB_API(void) nativeModule_OnUpdate(NativeModule* const modulePtr, const float deltaTime, Particle* const particleArrPtr, const int32_t length)
	{
		modulePtr->onUpdate(deltaTime, particleArrPtr, length);
	}

	LIB_API(NativeModule*) nativeModule_Create()
	{
		return new NativeModule();
	}

	LIB_API(void) nativeModule_OnInitialize(NativeModule* const modulePtr, NativeSubmodule* const submodulePtr, const int32_t particleArrayLength)
	{
		modulePtr->onInitialize(submodulePtr, particleArrayLength);
	}

	LIB_API(void) nativeModule_Delete(NativeModule* const modulePtr) 
	{
		delete modulePtr;
	}

	#pragma endregion


}