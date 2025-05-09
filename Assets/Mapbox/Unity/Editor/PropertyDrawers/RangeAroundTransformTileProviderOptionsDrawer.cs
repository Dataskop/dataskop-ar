﻿namespace Mapbox.Editor {

	using UnityEditor;
	using UnityEngine;
	using Mapbox.Unity.Map;

	[CustomPropertyDrawer(typeof(RangeAroundTransformTileProviderOptions))]
	public class RangeAroundTransformTileProviderOptionsDrawer : PropertyDrawer {

		private static float lineHeight = EditorGUIUtility.singleLineHeight;

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			foreach (object item in property) {
				SerializedProperty subproperty = item as SerializedProperty;
				EditorGUI.PropertyField(position, subproperty, true);
				position.height = lineHeight;
				position.y += lineHeight;
			}
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
			return 3 * lineHeight;
		}

	}

}