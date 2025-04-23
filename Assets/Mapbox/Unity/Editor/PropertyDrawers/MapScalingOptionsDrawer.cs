namespace Mapbox.Editor {

	using UnityEditor;
	using UnityEngine;
	using Mapbox.Unity.Map;
	using VectorTile.ExtensionMethods;

	[CustomPropertyDrawer(typeof(MapScalingOptions))]
	public class MapScalingOptionsDrawer : PropertyDrawer {

		private static float lineHeight = EditorGUIUtility.singleLineHeight;
		private GUIContent[] scalingTypeContent;
		private bool isGUIContentSet = false;

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			SerializedProperty scalingType = property.FindPropertyRelative("scalingType");
			string[] displayNames = scalingType.enumDisplayNames;
			int count = scalingType.enumDisplayNames.Length;

			if (!isGUIContentSet) {
				scalingTypeContent = new GUIContent[count];

				for (int extIdx = 0; extIdx < count; extIdx++) {
					scalingTypeContent[extIdx] = new GUIContent {
						text = displayNames[extIdx],
						tooltip = EnumExtensions.Description((MapScalingType)extIdx)
					};
				}

				isGUIContentSet = true;
			}

			// Draw label.
			GUIContent scalingTypeLabel = new() {
				text = label.text,
				tooltip = "Scale of map in game units."
			};

			scalingType.enumValueIndex = EditorGUILayout.Popup(
				scalingTypeLabel, scalingType.enumValueIndex, scalingTypeContent
			);

			if ((MapScalingType)scalingType.enumValueIndex == MapScalingType.Custom) {
				position.y += lineHeight;
				EditorGUILayout.PropertyField(property.FindPropertyRelative("unityTileSize"));
			}
		}

	}

}