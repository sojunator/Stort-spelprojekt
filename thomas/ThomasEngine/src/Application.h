#pragma once

#include "Project.h"
using namespace System;
namespace ThomasEngine
{
	public ref class Application
	{
	public:
		delegate void CurrentProjectChangedEvent(Project^ newProject);
		static event CurrentProjectChangedEvent^ currentProjectChanged;
		static Project^ m_currentProject;
		static String^ editorAssets = "..\\Data\\";

		static property Project^ currentProject
		{
			Project^ get() { return m_currentProject; }
			void set(Project^ value) {
				m_currentProject = nullptr;
				if (Scene::CurrentScene) {
					Scene::CurrentScene->UnLoad();
				}
				currentProjectChanged(value);
				m_currentProject = value;
				Scene^ currentScene;
				if (m_currentProject->currentScenePath)
					currentScene = Scene::LoadScene(m_currentProject->currentScenePath);
				
				if (currentScene)
					Scene::CurrentScene = currentScene;
				else
					Scene::CurrentScene = gcnew Scene("scene");
			}
		}
	};
}
