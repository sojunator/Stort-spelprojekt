#include "ThomasSelection.h"

namespace ThomasEngine {

	ThomasSelection::ThomasSelection()
		: m_SelectedGameObjects(gcnew ObservableCollection<GameObject^>())
	{
	}

	ThomasSelection::~ThomasSelection()
	{
	}


	void ThomasSelection::SelectGameObject(GameObject^ gObj)
	{
		Monitor::Enter(m_SelectedGameObjects);
		m_SelectedGameObjects->Clear();
		m_SelectedGameObjects->Add(gObj);
		thomas::editor::EditorCamera::SelectObject((thomas::object::GameObject*)gObj->nativePtr);
		Monitor::Exit(m_SelectedGameObjects);

	}

	void ThomasSelection::UnSelectGameObject(GameObject ^ gObj)
	{
		Monitor::Enter(m_SelectedGameObjects);
		m_SelectedGameObjects->Remove(gObj);
		thomas::editor::EditorCamera::UnselectObject((thomas::object::GameObject*)gObj->nativePtr);
		Monitor::Exit(m_SelectedGameObjects);
	}

	void ThomasSelection::UnselectGameObjects()
	{
		Monitor::Enter(m_SelectedGameObjects);
		m_SelectedGameObjects->Clear();
		thomas::editor::EditorCamera::SelectObject(nullptr);
		Monitor::Exit(m_SelectedGameObjects);
	}

	void ThomasSelection::render() 
	{
		Monitor::Enter(m_SelectedGameObjects);
		for each(ThomasEngine::GameObject^ gameObject in m_SelectedGameObjects)
		{
			if (gameObject->GetActive())
				gameObject->RenderSelectedGizmos();
		}
		Monitor::Exit(m_SelectedGameObjects);
	}


	void ThomasSelection::UpdateSelectedObjects() {
		List<GameObject^> tempSelectedGameObjects;
		Monitor::Enter(m_SelectedGameObjects);
		for (thomas::object::GameObject* gameObject : thomas::editor::EditorCamera::GetSelectedObjects())
		{
			GameObject^ gObj = (GameObject^)ThomasEngine::Object::GetObject(gameObject);
			if (gObj)
				tempSelectedGameObjects.Add(gObj);
		}
		if (tempSelectedGameObjects.Count == m_SelectedGameObjects->Count)
		{
			if (tempSelectedGameObjects.Count > 0)
				if (tempSelectedGameObjects[0] != m_SelectedGameObjects[0])
				{
					m_SelectedGameObjects->Clear();
					for each(GameObject^ gObj in tempSelectedGameObjects)
						m_SelectedGameObjects->Add(gObj);
				}
		}
		else
		{
			m_SelectedGameObjects->Clear();
			for each(GameObject^ gObj in tempSelectedGameObjects)
				m_SelectedGameObjects->Add(gObj);
		}
		Monitor::Exit(m_SelectedGameObjects);
		thomas::editor::EditorCamera::SetHasSelectionChanged(false);
	}

	bool ThomasSelection::Contain(GameObject ^ gObj)
	{
		Monitor::Enter(m_SelectedGameObjects);
		bool c = m_SelectedGameObjects->Contains(gObj);
		Monitor::Exit(m_SelectedGameObjects);
		return c;
	}

	GameObject ^ ThomasSelection::operator[](int index)
	{
		Monitor::Enter(m_SelectedGameObjects);
		GameObject ^ v = m_SelectedGameObjects[index];
		Monitor::Exit(m_SelectedGameObjects);
		return v;
	}


}
