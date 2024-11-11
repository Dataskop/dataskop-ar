namespace Mapbox.Editor {

	using UnityEditor;
	using UnityEngine;
	using Mapbox.Unity.Map;
	using VectorTile.ExtensionMethods;

	[CustomPropertyDrawer(typeof(ImageryLayerProperties))]
	public class ImageryLayerPropertiesDrawer : PropertyDrawer {

		private GUIContent[] sourceTypeContent;
		private bool isGUIContentSet = false;

		private GUIContent _tilesetIdGui = new() {
			text = "Tileset Id",
			tooltip = "Id of the tileset."
		};

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			SerializedProperty sourceTypeProperty = property.FindPropertyRelative("sourceType");
			ImagerySourceType sourceTypeValue = (ImagerySourceType)sourceTypeProperty.enumValueIndex;

			string[] displayNames = sourceTypeProperty.enumDisplayNames;
			int count = sourceTypeProperty.enumDisplayNames.Length;

			if (!isGUIContentSet) {
				sourceTypeContent = new GUIContent[count];

				for (int extIdx = 0; extIdx < count; extIdx++) {
					sourceTypeContent[extIdx] = new GUIContent {
						text = displayNames[extIdx],
						tooltip = EnumExtensions.Description((ImagerySourceType)extIdx)
					};
				}

				isGUIContentSet = true;
			}

			// Draw label.
			GUIContent sourceTypeLabel = new GUIContent {
				text = "Data Source",
				tooltip = "Source tileset for Imagery."
			};

			EditorGUI.BeginChangeCheck();
			sourceTypeProperty.enumValueIndex = EditorGUILayout.Popup(
				sourceTypeLabel, sourceTypeProperty.enumValueIndex, sourceTypeContent
			);

			if (EditorGUI.EndChangeCheck()) {
				EditorHelper.CheckForModifiedProperty(property);
			}

			sourceTypeValue = (ImagerySourceType)sourceTypeProperty.enumValueIndex;

			SerializedProperty sourceOptionsProperty = property.FindPropertyRelative("sourceOptions");
			SerializedProperty layerSourceProperty = sourceOptionsProperty.FindPropertyRelative("layerSource");
			SerializedProperty layerSourceId = layerSourceProperty.FindPropertyRelative("Id");

			EditorGUI.BeginChangeCheck();

			switch (sourceTypeValue) {
				case ImagerySourceType.MapboxStreets:
				case ImagerySourceType.MapboxOutdoors:
				case ImagerySourceType.MapboxDark:
				case ImagerySourceType.MapboxLight:
				case ImagerySourceType.MapboxSatellite:
				case ImagerySourceType.MapboxSatelliteStreet:
					Style sourcePropertyValue = MapboxDefaultImagery.GetParameters(sourceTypeValue);
					layerSourceId.stringValue = sourcePropertyValue.Id;
					GUI.enabled = false;
					EditorGUILayout.PropertyField(sourceOptionsProperty, _tilesetIdGui);
					GUI.enabled = true;
					break;
				case ImagerySourceType.Custom:
					EditorGUILayout.PropertyField(
						sourceOptionsProperty, new GUIContent {
							text = "Tileset Id / Style URL",
							tooltip = _tilesetIdGui.tooltip
						}
					);
					break;
				case ImagerySourceType.None:
					break;
				default:
					break;
			}

			if (EditorGUI.EndChangeCheck()) {
				EditorHelper.CheckForModifiedProperty(property);
			}

			if (sourceTypeValue != ImagerySourceType.None) {
				EditorGUI.BeginChangeCheck();
				EditorGUILayout.PropertyField(property.FindPropertyRelative("rasterOptions"));

				if (EditorGUI.EndChangeCheck()) {
					EditorHelper.CheckForModifiedProperty(property);
				}
			}
		}

	}

}