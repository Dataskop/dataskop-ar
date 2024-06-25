using Dataskop.Entities.Visualizations;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Bubble))]
public class BubbleSizeClamper : Editor {

	private SerializedProperty maxValue;

	private SerializedProperty minValue;

	private void OnEnable() {
		maxValue = serializedObject.FindProperty("maxScale");
		minValue = serializedObject.FindProperty("minScale");
	}

	public override void OnInspectorGUI() {
		base.OnInspectorGUI();
		serializedObject.Update();

		EditorGUILayout.Slider(minValue, 0, 2f, new GUIContent("Min Scale"));

		if (minValue.floatValue > maxValue.floatValue)
			maxValue.floatValue = minValue.floatValue;

		EditorGUILayout.Slider(maxValue, 0, 2f, new GUIContent("Max Scale"));

		if (maxValue.floatValue <= minValue.floatValue)
			minValue.floatValue = maxValue.floatValue;

		serializedObject.ApplyModifiedProperties();
	}

}