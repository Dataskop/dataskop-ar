namespace Mapbox.Editor {

	using UnityEngine;
	using UnityEditor;
	using Unity.MeshGeneration.Modifiers;

	[CustomEditor(typeof(MaterialModifier))]
	public class MaterialModifierEditor : Editor {

		private MonoScript script;
		private SerializedProperty _materials;

		private void OnEnable() {
			script = MonoScript.FromScriptableObject((MaterialModifier)target);
			_materials = serializedObject.FindProperty("_options");
		}

		public override void OnInspectorGUI() {
			serializedObject.Update();
			GUI.enabled = false;
			script = EditorGUILayout.ObjectField("Script", script, typeof(MonoScript), false) as MonoScript;
			GUI.enabled = true;

			EditorGUILayout.Space();
			EditorGUILayout.PropertyField(_materials);
			EditorGUILayout.Space();

			serializedObject.ApplyModifiedProperties();
		}

	}

}