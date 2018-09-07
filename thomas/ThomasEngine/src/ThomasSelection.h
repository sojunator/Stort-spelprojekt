#pragma once

#pragma managed
#include "object\GameObject.h"


using namespace System::Collections::Generic;

namespace ThomasEngine {

	public ref class ThomasSelection {
	private:
		ObservableCollection<GameObject^>^ m_SelectedGameObjects;

	public:

		ThomasSelection();
		~ThomasSelection();

		void SelectGameObject(GameObject^ gObj);
		void UnSelectGameObject(GameObject^ gObj);

		void UnselectGameObjects();

		void render();

		void UpdateSelectedObjects();


		bool Contain(GameObject^ gObj);

	public:
		GameObject^ operator[](int index);

		property int Count {
			int get() { return m_SelectedGameObjects->Count; }
		}

		property const ObservableCollection<GameObject^>^ Ref {
			const ObservableCollection<GameObject^>^ get() { return m_SelectedGameObjects; }
		}
	};
}
