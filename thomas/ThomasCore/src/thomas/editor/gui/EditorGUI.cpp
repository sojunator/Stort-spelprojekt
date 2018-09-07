#include "EditorGUI.hpp"
#include "../../Input.h"
#include "../../object/GameObject.h"
#include <imgui/imgui.h>

namespace thomas
{
	namespace editor
	{
		EditorGUI::EditorGUI() {}

		void EditorGUI::UpdateEditorGUI()
		{
			AddGameObjectsPopup();
		}

		void EditorGUI::AddGameObjectsPopup()
		{
			if (Input::GetKey(Input::Keys::LeftShift) && Input::GetKey(Input::Keys::A))
				ImGui::OpenPopup("Add_GameObjects");

			// Popup for different kinds of gameobjects
			ImGui::SetNextWindowSize(ImVec2(100, 100));
			if (ImGui::BeginPopup("Add_GameObjects"))
			{
				ImGui::Text("Add");
				ImGui::Separator();
				if (ImGui::BeginMenu("3D Object"))
				{
					if (ImGui::MenuItem("Cube"))
					{
						//auto cube = new object::GameObject("Cube");
						/*GameObject^ gameObject = gcnew GameObject("new" + type.ToString());
						gameObject->AddComponent<RenderComponent^>()->model = Model::GetPrimitive(type);*/
					}

					if (ImGui::MenuItem("Sphere"))
					{
					}

					ImGui::EndMenu();
				}
				ImGui::EndPopup();
			}
		}
	}
}