#pragma once

namespace thomas
{
	namespace editor
	{
		class EditorGUI
		{
		public:
			EditorGUI();
			~EditorGUI() = default;

		public:
			void UpdateEditorGUI();

		private:
			void AddGameObjectsPopup();
		};
	}
}