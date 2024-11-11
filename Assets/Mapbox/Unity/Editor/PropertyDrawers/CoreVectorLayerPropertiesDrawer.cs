namespace Mapbox.Editor {

	using UnityEditor;
	using UnityEngine;
	using Mapbox.Unity.Map;
	using System.Collections.Generic;
	using System.Linq;
	using System;
	using VectorTile.ExtensionMethods;
	using Editor;

	[CustomPropertyDrawer(typeof(CoreVectorLayerProperties))]
	public class CoreVectorLayerPropertiesDrawer : PropertyDrawer {

		private bool _isGUIContentSet = false;
		private GUIContent[] _primitiveTypeContent;

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {

			EditorGUI.BeginProperty(position, null, property);

			SerializedProperty primitiveType = property.FindPropertyRelative("geometryType");

			GUIContent primitiveTypeLabel = new GUIContent {
				text = "Primitive Type",
				tooltip = "Primitive geometry type of the visualizer, allowed primitives - point, line, polygon."
			};

			string[] displayNames = primitiveType.enumDisplayNames;
			int count = primitiveType.enumDisplayNames.Length;

			if (!_isGUIContentSet) {
				_primitiveTypeContent = new GUIContent[count];

				for (int extIdx = 0; extIdx < count; extIdx++) {
					_primitiveTypeContent[extIdx] = new GUIContent {
						text = displayNames[extIdx],
						tooltip = EnumExtensions.Description((VectorPrimitiveType)extIdx)
					};
				}

				_isGUIContentSet = true;
			}

			EditorGUI.BeginChangeCheck();
			primitiveType.enumValueIndex = EditorGUILayout.Popup(
				primitiveTypeLabel, primitiveType.enumValueIndex, _primitiveTypeContent
			);

			if (EditorGUI.EndChangeCheck()) {
				EditorHelper.CheckForModifiedProperty(property);
			}

			EditorGUI.EndProperty();
		}

	}

}