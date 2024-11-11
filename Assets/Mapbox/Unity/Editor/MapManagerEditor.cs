using System;

namespace Mapbox.Editor {

	using UnityEngine;
	using UnityEditor;
	using Mapbox.Unity.Map;
	using Platform.TilesetTileJSON;
	using System.Collections.Generic;
	using VectorTile.ExtensionMethods;

	[CustomEditor(typeof(AbstractMap))]
	[CanEditMultipleObjects]
	public class MapManagerEditor : Editor {

		private string objectId = "";
		private Color previewButtonColor = new(0.7f, 1.0f, 0.7f);

		/// <summary>
		/// Gets or sets a value indicating whether to show general section <see cref="T:Mapbox.Editor.MapManagerEditor"/>.
		/// </summary>
		/// <value><c>true</c> then show general section; otherwise hide, <c>false</c>.</value>
		private bool ShowGeneral
		{
			get => EditorPrefs.GetBool(objectId + "MapManagerEditor_showGeneral");
			set => EditorPrefs.SetBool(objectId + "MapManagerEditor_showGeneral", value);
		}

		/// <summary>
		/// Gets or sets a value to show or hide Image section<see cref="T:Mapbox.Editor.MapManagerEditor"/>.
		/// </summary>
		/// <value><c>true</c> if show image; otherwise, <c>false</c>.</value>
		private bool ShowImage
		{
			get => EditorPrefs.GetBool(objectId + "MapManagerEditor_showImage");
			set => EditorPrefs.SetBool(objectId + "MapManagerEditor_showImage", value);
		}

		/// <summary>
		/// Gets or sets a value to show or hide Terrain section <see cref="T:Mapbox.Editor.MapManagerEditor"/>
		/// </summary>
		/// <value><c>true</c> if show terrain; otherwise, <c>false</c>.</value>
		private bool ShowTerrain
		{
			get => EditorPrefs.GetBool(objectId + "MapManagerEditor_showTerrain");
			set => EditorPrefs.SetBool(objectId + "MapManagerEditor_showTerrain", value);
		}

		/// <summary>
		/// Gets or sets a value to show or hide Map Layers section <see cref="T:Mapbox.Editor.MapManagerEditor"/> show features.
		/// </summary>
		/// <value><c>true</c> if show features; otherwise, <c>false</c>.</value>
		private bool ShowMapLayers
		{
			get => EditorPrefs.GetBool(objectId + "MapManagerEditor_showMapLayers");
			set => EditorPrefs.SetBool(objectId + "MapManagerEditor_showMapLayers", value);
		}

		private bool ShowPosition
		{
			get => EditorPrefs.GetBool(objectId + "MapManagerEditor_showPosition");
			set => EditorPrefs.SetBool(objectId + "MapManagerEditor_showPosition", value);
		}

		private GUIContent tilesetIdGui = new() {
			text = "Tileset Id",
			tooltip = "Id of the tileset."
		};

		private bool _isGUIContentSet = false;
		private GUIContent[] _sourceTypeContent;
		private static float _lineHeight = EditorGUIUtility.singleLineHeight;

		private VectorLayerPropertiesDrawer _vectorLayerDrawer = new();

		public override void OnInspectorGUI() {
			objectId = serializedObject.targetObject.GetInstanceID().ToString();
			serializedObject.Update();
			EditorGUILayout.BeginVertical();
			EditorGUILayout.Space();

			SerializedProperty previewOptions = serializedObject.FindProperty("_previewOptions");
			SerializedProperty prevProp = previewOptions.FindPropertyRelative("isPreviewEnabled");
			bool prev = prevProp.boolValue;

			Color guiColor = GUI.color;
			GUI.color = prev ? previewButtonColor : guiColor;

			GUIStyle style = new("Button");
			style.alignment = TextAnchor.MiddleCenter;

			if (!Application.isPlaying) {
				prevProp.boolValue = GUILayout.Toggle(prevProp.boolValue, "Enable Preview", style);
				GUI.color = guiColor;
				EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
			}

			ShowGeneral = EditorGUILayout.Foldout(
				ShowGeneral, new GUIContent {
					text = "GENERAL",
					tooltip = "Options related to map data"
				}
			);

			if (ShowGeneral) {
				DrawMapOptions(serializedObject);
			}

			ShowSepartor();

			ShowImage = EditorGUILayout.Foldout(ShowImage, "IMAGE");

			if (ShowImage) {
				GUILayout.Space(-1.5f * _lineHeight);
				ShowSection(serializedObject.FindProperty("_imagery"), "_layerProperty");
			}

			ShowSepartor();

			ShowTerrain = EditorGUILayout.Foldout(ShowTerrain, "TERRAIN");

			if (ShowTerrain) {
				GUILayout.Space(-1.5f * _lineHeight);
				ShowSection(serializedObject.FindProperty("_terrain"), "_layerProperty");
			}

			ShowSepartor();

			ShowMapLayers = EditorGUILayout.Foldout(ShowMapLayers, "MAP LAYERS");

			if (ShowMapLayers) {
				DrawMapLayerOptions();
			}

			EditorGUILayout.EndVertical();

			EditorGUILayout.Space();

			serializedObject.ApplyModifiedProperties();
			SerializedProperty vectorDataProperty = serializedObject.FindProperty("_vectorData");
			SerializedProperty layerProperty = vectorDataProperty.FindPropertyRelative("_layerProperty");
			_vectorLayerDrawer.PostProcessLayerProperties(layerProperty);

			if (!Application.isPlaying) {
				if (prevProp.boolValue && !prev) {
					((AbstractMap)serializedObject.targetObject).EnableEditorPreview();
				}
				else if (prev && !prevProp.boolValue) {
					((AbstractMap)serializedObject.targetObject).DisableEditorPreview();
				}
			}
		}

		private void ShowSection(SerializedProperty property, string propertyName) {
			EditorGUILayout.Space();
			EditorGUILayout.PropertyField(property.FindPropertyRelative(propertyName));
		}

		private void ShowSepartor() {
			EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
			EditorGUILayout.Space();
		}

		private void DrawMapOptions(SerializedObject mapObject) {
			SerializedProperty property = mapObject.FindProperty("_options");

			if (!((AbstractMap)serializedObject.targetObject).IsAccessTokenValid) {
				EditorGUILayout.HelpBox(
					"Invalid Access Token. Please add a valid access token using the Mapbox  > Setup Menu",
					MessageType.Error
				);
			}

			EditorGUILayout.LabelField("Location ", GUILayout.Height(_lineHeight));

			EditorGUILayout.PropertyField(property.FindPropertyRelative("locationOptions"));

			SerializedProperty extentOptions = property.FindPropertyRelative("extentOptions");
			SerializedProperty extentOptionsType = extentOptions.FindPropertyRelative("extentType");

			if ((MapExtentType)extentOptionsType.enumValueIndex == MapExtentType.Custom) {
				SerializedProperty tileProviderProperty = mapObject.FindProperty("_tileProvider");
				EditorGUI.BeginChangeCheck();
				EditorGUILayout.PropertyField(extentOptionsType);

				if (EditorGUI.EndChangeCheck()) {
					EditorHelper.CheckForModifiedProperty(extentOptions);
				}

				EditorGUI.indentLevel++;
				EditorGUILayout.PropertyField(tileProviderProperty);
				EditorGUI.indentLevel--;
			}
			else {
				GUILayout.Space(-_lineHeight);
				EditorGUILayout.PropertyField(extentOptions);
			}

			EditorGUI.BeginChangeCheck();

			EditorGUILayout.PropertyField(serializedObject.FindProperty("_initializeOnStart"));

			if (EditorGUI.EndChangeCheck()) {
				EditorHelper.CheckForModifiedProperty(property);
			}

			ShowPosition = EditorGUILayout.Foldout(ShowPosition, "Others");

			if (ShowPosition) {
				GUILayout.Space(-_lineHeight);

				EditorGUI.BeginChangeCheck();
				SerializedProperty placementOptions = property.FindPropertyRelative("placementOptions");
				EditorGUILayout.PropertyField(placementOptions);

				if (EditorGUI.EndChangeCheck()) {
					EditorHelper.CheckForModifiedProperty(placementOptions);
				}

				GUILayout.Space(-_lineHeight);

				EditorGUI.BeginChangeCheck();
				SerializedProperty scalingOptions = property.FindPropertyRelative("scalingOptions");
				EditorGUILayout.PropertyField(scalingOptions);

				if (EditorGUI.EndChangeCheck()) {
					EditorHelper.CheckForModifiedProperty(scalingOptions);
				}

				EditorGUI.BeginChangeCheck();
				EditorGUILayout.PropertyField(property.FindPropertyRelative("loadingTexture"));
				EditorGUILayout.PropertyField(property.FindPropertyRelative("tileMaterial"));

				if (EditorGUI.EndChangeCheck()) {
					EditorHelper.CheckForModifiedProperty(property);
				}
			}
		}

		private void DrawMapLayerOptions() {
			SerializedProperty vectorDataProperty = serializedObject.FindProperty("_vectorData");
			SerializedProperty layerProperty = vectorDataProperty.FindPropertyRelative("_layerProperty");
			SerializedProperty layerSourceProperty = layerProperty.FindPropertyRelative("sourceOptions");
			SerializedProperty sourceTypeProperty = layerProperty.FindPropertyRelative("_sourceType");
			VectorSourceType sourceTypeValue = (VectorSourceType)sourceTypeProperty.enumValueIndex;
			SerializedProperty layerSourceId = layerProperty.FindPropertyRelative("sourceOptions.layerSource.Id");
			string layerString = layerSourceId.stringValue;
			SerializedProperty isActiveProperty = layerSourceProperty.FindPropertyRelative("isActive");

			string[] displayNames = sourceTypeProperty.enumDisplayNames;
			string[] names = sourceTypeProperty.enumNames;
			int count = sourceTypeProperty.enumDisplayNames.Length;

			if (!_isGUIContentSet) {
				_sourceTypeContent = new GUIContent[count];

				int index = 0;

				foreach (string name in names) {
					_sourceTypeContent[index] = new GUIContent {
						text = displayNames[index],
						tooltip = ((VectorSourceType)Enum.Parse(typeof(VectorSourceType), name)).Description()
					};
					index++;
				}
				//
				//				for (int extIdx = 0; extIdx < count; extIdx++)
				//				{
				//					_sourceTypeContent[extIdx] = new GUIContent
				//					{
				//						text = displayNames[extIdx],
				//						tooltip = ((VectorSourceType)extIdx).Description(),
				//					};
				//				}

				_isGUIContentSet = true;
			}

			EditorGUI.BeginChangeCheck();
			sourceTypeProperty.enumValueIndex = EditorGUILayout.Popup(
				new GUIContent {
					text = "Data Source",
					tooltip = "Source tileset for Vector Data"
				}, sourceTypeProperty.enumValueIndex, _sourceTypeContent
			);

			//sourceTypeValue = (VectorSourceType)sourceTypeProperty.enumValueIndex;
			sourceTypeValue = (VectorSourceType)Enum.Parse(
				typeof(VectorSourceType), names[sourceTypeProperty.enumValueIndex]
			);

			switch (sourceTypeValue) {
				case VectorSourceType.MapboxStreets:
				case VectorSourceType.MapboxStreetsV8:
				case VectorSourceType.MapboxStreetsWithBuildingIds:
				case VectorSourceType.MapboxStreetsV8WithBuildingIds:
					Style sourcePropertyValue = MapboxDefaultVector.GetParameters(sourceTypeValue);
					layerSourceId.stringValue = sourcePropertyValue.Id;
					GUI.enabled = false;
					EditorGUILayout.PropertyField(layerSourceProperty, tilesetIdGui);
					GUI.enabled = true;
					isActiveProperty.boolValue = true;
					break;
				case VectorSourceType.Custom:
					EditorGUILayout.PropertyField(layerSourceProperty, tilesetIdGui);
					isActiveProperty.boolValue = true;
					break;
				case VectorSourceType.None:
					isActiveProperty.boolValue = false;
					break;
				default:
					isActiveProperty.boolValue = false;
					break;
			}

			if (sourceTypeValue != VectorSourceType.None) {
				SerializedProperty isStyleOptimized = layerProperty.FindPropertyRelative("useOptimizedStyle");
				EditorGUILayout.PropertyField(isStyleOptimized);

				if (isStyleOptimized.boolValue) {
					EditorGUILayout.PropertyField(
						layerProperty.FindPropertyRelative("optimizedStyle"), new GUIContent("Style Options")
					);
				}

				GUILayout.Space(-_lineHeight);
				EditorGUILayout.PropertyField(
					layerProperty.FindPropertyRelative("performanceOptions"), new GUIContent("Perfomance Options")
				);
			}

			EditorGUILayout.Space();
			ShowSepartor();

			_vectorLayerDrawer.DrawUI(layerProperty);

			if (EditorGUI.EndChangeCheck()) {
				EditorHelper.CheckForModifiedProperty(layerProperty);
			}
		}

	}

}