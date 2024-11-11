namespace Mapbox.Editor {

	using UnityEditor;
	using UnityEngine;
	using Mapbox.Unity.Map;
	using VectorTile.ExtensionMethods;

	[CustomPropertyDrawer(typeof(ElevationLayerProperties))]
	public class ElevationLayerPropertiesDrawer : PropertyDrawer {

		private string objectId = "";
		private static float lineHeight = EditorGUIUtility.singleLineHeight;
		private GUIContent[] sourceTypeContent;
		private bool isGUIContentSet = false;

		private bool ShowPosition
		{
			get => EditorPrefs.GetBool(objectId + "ElevationLayerProperties_showPosition");
			set => EditorPrefs.SetBool(objectId + "ElevationLayerProperties_showPosition", value);
		}

		private GUIContent _tilesetIdGui = new() {
			text = "Tileset Id",
			tooltip = "Id of the tileset."
		};

		private string CustomSourceTilesetId
		{
			get => EditorPrefs.GetString(objectId + "ElevationLayerProperties_customSourceTilesetId");
			set => EditorPrefs.SetString(objectId + "ElevationLayerProperties_customSourceTilesetId", value);
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			objectId = property.serializedObject.targetObject.GetInstanceID().ToString();

			SerializedProperty sourceTypeProperty = property.FindPropertyRelative("sourceType");

			string[] displayNames = sourceTypeProperty.enumDisplayNames;
			int count = sourceTypeProperty.enumDisplayNames.Length;

			if (!isGUIContentSet) {
				sourceTypeContent = new GUIContent[count];

				for (int extIdx = 0; extIdx < count; extIdx++) {
					sourceTypeContent[extIdx] = new GUIContent {
						text = displayNames[extIdx],
						tooltip = ((ElevationSourceType)extIdx).Description()
					};
				}

				isGUIContentSet = true;
			}

			GUIContent sourceTypeLabel = new GUIContent {
				text = "Data Source",
				tooltip = "Source tileset for Terrain."
			};

			EditorGUI.BeginChangeCheck();
			sourceTypeProperty.enumValueIndex = EditorGUILayout.Popup(
				sourceTypeLabel, sourceTypeProperty.enumValueIndex, sourceTypeContent
			);

			if (EditorGUI.EndChangeCheck()) {
				EditorHelper.CheckForModifiedProperty(property);
			}

			ElevationSourceType sourceTypeValue = (ElevationSourceType)sourceTypeProperty.enumValueIndex;

			SerializedProperty sourceOptionsProperty = property.FindPropertyRelative("sourceOptions");
			SerializedProperty layerSourceProperty = sourceOptionsProperty.FindPropertyRelative("layerSource");
			SerializedProperty layerSourceId = layerSourceProperty.FindPropertyRelative("Id");

			EditorGUI.BeginChangeCheck();

			switch (sourceTypeValue) {
				case ElevationSourceType.MapboxTerrain:
					Style sourcePropertyValue = MapboxDefaultElevation.GetParameters(sourceTypeValue);
					layerSourceId.stringValue = sourcePropertyValue.Id;
					GUI.enabled = false;
					EditorGUILayout.PropertyField(sourceOptionsProperty, _tilesetIdGui);
					GUI.enabled = true;
					break;
				case ElevationSourceType.Custom:
					layerSourceId.stringValue = string.IsNullOrEmpty(CustomSourceTilesetId)
						? MapboxDefaultElevation.GetParameters(ElevationSourceType.MapboxTerrain).Id
						: CustomSourceTilesetId;
					EditorGUILayout.PropertyField(sourceOptionsProperty, _tilesetIdGui);
					CustomSourceTilesetId = layerSourceId.stringValue;
					break;
				default:
					break;
			}

			if (EditorGUI.EndChangeCheck()) {
				EditorHelper.CheckForModifiedProperty(property);
			}

			SerializedProperty elevationLayerType = property.FindPropertyRelative("elevationLayerType");

			if (sourceTypeValue == ElevationSourceType.None) {
				GUI.enabled = false;
				elevationLayerType.enumValueIndex = (int)ElevationLayerType.FlatTerrain;
			}

			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(
				property.FindPropertyRelative("elevationLayerType"), new GUIContent {
					text = elevationLayerType.displayName,
					tooltip = ((ElevationLayerType)elevationLayerType.enumValueIndex).Description()
				}
			);

			if (EditorGUI.EndChangeCheck()) {
				EditorHelper.CheckForModifiedProperty(property);
			}

			if (sourceTypeValue == ElevationSourceType.None) {
				GUI.enabled = true;
			}

			GUILayout.Space(-lineHeight);
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(property.FindPropertyRelative("colliderOptions"), true);

			if (EditorGUI.EndChangeCheck()) {
				EditorHelper.CheckForModifiedProperty(property.FindPropertyRelative("colliderOptions"));
			}

			GUILayout.Space(2 * -lineHeight);

			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(property.FindPropertyRelative("requiredOptions"), true);
			GUILayout.Space(-lineHeight);

			if (EditorGUI.EndChangeCheck()) {
				EditorHelper.CheckForModifiedProperty(property);
			}

			ShowPosition = EditorGUILayout.Foldout(ShowPosition, "Others");

			if (ShowPosition) {
				EditorGUI.BeginChangeCheck();

				EditorGUILayout.PropertyField(property.FindPropertyRelative("modificationOptions"), true);

				EditorGUILayout.PropertyField(property.FindPropertyRelative("sideWallOptions"), true);

				EditorGUILayout.PropertyField(property.FindPropertyRelative("unityLayerOptions"), true);

				if (EditorGUI.EndChangeCheck()) {
					EditorHelper.CheckForModifiedProperty(property);
				}
			}
		}

	}

}