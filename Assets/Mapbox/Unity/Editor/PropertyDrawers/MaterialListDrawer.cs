namespace Mapbox.Editor {

	using UnityEditor;
	using UnityEngine;
	using Unity.MeshGeneration.Modifiers;

	[CustomPropertyDrawer(typeof(MaterialList))]
	public class MaterialListDrawer : PropertyDrawer {

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			EditorGUI.BeginProperty(position, label, property);
			SerializedProperty matArray = property.FindPropertyRelative("Materials");

			if (matArray.arraySize == 0) {
				matArray.arraySize = 1;
			}

			EditorGUILayout.PropertyField(property.FindPropertyRelative("Materials").GetArrayElementAtIndex(0), label);
			EditorGUI.EndProperty();
		}

	}

}