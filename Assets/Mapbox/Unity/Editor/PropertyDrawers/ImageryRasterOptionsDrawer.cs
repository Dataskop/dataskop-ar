namespace Mapbox.Editor {

	using UnityEditor;
	using UnityEngine;
	using Mapbox.Unity.Map;

	[CustomPropertyDrawer(typeof(ImageryRasterOptions))]
	public class ImageryRasterOptionsDrawer : PropertyDrawer {

		private static float lineHeight = EditorGUIUtility.singleLineHeight;
		private bool showPosition = true;

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			EditorGUI.BeginProperty(position, label, property);
			position.height = lineHeight;

			foreach (object item in property) {
				SerializedProperty subproperty = item as SerializedProperty;
				EditorGUI.PropertyField(position, subproperty, true);
				position.height = lineHeight;
				position.y += lineHeight;
			}

			EditorGUI.EndProperty();
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
			int rows = showPosition ? 3 : 1;
			return (float)rows * lineHeight;
		}

	}

}