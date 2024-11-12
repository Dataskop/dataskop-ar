namespace Mapbox.Editor {

	using System.Collections;
	using System.Collections.Generic;
	using UnityEditor;
	using UnityEngine;
	using Mapbox.Unity.Map;
	using VectorTile.ExtensionMethods;
	using Editor;

	[CustomPropertyDrawer(typeof(ColliderOptions))]
	public class ColliderOptionsDrawer : PropertyDrawer {

		private static float lineHeight = EditorGUIUtility.singleLineHeight;
		private bool isGUIContentSet = false;
		private GUIContent[] colliderTypeContent;

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			EditorGUI.BeginProperty(position, null, property);
			GUIContent colliderTypeLabel = new() {
				text = "Collider Type",
				tooltip = "The type of collider added to game objects in this layer."
			};

			SerializedProperty colliderTypeProperty = property.FindPropertyRelative("colliderType");

			string[] displayNames = colliderTypeProperty.enumDisplayNames;
			int count = colliderTypeProperty.enumDisplayNames.Length;

			if (!isGUIContentSet) {
				colliderTypeContent = new GUIContent[count];

				for (int extIdx = 0; extIdx < count; extIdx++) {
					colliderTypeContent[extIdx] = new GUIContent {
						text = displayNames[extIdx],
						tooltip = EnumExtensions.Description((ColliderType)extIdx)
					};
				}

				isGUIContentSet = true;
			}

			EditorGUI.BeginChangeCheck();
			colliderTypeProperty.enumValueIndex = EditorGUILayout.Popup(
				colliderTypeLabel, colliderTypeProperty.enumValueIndex, colliderTypeContent
			);

			if (EditorGUI.EndChangeCheck()) {
				EditorHelper.CheckForModifiedProperty(property);
			}

			EditorGUI.EndProperty();
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
			return lineHeight;
		}

	}

}