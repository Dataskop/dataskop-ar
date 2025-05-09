namespace Mapbox.Editor {

	using UnityEngine;
	using Mapbox.Unity.Map;
	using UnityEditor;
	using System;
	using System.Collections.Generic;
	using VectorTile.ExtensionMethods;

	[CustomPropertyDrawer(typeof(PrefabItemOptions))]
	public class PrefabItemOptionsDrawer : PropertyDrawer {

		private static float _lineHeight = EditorGUIUtility.singleLineHeight;
		private const string searchButtonContent = "Search";

		private GUIContent prefabLocationsTitle = new() {
			text = "Prefab Locations",
			tooltip = "Where on the map to spawn the selected prefab"
		};

		private GUIContent findByDropDown = new() {
			text = "Find by",
			tooltip = "Find points-of-interest by category, name, or address"
		};

		private GUIContent categoriesDropDown = new() {
			text = "Category",
			tooltip = "Spawn at locations in the categories selected"
		};

		private GUIContent densitySlider = new() {
			text = "Density",
			tooltip = "The number of prefabs to spawn per-tile; try a lower number if the map is cluttered"
		};

		private GUIContent nameField = new() {
			text = "Name",
			tooltip = "Spawn at locations containing this name string"
		};

		private GUIContent[] findByPropContent;
		private bool isGUIContentSet = false;

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			GUILayout.Space(-_lineHeight);
			SerializedProperty prefabItemCoreOptions = property.FindPropertyRelative("coreOptions");
			GUILayout.Label(prefabItemCoreOptions.FindPropertyRelative("sublayerName").stringValue + " Properties");

			//Prefab Game Object
			EditorGUI.indentLevel++;
			SerializedProperty spawnPrefabOptions = property.FindPropertyRelative("spawnPrefabOptions");

			EditorGUILayout.PropertyField(spawnPrefabOptions);

			GUILayout.Space(1);
			EditorGUI.indentLevel--;

			//Prefab Locations title
			GUILayout.Label(prefabLocationsTitle);

			//FindBy drop down
			EditorGUILayout.BeginHorizontal();

			SerializedProperty findByProp = property.FindPropertyRelative("findByType");

			string[] displayNames = findByProp.enumDisplayNames;
			int count = findByProp.enumDisplayNames.Length;

			if (!isGUIContentSet) {
				findByPropContent = new GUIContent[count];

				for (int extIdx = 0; extIdx < count; extIdx++) {
					findByPropContent[extIdx] = new GUIContent {
						text = displayNames[extIdx],
						tooltip = ((LocationPrefabFindBy)extIdx).Description()
					};
				}

				isGUIContentSet = true;
			}

			EditorGUI.indentLevel++;

			EditorGUI.BeginChangeCheck();
			findByProp.enumValueIndex = EditorGUILayout.Popup(
				findByDropDown, findByProp.enumValueIndex, findByPropContent
			);

			if (EditorGUI.EndChangeCheck()) {
				EditorHelper.CheckForModifiedProperty(property);
			}

			EditorGUILayout.EndHorizontal();

			switch ((LocationPrefabFindBy)findByProp.enumValueIndex) {
				case LocationPrefabFindBy.MapboxCategory:
					ShowCategoryOptions(property);
					break;
				case LocationPrefabFindBy.AddressOrLatLon:
					ShowAddressOrLatLonUI(property);
					break;
				case LocationPrefabFindBy.POIName:
					ShowPOINames(property);
					break;
				default:
					break;
			}

			EditorGUI.indentLevel--;
		}

		private void ShowCategoryOptions(SerializedProperty property) {
			//Category drop down
			EditorGUI.BeginChangeCheck();
			SerializedProperty categoryProp = property.FindPropertyRelative("categories");
			categoryProp.intValue = (int)(LocationPrefabCategories)EditorGUILayout.EnumFlagsField(
				categoriesDropDown, (LocationPrefabCategories)categoryProp.intValue
			);

			if (EditorGUI.EndChangeCheck()) {
				EditorHelper.CheckForModifiedProperty(property);
			}

			ShowDensitySlider(property);
		}

		private void ShowAddressOrLatLonUI(SerializedProperty property) {
			//EditorGUILayout.BeginVertical();
			SerializedProperty coordinateProperties = property.FindPropertyRelative("coordinates");

			for (int i = 0; i < coordinateProperties.arraySize; i++) {
				EditorGUILayout.BeginHorizontal();
				//get the element to draw
				SerializedProperty coordinate = coordinateProperties.GetArrayElementAtIndex(i);

				//label for each location.
				string coordinateLabel = string.Format("Location {0}", i);

				// draw coordinate string.
				EditorGUI.BeginChangeCheck();
				coordinate.stringValue = EditorGUILayout.TextField(coordinateLabel, coordinate.stringValue);

				if (EditorGUI.EndChangeCheck()) {
					EditorHelper.CheckForModifiedProperty(property, true);
				}

				// draw search button.
				if (GUILayout.Button(
					    new GUIContent(searchButtonContent), (GUIStyle)"minibuttonleft", GUILayout.MaxWidth(100)
				    )) {
					object propertyObject = EditorHelper.GetTargetObjectOfProperty(property);
					GeocodeAttributeSearchWindow.Open(coordinate, propertyObject);
				}

				//include a remove button in the row
				if (GUILayout.Button(new GUIContent(" X "), (GUIStyle)"minibuttonright", GUILayout.MaxWidth(30))) {
					coordinateProperties.DeleteArrayElementAtIndex(i);
					EditorHelper.CheckForModifiedProperty(property);
				}

				EditorGUILayout.EndHorizontal();
			}

			EditorGUILayout.BeginHorizontal();
			GUILayout.Space(EditorGUIUtility.labelWidth - 3);

			if (GUILayout.Button(new GUIContent("Add Location"), (GUIStyle)"minibutton")) {
				coordinateProperties.arraySize++;
				SerializedProperty newElement =
					coordinateProperties.GetArrayElementAtIndex(coordinateProperties.arraySize - 1);

				newElement.stringValue = "";
				EditorHelper.CheckForModifiedProperty(property);
			}

			EditorGUILayout.EndHorizontal();
		}

		private void ShowPOINames(SerializedProperty property) {
			//Name field
			SerializedProperty categoryProp = property.FindPropertyRelative("nameString");

			EditorGUI.BeginChangeCheck();
			categoryProp.stringValue = EditorGUILayout.TextField(nameField, categoryProp.stringValue);

			if (EditorGUI.EndChangeCheck()) {
				EditorHelper.CheckForModifiedProperty(property);
			}

			ShowDensitySlider(property);
		}

		private void ShowDensitySlider(SerializedProperty property) {
			//Density slider
			SerializedProperty densityProp = property.FindPropertyRelative("density");

			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(densityProp, densitySlider);

			if (EditorGUI.EndChangeCheck()) {
				EditorHelper.CheckForModifiedProperty(property);
			}

			GUI.enabled = true;
			densityProp.serializedObject.ApplyModifiedProperties();
		}

		private Rect GetNewRect(Rect position) {
			return new Rect(position.x, position.y, position.width, _lineHeight);
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
			return _lineHeight;
		}

	}

}