using System;

namespace Mapbox.Editor {

	using Unity.MeshGeneration.Modifiers;
	using UnityEditor;
	using UnityEngine;

	[CustomPropertyDrawer(typeof(AddMonoBehavioursModifierType))]
	internal class AddMonoBehavioursModifierDrawer : PropertyDrawer {

		private const int _offset = 40;
		private MonoScript _monoscript;

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			EditorGUI.BeginProperty(position, label, property);

			position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
			Rect scriptRect = new(position.x, position.y, position.width, position.height - _offset);
			Rect helpRect = new(position.x, position.y + _offset / 2, position.width, _offset);
			SerializedProperty typeStringProperty = property.FindPropertyRelative("_typeString");
			SerializedProperty monoscriptProperty = property.FindPropertyRelative("_script");

			EditorGUI.BeginChangeCheck();

			_monoscript = monoscriptProperty.objectReferenceValue as MonoScript;
			_monoscript = EditorGUI.ObjectField(scriptRect, _monoscript, typeof(MonoScript), false) as MonoScript;

			if (EditorGUI.EndChangeCheck()) {
				Type type = _monoscript.GetClass();

				if (type != null && type.IsSubclassOf(typeof(MonoBehaviour))) {
					monoscriptProperty.objectReferenceValue = _monoscript;
					typeStringProperty.stringValue = _monoscript.GetClass().ToString();
				}
				else {
					monoscriptProperty.objectReferenceValue = null;
					typeStringProperty.stringValue = "";
				}
			}

			if (monoscriptProperty.objectReferenceValue == null) {
				EditorGUI.HelpBox(helpRect, "Selected object is not a MonoBehaviour!", MessageType.Error);
			}

			EditorGUI.EndProperty();
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
			return base.GetPropertyHeight(property, label) + _offset;
		}

	}

}