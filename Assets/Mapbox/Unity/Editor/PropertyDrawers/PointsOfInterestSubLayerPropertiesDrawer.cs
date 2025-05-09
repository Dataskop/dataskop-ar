namespace Mapbox.Unity.Map {

	using UnityEngine;
	using System.Collections.Generic;
	using UnityEditor;
	using Editor;
	using UnityEditor.IMGUI.Controls;
	using System.Linq;

	public class PointsOfInterestSubLayerPropertiesDrawer {

		private string objectId = "";
		private static float _lineHeight = EditorGUIUtility.singleLineHeight;

		private FeatureSubLayerTreeView layerTreeView;
		private IList<int> selectedLayers = new List<int>();

		private TreeModel<FeatureTreeElement> treeModel;
		[SerializeField] private TreeViewState m_TreeViewState;

		[SerializeField] private MultiColumnHeaderState m_MultiColumnHeaderState;

		private bool m_Initialized = false;
		public bool isLayerAdded = false;

		private int SelectionIndex
		{
			get => EditorPrefs.GetInt(objectId + "LocationPrefabsLayerProperties_selectionIndex");
			set => EditorPrefs.SetInt(objectId + "LocationPrefabsLayerProperties_selectionIndex", value);
		}

		public void DrawUI(SerializedProperty property) {
			objectId = property.serializedObject.targetObject.GetInstanceID().ToString();
			SerializedProperty prefabItemArray = property.FindPropertyRelative("locationPrefabList");
			Rect layersRect = EditorGUILayout.GetControlRect(
				GUILayout.MinHeight(
					Mathf.Max(prefabItemArray.arraySize + 1, 1) * _lineHeight +
					MultiColumnHeader.DefaultGUI.defaultHeight
				),
				GUILayout.MaxHeight(
					(prefabItemArray.arraySize + 1) * _lineHeight + MultiColumnHeader.DefaultGUI.defaultHeight
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

				treeModel = new TreeModel<FeatureTreeElement>(GetData(prefabItemArray));

				if (m_TreeViewState == null) {
					m_TreeViewState = new TreeViewState();
				}

				if (layerTreeView == null) {
					layerTreeView = new FeatureSubLayerTreeView(
						m_TreeViewState, multiColumnHeader, treeModel, FeatureSubLayerTreeView.uniqueIdPoI
					);
				}

				layerTreeView.multiColumnHeader = multiColumnHeader;
				m_Initialized = true;
			}

			layerTreeView.Layers = prefabItemArray;
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
				if (selectedLayers[0] - FeatureSubLayerTreeView.uniqueIdPoI > prefabItemArray.arraySize - 1) {
					selectedLayers[0] = prefabItemArray.arraySize - 1 + FeatureSubLayerTreeView.uniqueIdPoI;
				}

				SelectionIndex = selectedLayers[0];

			}
			else {
				selectedLayers = new int[1] {
					SelectionIndex
				};

				if (SelectionIndex > 0 && SelectionIndex - FeatureSubLayerTreeView.uniqueIdPoI <=
				    prefabItemArray.arraySize - 1) {
					layerTreeView.SetSelection(selectedLayers);
				}
			}

			GUILayout.Space(EditorGUIUtility.singleLineHeight);
			EditorGUILayout.BeginHorizontal();

			if (GUILayout.Button(new GUIContent("Add Layer"), (GUIStyle)"minibuttonleft")) {
				prefabItemArray.arraySize++;

				SerializedProperty prefabItem = prefabItemArray.GetArrayElementAtIndex(prefabItemArray.arraySize - 1);
				SerializedProperty prefabItemName = prefabItem.FindPropertyRelative("coreOptions.sublayerName");

				prefabItemName.stringValue = "New Location";

				// Set defaults here because SerializedProperty copies the previous element.
				prefabItem.FindPropertyRelative("coreOptions.isActive").boolValue = true;
				prefabItem.FindPropertyRelative("coreOptions.snapToTerrain").boolValue = true;
				prefabItem.FindPropertyRelative("presetFeatureType").enumValueIndex = (int)PresetFeatureType.Points;
				SerializedProperty categories = prefabItem.FindPropertyRelative("categories");
				categories.intValue = (int)LocationPrefabCategories.AnyCategory; //To select any category option

				SerializedProperty density = prefabItem.FindPropertyRelative("density");
				density.intValue = 15; //To select all locations option

				//Refreshing the tree
				layerTreeView.Layers = prefabItemArray;
				layerTreeView.AddElementToTree(prefabItem);
				layerTreeView.Reload();

				selectedLayers = new int[1] {
					prefabItemArray.arraySize - 1
				};

				layerTreeView.SetSelection(selectedLayers);

				if (EditorHelper.DidModifyProperty(property)) {
					isLayerAdded = true;
				}
			}

			if (GUILayout.Button(new GUIContent("Remove Selected"), (GUIStyle)"minibuttonright")) {
				foreach (int index in selectedLayers.OrderByDescending(i => i)) {
					if (layerTreeView != null) {
						SerializedProperty poiSubLayer =
							prefabItemArray.GetArrayElementAtIndex(index - FeatureSubLayerTreeView.uniqueIdPoI);

						VectorLayerProperties vectorLayerProperties =
							(VectorLayerProperties)EditorHelper.GetTargetObjectOfProperty(property);

						PrefabItemOptions poiSubLayerProperties =
							(PrefabItemOptions)EditorHelper.GetTargetObjectOfProperty(poiSubLayer);

						vectorLayerProperties.OnSubLayerPropertyRemoved(
							new VectorLayerUpdateArgs {
								property = poiSubLayerProperties
							}
						);

						layerTreeView.RemoveItemFromTree(index);
						prefabItemArray.DeleteArrayElementAtIndex(index - FeatureSubLayerTreeView.uniqueIdPoI);
						layerTreeView.treeModel.SetData(GetData(prefabItemArray));
					}
				}

				selectedLayers = new int[0];
				layerTreeView.SetSelection(selectedLayers);
			}

			EditorGUILayout.EndHorizontal();

			if (selectedLayers.Count == 1 && prefabItemArray.arraySize != 0 &&
			    selectedLayers[0] - FeatureSubLayerTreeView.uniqueIdPoI >= 0) {
				//ensure that selectedLayers[0] isn't out of bounds
				if (selectedLayers[0] - FeatureSubLayerTreeView.uniqueIdPoI > prefabItemArray.arraySize - 1) {
					selectedLayers[0] = prefabItemArray.arraySize - 1 + FeatureSubLayerTreeView.uniqueIdPoI;
				}

				SelectionIndex = selectedLayers[0];

				SerializedProperty layerProperty =
					prefabItemArray.GetArrayElementAtIndex(SelectionIndex - FeatureSubLayerTreeView.uniqueIdPoI);

				layerProperty.isExpanded = true;
				SerializedProperty subLayerCoreOptions = layerProperty.FindPropertyRelative("coreOptions");
				bool isLayerActive = subLayerCoreOptions.FindPropertyRelative("isActive").boolValue;

				if (!isLayerActive) {
					GUI.enabled = false;
				}

				DrawLayerLocationPrefabProperties(layerProperty, property);

				if (!isLayerActive) {
					GUI.enabled = true;
				}
			}
			else {
				GUILayout.Space(15);
				GUILayout.Label("Select a visualizer to see properties");
			}
		}

		private void DrawLayerLocationPrefabProperties(SerializedProperty layerProperty, SerializedProperty property) {
			EditorGUILayout.PropertyField(layerProperty);
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
				id = i + FeatureSubLayerTreeView.uniqueIdPoI;
				type = PresetFeatureType.Points.ToString();
				FeatureTreeElement element = new(name, 0, id);
				element.Name = name;
				element.name = name;
				element.Type = type;
				elements.Add(element);
			}

			return elements;
		}

	}

}