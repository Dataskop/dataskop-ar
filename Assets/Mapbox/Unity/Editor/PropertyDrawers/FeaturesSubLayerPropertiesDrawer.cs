﻿namespace Mapbox.Editor {

	using System.Collections.Generic;
	using System.Linq;
	using UnityEngine;
	using UnityEditor;
	using Mapbox.Unity.Map;
	using UnityEditor.IMGUI.Controls;
	using Unity.MeshGeneration.Modifiers;
	using VectorTile.ExtensionMethods;
	using Unity.MeshGeneration.Filters;
	using Platform.TilesetTileJSON;
	using Editor;
	using System;

	public class FeaturesSubLayerPropertiesDrawer {

		private static float _lineHeight = EditorGUIUtility.singleLineHeight;
		private GUIContent[] _sourceTypeContent;
		private bool _isGUIContentSet = false;
		private bool _isInitialized = false;
		private TileJsonData tileJSONData;
		private static TileJSONResponse tileJSONResponse;
		private static TileJsonData tileJsonData = new();
		private int _layerIndex = 0;
		private GUIContent[] _layerTypeContent;
		private static VectorSubLayerProperties subLayerProperties;
		private TreeModel<FeatureTreeElement> treeModel;

		private static string[] names;
		[SerializeField] private TreeViewState m_TreeViewState;

		[SerializeField] private MultiColumnHeaderState m_MultiColumnHeaderState;

		public bool isLayerAdded = false;
		private bool m_Initialized = false;
		private string objectId = "";

		private string TilesetId
		{
			get => EditorPrefs.GetString(objectId + "VectorSubLayerProperties_tilesetId");
			set => EditorPrefs.SetString(objectId + "VectorSubLayerProperties_tilesetId", value);
		}

		private bool ShowPosition
		{
			get => EditorPrefs.GetBool(objectId + "VectorSubLayerProperties_showPosition");
			set => EditorPrefs.SetBool(objectId + "VectorSubLayerProperties_showPosition", value);
		}

		private int SelectionIndex
		{
			get => EditorPrefs.GetInt(objectId + "VectorSubLayerProperties_selectionIndex");
			set => EditorPrefs.SetInt(objectId + "VectorSubLayerProperties_selectionIndex", value);
		}

		private ModelingSectionDrawer _modelingSectionDrawer = new();
		private BehaviorModifiersSectionDrawer _behaviorModifierSectionDrawer = new();

		private static TileStats _streetsV7TileStats;
		private static string[] subTypeValues;
		private FeatureSubLayerTreeView layerTreeView;
		private IList<int> selectedLayers = new List<int>();

		public void DrawUI(SerializedProperty property) {

			objectId = property.serializedObject.targetObject.GetInstanceID().ToString();
			SerializedObject serializedMapObject = property.serializedObject;
			AbstractMap mapObject = (AbstractMap)serializedMapObject.targetObject;
			tileJSONData = mapObject.VectorData.GetTileJsonData();

			SerializedProperty sourceTypeProperty = property.FindPropertyRelative("_sourceType");
			VectorSourceType sourceTypeValue = (VectorSourceType)sourceTypeProperty.enumValueIndex;

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

				//				for (int index0 = 0; index0 < count; index0++)
				//				{
				//					_sourceTypeContent[index0] = new GUIContent
				//					{
				//						text = displayNames[index0],
				//						tooltip = ((VectorSourceType)index0).Description(),
				//					};
				//				}
				_isGUIContentSet = true;
			}

			//sourceTypeValue = (VectorSourceType)sourceTypeProperty.enumValueIndex;
			sourceTypeValue = (VectorSourceType)Enum.Parse(
				typeof(VectorSourceType), names[sourceTypeProperty.enumValueIndex]
			);

			SerializedProperty sourceOptionsProperty = property.FindPropertyRelative("sourceOptions");
			SerializedProperty layerSourceProperty = sourceOptionsProperty.FindPropertyRelative("layerSource");
			SerializedProperty layerSourceId = layerSourceProperty.FindPropertyRelative("Id");
			SerializedProperty isActiveProperty = sourceOptionsProperty.FindPropertyRelative("isActive");

			switch (sourceTypeValue) {
				case VectorSourceType.MapboxStreets:
				case VectorSourceType.MapboxStreetsV8:
				case VectorSourceType.MapboxStreetsWithBuildingIds:
				case VectorSourceType.MapboxStreetsV8WithBuildingIds:
					Style sourcePropertyValue = MapboxDefaultVector.GetParameters(sourceTypeValue);
					layerSourceId.stringValue = sourcePropertyValue.Id;
					GUI.enabled = false;

					if (_isInitialized) {
						LoadEditorTileJSON(property, sourceTypeValue, layerSourceId.stringValue);
					}
					else {
						_isInitialized = true;
					}

					if (tileJSONData.PropertyDisplayNames.Count == 0 && tileJSONData.tileJSONLoaded) {
						EditorGUILayout.HelpBox(
							"Invalid Tileset Id / There might be a problem with the internet connection.",
							MessageType.Error
						);
					}

					GUI.enabled = true;
					isActiveProperty.boolValue = true;
					break;
				case VectorSourceType.Custom:
					if (_isInitialized) {
						string test = layerSourceId.stringValue;
						LoadEditorTileJSON(property, sourceTypeValue, layerSourceId.stringValue);
					}
					else {
						_isInitialized = true;
					}

					if (tileJSONData.PropertyDisplayNames.Count == 0 && tileJSONData.tileJSONLoaded) {
						EditorGUILayout.HelpBox(
							"Invalid Tileset Id / There might be a problem with the internet connection.",
							MessageType.Error
						);
					}

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
				EditorGUILayout.LabelField(
					new GUIContent {
						text = "Map Features",
						tooltip = "Visualizers for vector features contained in a layer. "
					}
				);

				SerializedProperty subLayerArray = property.FindPropertyRelative("vectorSubLayers");

				Rect layersRect = EditorGUILayout.GetControlRect(
					GUILayout.MinHeight(
						Mathf.Max(subLayerArray.arraySize + 1, 1) * _lineHeight +
						MultiColumnHeader.DefaultGUI.defaultHeight
					),
					GUILayout.MaxHeight(
						(subLayerArray.arraySize + 1) * _lineHeight + MultiColumnHeader.DefaultGUI.defaultHeight
					)
				);

				if (!m_Initialized) {
					bool firstInit = m_MultiColumnHeaderState == null;
					MultiColumnHeaderState headerState = FeatureSubLayerTreeView.CreateDefaultMultiColumnHeaderState();

					if (MultiColumnHeaderState.CanOverwriteSerializedFields(m_MultiColumnHeaderState, headerState)) {
						MultiColumnHeaderState.OverwriteSerializedFields(m_MultiColumnHeaderState, headerState);
					}

					m_MultiColumnHeaderState = headerState;

					FeatureSectionMultiColumnHeader multiColumnHeader = new(headerState);

					if (firstInit) {
						multiColumnHeader.ResizeToFit();
					}

					treeModel = new TreeModel<FeatureTreeElement>(GetData(subLayerArray));

					if (m_TreeViewState == null) {
						m_TreeViewState = new TreeViewState();
					}

					if (layerTreeView == null) {
						layerTreeView = new FeatureSubLayerTreeView(m_TreeViewState, multiColumnHeader, treeModel);
					}

					layerTreeView.multiColumnHeader = multiColumnHeader;
					m_Initialized = true;
				}

				layerTreeView.Layers = subLayerArray;
				layerTreeView.Reload();
				layerTreeView.OnGUI(layersRect);

				if (layerTreeView.hasChanged) {
					EditorHelper.CheckForModifiedProperty(property);
					layerTreeView.hasChanged = false;
				}

				selectedLayers = layerTreeView.GetSelection();

				//if there are selected elements, set the selection index at the first element.
				//if not, use the Selection index to persist the selection at the right index.
				if (selectedLayers.Count > 0) {
					//ensure that selectedLayers[0] isn't out of bounds
					if (selectedLayers[0] - FeatureSubLayerTreeView.uniqueIdFeature > subLayerArray.arraySize - 1) {
						selectedLayers[0] = subLayerArray.arraySize - 1 + FeatureSubLayerTreeView.uniqueIdFeature;
					}

					SelectionIndex = selectedLayers[0];
				}
				else {
					if (SelectionIndex > 0 && SelectionIndex - FeatureSubLayerTreeView.uniqueIdFeature <=
					    subLayerArray.arraySize - 1) {
						selectedLayers = new int[1] {
							SelectionIndex
						};

						layerTreeView.SetSelection(selectedLayers);
					}
				}

				GUILayout.Space(EditorGUIUtility.singleLineHeight);

				EditorGUILayout.BeginHorizontal();
				GenericMenu menu = new();

				foreach (string name in Enum.GetNames(typeof(PresetFeatureType))) {
					menu.AddItem(
						new GUIContent() {
							text = name
						}, false, FetchPresetProperties, name
					);
				}

				GUILayout.Space(0); // do not remove this line; it is needed for the next line to work
				Rect rect = GUILayoutUtility.GetLastRect();
				rect.y += 2 * _lineHeight / 3;

				if (EditorGUILayout.DropdownButton(
					    new GUIContent {
						    text = "Add Feature"
					    }, FocusType.Passive, (GUIStyle)"minibuttonleft"
				    )) {
					menu.DropDown(rect);
				}

				//Assign subLayerProperties after fetching it from the presets class. This happens everytime an element is added
				if (subLayerProperties != null) {
					subLayerArray.arraySize++;
					SerializedProperty subLayer = subLayerArray.GetArrayElementAtIndex(subLayerArray.arraySize - 1);
					SetSubLayerProps(subLayer);

					//Refreshing the tree
					layerTreeView.Layers = subLayerArray;
					layerTreeView.AddElementToTree(subLayer);
					layerTreeView.Reload();

					selectedLayers = new int[1] {
						subLayerArray.arraySize - 1 + FeatureSubLayerTreeView.uniqueIdFeature
					};

					layerTreeView.SetSelection(selectedLayers);
					subLayerProperties = null; // setting this to null so that the if block is not called again

					if (EditorHelper.DidModifyProperty(property)) {
						isLayerAdded = true;
					}
				}

				if (GUILayout.Button(new GUIContent("Remove Selected"), (GUIStyle)"minibuttonright")) {
					foreach (int index in selectedLayers.OrderByDescending(i => i)) {
						if (layerTreeView != null) {
							SerializedProperty subLayer =
								subLayerArray.GetArrayElementAtIndex(index - FeatureSubLayerTreeView.uniqueIdFeature);

							VectorLayerProperties vectorLayerProperties =
								(VectorLayerProperties)EditorHelper.GetTargetObjectOfProperty(property);

							VectorSubLayerProperties vectorSubLayerProperties =
								(VectorSubLayerProperties)EditorHelper.GetTargetObjectOfProperty(subLayer);

							vectorLayerProperties.OnSubLayerPropertyRemoved(
								new VectorLayerUpdateArgs {
									property = vectorSubLayerProperties
								}
							);

							layerTreeView.RemoveItemFromTree(index);
							subLayerArray.DeleteArrayElementAtIndex(index - FeatureSubLayerTreeView.uniqueIdFeature);
							layerTreeView.treeModel.SetData(GetData(subLayerArray));
						}
					}

					selectedLayers = new int[0];
					layerTreeView.SetSelection(selectedLayers);
				}

				EditorGUILayout.EndHorizontal();

				GUILayout.Space(EditorGUIUtility.singleLineHeight);

				if (selectedLayers.Count == 1 && subLayerArray.arraySize != 0 &&
				    selectedLayers[0] - FeatureSubLayerTreeView.uniqueIdFeature >= 0) {
					//ensure that selectedLayers[0] isn't out of bounds
					if (selectedLayers[0] - FeatureSubLayerTreeView.uniqueIdFeature > subLayerArray.arraySize - 1) {
						selectedLayers[0] = subLayerArray.arraySize - 1 + FeatureSubLayerTreeView.uniqueIdFeature;
					}

					SelectionIndex = selectedLayers[0];

					SerializedProperty layerProperty =
						subLayerArray.GetArrayElementAtIndex(SelectionIndex - FeatureSubLayerTreeView.uniqueIdFeature);

					layerProperty.isExpanded = true;
					SerializedProperty subLayerCoreOptions = layerProperty.FindPropertyRelative("coreOptions");
					bool isLayerActive = subLayerCoreOptions.FindPropertyRelative("isActive").boolValue;

					if (!isLayerActive) {
						GUI.enabled = false;
					}

					DrawLayerVisualizerProperties(sourceTypeValue, layerProperty, property);

					if (!isLayerActive) {
						GUI.enabled = true;
					}
				}
				else {
					GUILayout.Label("Select a visualizer to see properties");
				}
			}
		}

		private IList<FeatureTreeElement> GetData(SerializedProperty subLayerArray) {
			List<FeatureTreeElement> elements = new();
			string name = string.Empty;
			string type = string.Empty;
			int id = 0;
			FeatureTreeElement root = new("Root", -1, 0);
			elements.Add(root);

			for (int i = 0; i < subLayerArray.arraySize; i++) {
				SerializedProperty subLayer = subLayerArray.GetArrayElementAtIndex(i);
				name = subLayer.FindPropertyRelative("coreOptions.sublayerName").stringValue;
				id = i + FeatureSubLayerTreeView.uniqueIdFeature;
				type = ((PresetFeatureType)subLayer.FindPropertyRelative("presetFeatureType").enumValueIndex)
					.ToString();

				FeatureTreeElement element = new(name, 0, id);
				element.Name = name;
				element.name = name;
				element.Type = type;
				elements.Add(element);
			}

			return elements;
		}

		/// <summary>
		/// Fetches the preset properties using the supplied <see cref="PresetFeatureType">PresetFeatureType</see>
		/// </summary>
		/// <param name="name">Name.</param>
		private void FetchPresetProperties(object name) {
			PresetFeatureType featureType = (PresetFeatureType)Enum.Parse(typeof(PresetFeatureType), name.ToString());
			subLayerProperties = PresetSubLayerPropertiesFetcher.GetSubLayerProperties(featureType);
		}

		/// <summary>
		/// Sets the sub layer properties for the newly added layer
		/// </summary>
		/// <param name="subLayer">Sub layer.</param>
		private void SetSubLayerProps(SerializedProperty subLayer) {
			subLayer.FindPropertyRelative("coreOptions.sublayerName").stringValue =
				subLayerProperties.coreOptions.sublayerName;

			subLayer.FindPropertyRelative("presetFeatureType").enumValueIndex =
				(int)subLayerProperties.presetFeatureType;

			// Set defaults here because SerializedProperty copies the previous element.
			SerializedProperty subLayerCoreOptions = subLayer.FindPropertyRelative("coreOptions");
			CoreVectorLayerProperties coreOptions = subLayerProperties.coreOptions;
			subLayerCoreOptions.FindPropertyRelative("isActive").boolValue = coreOptions.isActive;
			subLayerCoreOptions.FindPropertyRelative("layerName").stringValue = coreOptions.layerName;
			subLayerCoreOptions.FindPropertyRelative("geometryType").enumValueIndex = (int)coreOptions.geometryType;
			subLayerCoreOptions.FindPropertyRelative("snapToTerrain").boolValue = coreOptions.snapToTerrain;
			subLayerCoreOptions.FindPropertyRelative("combineMeshes").boolValue = coreOptions.combineMeshes;

			SerializedProperty subLayerlineGeometryOptions = subLayer.FindPropertyRelative("lineGeometryOptions");
			LineGeometryOptions lineGeometryOptions = subLayerProperties.lineGeometryOptions;
			subLayerlineGeometryOptions.FindPropertyRelative("Width").floatValue = lineGeometryOptions.Width;
			subLayerlineGeometryOptions.FindPropertyRelative("CapType").enumValueIndex =
				(int)lineGeometryOptions.CapType;

			subLayerlineGeometryOptions.FindPropertyRelative("JoinType").enumValueIndex =
				(int)lineGeometryOptions.JoinType;

			subLayerlineGeometryOptions.FindPropertyRelative("MiterLimit").floatValue = lineGeometryOptions.MiterLimit;
			subLayerlineGeometryOptions.FindPropertyRelative("RoundLimit").floatValue = lineGeometryOptions.RoundLimit;

			SerializedProperty subLayerExtrusionOptions = subLayer.FindPropertyRelative("extrusionOptions");
			GeometryExtrusionOptions extrusionOptions = subLayerProperties.extrusionOptions;
			subLayerExtrusionOptions.FindPropertyRelative("extrusionType").enumValueIndex =
				(int)extrusionOptions.extrusionType;

			subLayerExtrusionOptions.FindPropertyRelative("extrusionGeometryType").enumValueIndex =
				(int)extrusionOptions.extrusionGeometryType;

			subLayerExtrusionOptions.FindPropertyRelative("propertyName").stringValue = extrusionOptions.propertyName;
			subLayerExtrusionOptions.FindPropertyRelative("extrusionScaleFactor").floatValue =
				extrusionOptions.extrusionScaleFactor;

			subLayerExtrusionOptions.FindPropertyRelative("maximumHeight").floatValue = extrusionOptions.maximumHeight;

			SerializedProperty subLayerFilterOptions = subLayer.FindPropertyRelative("filterOptions");
			VectorFilterOptions filterOptions = subLayerProperties.filterOptions;
			subLayerFilterOptions.FindPropertyRelative("filters").ClearArray();
			subLayerFilterOptions.FindPropertyRelative("combinerType").enumValueIndex = (int)filterOptions.combinerType;
			//Add any future filter related assignments here

			SerializedProperty subLayerGeometryMaterialOptions = subLayer.FindPropertyRelative("materialOptions");
			GeometryMaterialOptions materialOptions = subLayerProperties.materialOptions;
			subLayerGeometryMaterialOptions.FindPropertyRelative("style").enumValueIndex = (int)materialOptions.style;

			SerializedProperty mats = subLayerGeometryMaterialOptions.FindPropertyRelative("materials");
			mats.arraySize = 2;

			SerializedProperty topMatArray = mats.GetArrayElementAtIndex(0).FindPropertyRelative("Materials");
			SerializedProperty sideMatArray = mats.GetArrayElementAtIndex(1).FindPropertyRelative("Materials");

			if (topMatArray.arraySize == 0) {
				topMatArray.arraySize = 1;
			}

			if (sideMatArray.arraySize == 0) {
				sideMatArray.arraySize = 1;
			}

			SerializedProperty topMat = topMatArray.GetArrayElementAtIndex(0);
			SerializedProperty sideMat = sideMatArray.GetArrayElementAtIndex(0);

			SerializedProperty atlas = subLayerGeometryMaterialOptions.FindPropertyRelative("atlasInfo");
			SerializedProperty palette = subLayerGeometryMaterialOptions.FindPropertyRelative("colorPalette");
			SerializedProperty lightStyleOpacity =
				subLayerGeometryMaterialOptions.FindPropertyRelative("lightStyleOpacity");

			SerializedProperty darkStyleOpacity =
				subLayerGeometryMaterialOptions.FindPropertyRelative("darkStyleOpacity");

			SerializedProperty colorStyleColor =
				subLayerGeometryMaterialOptions.FindPropertyRelative("colorStyleColor");

			SerializedProperty customStyleOptions =
				subLayerGeometryMaterialOptions.FindPropertyRelative("customStyleOptions");

			topMat.objectReferenceValue = materialOptions.materials[0].Materials[0];
			sideMat.objectReferenceValue = materialOptions.materials[1].Materials[0];
			atlas.objectReferenceValue = materialOptions.atlasInfo;
			palette.objectReferenceValue = materialOptions.colorPalette;
			lightStyleOpacity.floatValue = materialOptions.lightStyleOpacity;
			darkStyleOpacity.floatValue = materialOptions.darkStyleOpacity;
			colorStyleColor.colorValue = materialOptions.colorStyleColor;
			//set custom style options.
			SerializedProperty customMats = customStyleOptions.FindPropertyRelative("materials");
			customMats.arraySize = 2;

			SerializedProperty customTopMatArray =
				customMats.GetArrayElementAtIndex(0).FindPropertyRelative("Materials");

			SerializedProperty customSideMatArray =
				customMats.GetArrayElementAtIndex(1).FindPropertyRelative("Materials");

			if (customTopMatArray.arraySize == 0) {
				customTopMatArray.arraySize = 1;
			}

			if (customSideMatArray.arraySize == 0) {
				customSideMatArray.arraySize = 1;
			}

			SerializedProperty customTopMat = customTopMatArray.GetArrayElementAtIndex(0);
			SerializedProperty customSideMat = customSideMatArray.GetArrayElementAtIndex(0);

			customTopMat.objectReferenceValue = materialOptions.customStyleOptions.materials[0].Materials[0];
			customSideMat.objectReferenceValue = materialOptions.customStyleOptions.materials[1].Materials[0];
			customStyleOptions.FindPropertyRelative("atlasInfo").objectReferenceValue =
				materialOptions.customStyleOptions.atlasInfo;

			customStyleOptions.FindPropertyRelative("colorPalette").objectReferenceValue =
				materialOptions.customStyleOptions.colorPalette;

			subLayer.FindPropertyRelative("buildingsWithUniqueIds").boolValue =
				subLayerProperties.buildingsWithUniqueIds;

			subLayer.FindPropertyRelative("moveFeaturePositionTo").enumValueIndex =
				(int)subLayerProperties.moveFeaturePositionTo;

			subLayer.FindPropertyRelative("MeshModifiers").ClearArray();
			subLayer.FindPropertyRelative("GoModifiers").ClearArray();

			SerializedProperty subLayerColliderOptions = subLayer.FindPropertyRelative("colliderOptions");
			subLayerColliderOptions.FindPropertyRelative("colliderType").enumValueIndex =
				(int)subLayerProperties.colliderOptions.colliderType;
		}

		private void UpdateMe() {
			Debug.Log("Update!");
		}

		private void DrawLayerVisualizerProperties(VectorSourceType sourceType, SerializedProperty layerProperty,
			SerializedProperty property) {
			SerializedProperty subLayerCoreOptions = layerProperty.FindPropertyRelative("coreOptions");

			string subLayerName = subLayerCoreOptions.FindPropertyRelative("sublayerName").stringValue;
			string visualizerLayer = subLayerCoreOptions.FindPropertyRelative("layerName").stringValue;
			PresetFeatureType subLayerType =
				PresetSubLayerPropertiesFetcher.GetPresetTypeFromLayerName(visualizerLayer);

			GUILayout.Space(-_lineHeight);
			layerProperty.FindPropertyRelative("presetFeatureType").intValue = (int)subLayerType;

			GUILayout.Space(_lineHeight);
			//*********************** LAYER NAME BEGINS ***********************************//
			VectorPrimitiveType primitiveTypeProp =
				(VectorPrimitiveType)subLayerCoreOptions.FindPropertyRelative("geometryType").enumValueIndex;

			SerializedObject serializedMapObject = property.serializedObject;
			AbstractMap mapObject = (AbstractMap)serializedMapObject.targetObject;
			tileJsonData = mapObject.VectorData.GetTileJsonData();

			List<string> layerDisplayNames = tileJsonData.LayerDisplayNames;

			EditorGUI.BeginChangeCheck();
			DrawLayerName(subLayerCoreOptions, layerDisplayNames);

			if (EditorGUI.EndChangeCheck()) {
				EditorHelper.CheckForModifiedProperty(subLayerCoreOptions);
			}
			//*********************** LAYER NAME ENDS ***********************************//

			EditorGUI.indentLevel++;

			//*********************** FILTERS SECTION BEGINS ***********************************//
			SerializedProperty filterOptions = layerProperty.FindPropertyRelative("filterOptions");
			filterOptions.FindPropertyRelative("_selectedLayerName").stringValue =
				subLayerCoreOptions.FindPropertyRelative("layerName").stringValue;

			GUILayout.Space(-_lineHeight);
			EditorGUILayout.PropertyField(filterOptions, new GUIContent("Filters"));
			//*********************** FILTERS SECTION ENDS ***********************************//

			//*********************** MODELING SECTION BEGINS ***********************************//
			_modelingSectionDrawer.DrawUI(subLayerCoreOptions, layerProperty, primitiveTypeProp);
			//*********************** MODELING SECTION ENDS ***********************************//

			//*********************** TEXTURING SECTION BEGINS ***********************************//
			if (primitiveTypeProp != VectorPrimitiveType.Point && primitiveTypeProp != VectorPrimitiveType.Custom) {
				GUILayout.Space(-_lineHeight);
				EditorGUILayout.PropertyField(layerProperty.FindPropertyRelative("materialOptions"));
			}
			//*********************** TEXTURING SECTION ENDS ***********************************//

			//*********************** GAMEPLAY SECTION BEGINS ***********************************//
			_behaviorModifierSectionDrawer.DrawUI(layerProperty, primitiveTypeProp, sourceType);
			//*********************** GAMEPLAY SECTION ENDS ***********************************//

			EditorGUI.indentLevel--;
		}

		private void LoadEditorTileJSON(SerializedProperty property, VectorSourceType sourceTypeValue,
			string sourceString) {
			if (sourceTypeValue != VectorSourceType.None && !string.IsNullOrEmpty(sourceString)) {
				if (tileJSONResponse == null || string.IsNullOrEmpty(sourceString) || sourceString != TilesetId) {
					try {
						Unity.MapboxAccess.Instance.TileJSON.Get(
							sourceString, (response) =>
							{
								//if the code has reached this point it means that there is a valid access token
								tileJSONResponse = response;

								if (response == null || response.VectorLayers == null) //indicates bad tileresponse
								{
									tileJSONData.ClearData();
									return;
								}

								tileJSONData.ProcessTileJSONData(response);
							}
						);
					}
					catch (Exception) {
						//no valid access token causes MapboxAccess to throw an error and hence setting this property
						tileJSONData.ClearData();
					}
				}
				else if (tileJSONData.LayerPropertyDescriptionDictionary.Count == 0) {
					tileJSONData.ProcessTileJSONData(tileJSONResponse);
				}
			}
			else {
				tileJSONData.ClearData();
			}

			TilesetId = sourceString;
		}

		public void DrawLayerName(SerializedProperty property, List<string> layerDisplayNames) {

			GUIContent layerNameLabel = new() {
				text = "Data Layer",
				tooltip = "The layer name from the Mapbox tileset that would be used for visualizing a feature"
			};

			//disable the selection if there is no layer
			if (layerDisplayNames.Count == 0) {
				EditorGUILayout.LabelField(
					layerNameLabel, new GUIContent("No layers found: Invalid TilesetId / No Internet."),
					(GUIStyle)"minipopUp"
				);

				return;
			}

			//check the string value at the current _layerIndex to verify that the stored index matches the property string.
			string layerString = property.FindPropertyRelative("layerName").stringValue;

			if (layerDisplayNames.Contains(layerString)) {
				//if the layer contains the current layerstring, set it's index to match
				_layerIndex = layerDisplayNames.FindIndex(s => s.Equals(layerString));

			}
			else {
				//if the selected layer isn't in the source, add a placeholder entry
				_layerIndex = 0;
				layerDisplayNames.Insert(0, layerString);

				if (!tileJsonData.LayerPropertyDescriptionDictionary.ContainsKey(layerString)) {
					tileJsonData.LayerPropertyDescriptionDictionary.Add(layerString, new Dictionary<string, string>());
				}

			}

			//create the display name guicontent array with an additional entry for the currently selected item
			_layerTypeContent = new GUIContent[layerDisplayNames.Count];

			for (int extIdx = 0; extIdx < layerDisplayNames.Count; extIdx++) {
				_layerTypeContent[extIdx] = new GUIContent {
					text = layerDisplayNames[extIdx]
				};
			}

			//draw the layer selection popup
			_layerIndex = EditorGUILayout.Popup(layerNameLabel, _layerIndex, _layerTypeContent);
			string parsedString = layerDisplayNames.ToArray()[_layerIndex].Split(
				new string[] {
					tileJsonData.commonLayersKey
				}, StringSplitOptions.None
			)[0].Trim();

			property.FindPropertyRelative("layerName").stringValue = parsedString;
		}

		private string[] GetSubTypeValues(SerializedProperty layerProperty, string visualizerLayer,
			VectorSourceType sourceType) {
			string[] typesArray = null;
			string roadLayer = layerProperty.FindPropertyRelative("roadLayer").stringValue;
			string landuseLayer = layerProperty.FindPropertyRelative("landuseLayer").stringValue;

			if (visualizerLayer == roadLayer || visualizerLayer == landuseLayer) {
				_streetsV7TileStats = TileStatsFetcher.Instance.GetTileStats(sourceType);

				if (_streetsV7TileStats != null && _streetsV7TileStats.layers != null &&
				    _streetsV7TileStats.layers.Length != 0) {
					foreach (LayerStats layer in _streetsV7TileStats.layers) {
						if (layer.layer != visualizerLayer) {
							continue;
						}

						string presetPropertyName = "";

						if (layer.layer == roadLayer) {
							presetPropertyName =
								layerProperty.FindPropertyRelative("roadLayer_TypeProperty").stringValue;
						}
						else if (layer.layer == landuseLayer) {
							presetPropertyName = layerProperty.FindPropertyRelative("landuseLayer_TypeProperty")
								.stringValue;
						}

						if (layer.attributes != null && layer.attributes.Length > 0) {
							foreach (AttributeStats attributeItem in layer.attributes) {
								if (attributeItem.attribute == presetPropertyName) {
									typesArray = attributeItem.values;
								}
							}
						}
					}
				}
			}

			return typesArray;
		}

	}

}