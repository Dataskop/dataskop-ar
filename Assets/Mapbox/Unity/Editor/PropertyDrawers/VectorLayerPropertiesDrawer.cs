﻿namespace Mapbox.Editor {

	using System;
	using UnityEngine;
	using UnityEditor;
	using Mapbox.Unity.Map;

	public class VectorLayerPropertiesDrawer {

		private string objectId = "";

		/// <summary>
		/// Gets or sets a value to show or hide Vector section <see cref="T:Mapbox.Editor.MapManagerEditor"/>.
		/// </summary>
		/// <value><c>true</c> if show vector; otherwise, <c>false</c>.</value>
		private bool ShowLocationPrefabs
		{
			get => EditorPrefs.GetBool(objectId + "VectorLayerProperties_showLocationPrefabs");
			set => EditorPrefs.SetBool(objectId + "VectorLayerProperties_showLocationPrefabs", value);
		}

		/// <summary>
		/// Gets or sets a value to show or hide Vector section <see cref="T:Mapbox.Editor.MapManagerEditor"/>.
		/// </summary>
		/// <value><c>true</c> if show vector; otherwise, <c>false</c>.</value>
		private bool ShowFeatures
		{
			get => EditorPrefs.GetBool(objectId + "VectorLayerProperties_showFeatures");
			set => EditorPrefs.SetBool(objectId + "VectorLayerProperties_showFeatures", value);
		}

		private GUIContent _requiredTilesetIdGui = new() {
			text = "Required Tileset Id",
			tooltip =
				"For location prefabs to spawn the \"streets-v7\" tileset needs to be a part of the Vector data source"
		};

		private FeaturesSubLayerPropertiesDrawer _vectorSublayerDrawer = new();
		private PointsOfInterestSubLayerPropertiesDrawer _poiSublayerDrawer = new();

		private void ShowSepartor() {
			EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
			EditorGUILayout.Space();
		}

		public void DrawUI(SerializedProperty property) {
			objectId = property.serializedObject.targetObject.GetInstanceID().ToString();
			SerializedProperty layerSourceProperty = property.FindPropertyRelative("sourceOptions");
			SerializedProperty sourceTypeProperty = property.FindPropertyRelative("_sourceType");

			string[] names = sourceTypeProperty.enumNames;
			VectorSourceType sourceTypeValue = (VectorSourceType)Enum.Parse(
				typeof(VectorSourceType), names[sourceTypeProperty.enumValueIndex]
			);

			//VectorSourceType sourceTypeValue = (VectorSourceType)sourceTypeProperty.enumValueIndex;
			string streets_v7 = MapboxDefaultVector.GetParameters(VectorSourceType.MapboxStreets).Id;
			SerializedProperty layerSourceId = layerSourceProperty.FindPropertyRelative("layerSource.Id");
			string layerString = layerSourceId.stringValue;

			//Draw POI Section
			if (sourceTypeValue == VectorSourceType.None) {
				return;
			}

			ShowLocationPrefabs = EditorGUILayout.Foldout(ShowLocationPrefabs, "POINTS OF INTEREST");

			if (ShowLocationPrefabs) {
				if (sourceTypeValue != VectorSourceType.None && layerString.Contains(streets_v7)) {
					GUI.enabled = false;
					EditorGUILayout.TextField(_requiredTilesetIdGui, streets_v7);
					GUI.enabled = true;
					_poiSublayerDrawer.DrawUI(property);
				}
				else {
					EditorGUILayout.HelpBox(
						"In order to place points of interest please add \"mapbox.mapbox-streets-v7\" to the data source.",
						MessageType.Error
					);
				}
			}

			ShowSepartor();

			//Draw Feature section.
			ShowFeatures = EditorGUILayout.Foldout(ShowFeatures, "FEATURES");

			if (ShowFeatures) {
				_vectorSublayerDrawer.DrawUI(property);
			}
		}

		public void PostProcessLayerProperties(SerializedProperty property) {

			SerializedProperty layerSourceProperty = property.FindPropertyRelative("sourceOptions");
			SerializedProperty sourceTypeProperty = property.FindPropertyRelative("_sourceType");
			VectorSourceType sourceTypeValue = (VectorSourceType)sourceTypeProperty.enumValueIndex;
			string streets_v7 = MapboxDefaultVector.GetParameters(VectorSourceType.MapboxStreets).Id;
			SerializedProperty layerSourceId = layerSourceProperty.FindPropertyRelative("layerSource.Id");
			string layerString = layerSourceId.stringValue;

			if (ShowLocationPrefabs) {
				if (_poiSublayerDrawer.isLayerAdded == true && sourceTypeValue != VectorSourceType.None &&
				    layerString.Contains(streets_v7)) {
					SerializedProperty prefabItemArray = property.FindPropertyRelative("locationPrefabList");
					SerializedProperty prefabItem =
						prefabItemArray.GetArrayElementAtIndex(prefabItemArray.arraySize - 1);

					PrefabItemOptions prefabItemOptionToAdd =
						(PrefabItemOptions)EditorHelper.GetTargetObjectOfProperty(prefabItem) as PrefabItemOptions;

					((VectorLayerProperties)EditorHelper.GetTargetObjectOfProperty(property)).OnSubLayerPropertyAdded(
						new VectorLayerUpdateArgs {
							property = prefabItemOptionToAdd
						}
					);

					_poiSublayerDrawer.isLayerAdded = false;
				}
			}

			if (ShowFeatures) {
				if (_vectorSublayerDrawer.isLayerAdded == true) {
					SerializedProperty subLayerArray = property.FindPropertyRelative("vectorSubLayers");
					SerializedProperty subLayer = subLayerArray.GetArrayElementAtIndex(subLayerArray.arraySize - 1);
					((VectorLayerProperties)EditorHelper.GetTargetObjectOfProperty(property)).OnSubLayerPropertyAdded(
						new VectorLayerUpdateArgs {
							property = EditorHelper.GetTargetObjectOfProperty(subLayer) as MapboxDataProperty
						}
					);

					_vectorSublayerDrawer.isLayerAdded = false;
				}
			}

		}

	}

}