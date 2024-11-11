namespace Mapbox.Editor {

	using UnityEditor;
	using UnityEngine;
	using Mapbox.Unity.Map;

	[CustomPropertyDrawer(typeof(RangeTileProviderOptions))]
	public class RangeTileProviderOptionsDrawer : PropertyDrawer {

		private static float lineHeight = EditorGUIUtility.singleLineHeight;

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			foreach (object item in property) {
				SerializedProperty subproperty = item as SerializedProperty;
				EditorGUILayout.PropertyField(subproperty, true);
				position.height = lineHeight;
				position.y += lineHeight;
			}
		}

	}

}