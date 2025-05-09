﻿namespace Mapbox.Editor {

	using UnityEngine;
	using UnityEditor;
	using Unity.MeshGeneration.Modifiers;

	[CustomEditor(typeof(ModifierStack))]
	public class ModifierStackEditor : Editor {

		private MonoScript script;
		private SerializedProperty _positionType;
		private Texture2D _magnifier;

		private void OnEnable() {
			script = MonoScript.FromScriptableObject((ModifierStack)target);
			_positionType = serializedObject.FindProperty("moveFeaturePositionTo");
			_magnifier = EditorGUIUtility.FindTexture("d_ViewToolZoom");
		}

		public override void OnInspectorGUI() {
			serializedObject.Update();
			GUI.enabled = false;
			script = EditorGUILayout.ObjectField("Script", script, typeof(MonoScript), false) as MonoScript;
			GUI.enabled = true;

			EditorGUILayout.Space();
			EditorGUILayout.PropertyField(_positionType, new GUIContent("Feature Position"));

			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Mesh Modifiers");
			SerializedProperty meshfac = serializedObject.FindProperty("MeshModifiers");

			for (int i = 0; i < meshfac.arraySize; i++) {
				int ind = i;
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.BeginVertical();
				GUILayout.Space(5);
				meshfac.GetArrayElementAtIndex(ind).objectReferenceValue = EditorGUILayout.ObjectField(
					meshfac.GetArrayElementAtIndex(i).objectReferenceValue, typeof(MeshModifier), false
				) as ScriptableObject;

				EditorGUILayout.EndVertical();

				if (GUILayout.Button(_magnifier, (GUIStyle)"minibuttonleft", GUILayout.Width(30))) {
					ScriptableCreatorWindow.Open(typeof(MeshModifier), meshfac, ind);
				}

				if (GUILayout.Button(
					    new GUIContent("-"), (GUIStyle)"minibuttonright", GUILayout.Width(30), GUILayout.Height(22)
				    )) {
					meshfac.DeleteArrayElementAtIndex(ind);
				}

				EditorGUILayout.EndHorizontal();
			}

			EditorGUILayout.Space();
			EditorGUILayout.BeginHorizontal();

			if (GUILayout.Button(new GUIContent("Add New Empty"), (GUIStyle)"minibuttonleft")) {
				meshfac.arraySize++;
				meshfac.GetArrayElementAtIndex(meshfac.arraySize - 1).objectReferenceValue = null;
			}

			if (GUILayout.Button(new GUIContent("Find Asset"), (GUIStyle)"minibuttonright")) {
				ScriptableCreatorWindow.Open(typeof(MeshModifier), meshfac);
			}

			EditorGUILayout.EndHorizontal();

			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Game Object Modifiers");
			SerializedProperty gofac = serializedObject.FindProperty("GoModifiers");

			for (int i = 0; i < gofac.arraySize; i++) {
				int ind = i;
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.BeginVertical();
				GUILayout.Space(5);
				gofac.GetArrayElementAtIndex(ind).objectReferenceValue = EditorGUILayout.ObjectField(
					gofac.GetArrayElementAtIndex(i).objectReferenceValue, typeof(GameObjectModifier), false
				) as ScriptableObject;

				EditorGUILayout.EndVertical();

				if (GUILayout.Button(_magnifier, (GUIStyle)"minibuttonleft", GUILayout.Width(30))) {
					ScriptableCreatorWindow.Open(typeof(GameObjectModifier), gofac, ind);
				}

				if (GUILayout.Button(
					    new GUIContent("-"), (GUIStyle)"minibuttonright", GUILayout.Width(30), GUILayout.Height(22)
				    )) {
					gofac.DeleteArrayElementAtIndex(ind);
				}

				EditorGUILayout.EndHorizontal();
			}

			EditorGUILayout.Space();
			EditorGUILayout.BeginHorizontal();

			if (GUILayout.Button(new GUIContent("Add New Empty"), (GUIStyle)"minibuttonleft")) {
				gofac.arraySize++;
				gofac.GetArrayElementAtIndex(gofac.arraySize - 1).objectReferenceValue = null;
			}

			if (GUILayout.Button(new GUIContent("Find Asset"), (GUIStyle)"minibuttonright")) {
				ScriptableCreatorWindow.Open(typeof(GameObjectModifier), gofac);
			}

			EditorGUILayout.EndHorizontal();

			serializedObject.ApplyModifiedProperties();
		}

	}

}