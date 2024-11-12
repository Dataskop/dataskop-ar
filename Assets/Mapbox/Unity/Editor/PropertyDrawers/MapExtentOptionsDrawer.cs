namespace Mapbox.Editor {

	using UnityEditor;
	using UnityEngine;
	using Mapbox.Unity.Map;
	using VectorTile.ExtensionMethods;

	[CustomPropertyDrawer(typeof(MapExtentOptions))]
	public class MapExtentOptionsDrawer : PropertyDrawer {

		private static string extTypePropertyName = "extentType";
		private static float _lineHeight = EditorGUIUtility.singleLineHeight;
		private GUIContent[] extentTypeContent;
		private bool isGUIContentSet = false;

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			SerializedProperty kindProperty = property.FindPropertyRelative(extTypePropertyName);
			string[] displayNames = kindProperty.enumDisplayNames;
			int count = kindProperty.enumDisplayNames.Length;

			if (!isGUIContentSet) {
				extentTypeContent = new GUIContent[count];

				for (int extIdx = 0; extIdx < count; extIdx++) {
					extentTypeContent[extIdx] = new GUIContent {
						text = displayNames[extIdx],
						tooltip = EnumExtensions.Description((MapExtentType)extIdx)
					};
				}

				isGUIContentSet = true;
			}

			// Draw label.
			GUIContent extentTypeLabel = new() {
				text = label.text,
				tooltip =
					"Options to determine the geographic extent of the world for which the map tiles will be requested."
			};

			EditorGUI.BeginChangeCheck();
			kindProperty.enumValueIndex = EditorGUILayout.Popup(
				extentTypeLabel, kindProperty.enumValueIndex, extentTypeContent, GUILayout.Height(_lineHeight)
			);

			MapExtentType kind = (MapExtentType)kindProperty.enumValueIndex;

			if (EditorGUI.EndChangeCheck()) {
				EditorHelper.CheckForModifiedProperty(property);
			}

			EditorGUI.indentLevel++;

			GUILayout.Space(-_lineHeight);
			SerializedProperty defaultExtentsProp = property.FindPropertyRelative("defaultExtents");
			EditorGUI.BeginChangeCheck();

			switch (kind) {
				case MapExtentType.CameraBounds:
					GUILayout.Space(_lineHeight);
					EditorGUILayout.PropertyField(
						defaultExtentsProp.FindPropertyRelative("cameraBoundsOptions"), new GUIContent {
							text = "CameraOptions-"
						}
					);

					break;
				case MapExtentType.RangeAroundCenter:
					EditorGUILayout.PropertyField(
						defaultExtentsProp.FindPropertyRelative("rangeAroundCenterOptions"), new GUIContent {
							text = "RangeAroundCenter"
						}
					);

					break;
				case MapExtentType.RangeAroundTransform:
					GUILayout.Space(_lineHeight);
					EditorGUILayout.PropertyField(
						defaultExtentsProp.FindPropertyRelative("rangeAroundTransformOptions"), new GUIContent {
							text = "RangeAroundTransform"
						}
					);

					break;
				default:
					break;
			}

			if (EditorGUI.EndChangeCheck()) {
				EditorHelper.CheckForModifiedProperty(defaultExtentsProp);
			}

			EditorGUI.indentLevel--;
		}

	}

}