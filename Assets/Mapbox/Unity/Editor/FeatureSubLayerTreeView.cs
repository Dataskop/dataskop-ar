namespace Mapbox.Editor {

	using System.Collections.Generic;
	using UnityEngine;
	using UnityEditor.IMGUI.Controls;
	using UnityEditor;
	using Mapbox.Unity.Map;

	internal class FeatureSubLayerTreeView : TreeViewWithTreeModel<FeatureTreeElement> {

		public SerializedProperty Layers;
		private float kToggleWidth = 18f;
		public int uniqueId;
		public static int uniqueIdPoI = 1000;
		public static int uniqueIdFeature = 3000;
		public int maxElementsAdded = 0;

		public bool hasChanged = false;

		private const float kRowHeights = 15f;
		private const float nameOffset = 15f;

		private MultiColumnHeaderState m_MultiColumnHeaderState;
		private GUIStyle columnStyle = new() {
			alignment = TextAnchor.MiddleCenter,
			normal = new GUIStyleState() {
				textColor = Color.white
			}
		};

		public FeatureSubLayerTreeView(TreeViewState state, MultiColumnHeader multicolumnHeader,
			TreeModel<FeatureTreeElement> model, int uniqueIdentifier = 3000) : base(state, multicolumnHeader, model) {
			showAlternatingRowBackgrounds = true;
			showBorder = true;
			customFoldoutYOffset =
				(kRowHeights - EditorGUIUtility.singleLineHeight) *
				0.5f; // center foldout in the row since we also center content. See RowGUI

			extraSpaceBeforeIconAndLabel = kToggleWidth;
			uniqueId = uniqueIdentifier;
			Reload();
		}

		protected override bool CanRename(TreeViewItem item) {
			// Only allow rename if we can show the rename overlay with a certain width (label might be clipped by other columns)
			Rect renameRect = GetRenameRect(treeViewRect, 0, item);
			return renameRect.width > 30;
		}

		protected override void RenameEnded(RenameEndedArgs args) {
			if (Layers == null || Layers.arraySize == 0) {
				return;
			}

			if (args.acceptedRename) {
				FeatureTreeElement element = treeModel.Find(args.itemID);
				element.name = string.IsNullOrEmpty(args.newName.Trim()) ? args.originalName : args.newName;
				element.Name = string.IsNullOrEmpty(args.newName.Trim()) ? args.originalName : args.newName;
				SerializedProperty layer = Layers.GetArrayElementAtIndex(args.itemID - uniqueId);
				layer.FindPropertyRelative("coreOptions.sublayerName").stringValue = element.name;
				Reload();
			}
		}

		protected override Rect GetRenameRect(Rect rowRect, int row, TreeViewItem item) {
			Rect cellRect = GetCellRectForTreeFoldouts(rowRect);
			cellRect.xMin = nameOffset;
			CenterRectUsingSingleLineHeight(ref cellRect);
			return base.GetRenameRect(cellRect, row, item);
		}

		public void RemoveItemFromTree(int id) {
			treeModel.RemoveElements(
				new List<int>() {
					id
				}
			);
		}

		public void AddElementToTree(SerializedProperty subLayer) {
			string name = subLayer.FindPropertyRelative("coreOptions.sublayerName").stringValue;
			int id = Layers.arraySize - 1 + uniqueId;

			if (treeModel.Find(id) != null) {
				Debug.Log(" found one. exiting");
				return;
			}

			string type = ((PresetFeatureType)subLayer.FindPropertyRelative("presetFeatureType").enumValueIndex)
				.ToString();

			FeatureTreeElement element = new(name, 0, id);
			element.Name = name;
			element.Type = type;
			treeModel.AddElement(element, treeModel.root, treeModel.numberOfDataElements - 1);
		}

		protected override void RowGUI(RowGUIArgs args) {
			TreeViewItem<FeatureTreeElement> rowItem = (TreeViewItem<FeatureTreeElement>)args.item;

			for (int i = 0; i < args.GetNumVisibleColumns(); ++i) {
				CellGUI(args.GetCellRect(i), rowItem, (FeatureSubLayerColumns)args.GetColumn(i), ref args);
			}
		}

		private void CellGUI(Rect cellRect, TreeViewItem<FeatureTreeElement> item, FeatureSubLayerColumns column,
			ref RowGUIArgs args) {
			// Center cell rect vertically (makes it easier to place controls, icons etc in the cells)
			if (Layers == null || Layers.arraySize == 0) {
				return;
			}

			if (Layers.arraySize <= args.item.id - uniqueId) {
				return;
			}

			SerializedProperty layer = Layers.GetArrayElementAtIndex(args.item.id - uniqueId);
			CenterRectUsingSingleLineHeight(ref cellRect);

			if (column == FeatureSubLayerColumns.Name) {
				Rect toggleRect = cellRect;
				toggleRect.x += GetContentIndent(item);
				toggleRect.width = kToggleWidth;

				EditorGUI.BeginChangeCheck();
				item.data.isActive = layer.FindPropertyRelative("coreOptions.isActive").boolValue;

				if (toggleRect.xMax < cellRect.xMax) {
					item.data.isActive = EditorGUI.Toggle(
						toggleRect, item.data.isActive
					); // hide when outside cell rect
				}

				layer.FindPropertyRelative("coreOptions.isActive").boolValue = item.data.isActive;

				if (EditorGUI.EndChangeCheck()) {
					VectorSubLayerProperties vectorSubLayerProperties =
						(VectorSubLayerProperties)EditorHelper.GetTargetObjectOfProperty(layer);

					EditorHelper.CheckForModifiedProperty(layer, vectorSubLayerProperties.coreOptions);
				}

				cellRect.xMin += nameOffset; // Adding some gap between the checkbox and the name
				args.rowRect = cellRect;

				layer.FindPropertyRelative("coreOptions.sublayerName").stringValue = item.data.Name;
				//This draws the name property
				base.RowGUI(args);
			}

			if (column == FeatureSubLayerColumns.Type) {
				cellRect.xMin += 15f; // Adding some gap between the checkbox and the name

				string typeString = ((PresetFeatureType)layer.FindPropertyRelative("presetFeatureType").intValue)
					.ToString();

				item.data.Type = typeString;
				EditorGUI.LabelField(cellRect, item.data.Type, columnStyle);
			}
		}

		// All columns
		private enum FeatureSubLayerColumns {

			Name,
			Type

		}

		public static MultiColumnHeaderState CreateDefaultMultiColumnHeaderState() {
			MultiColumnHeaderState.Column[] columns = new[] {
				//Name column
				new MultiColumnHeaderState.Column {
					headerContent = new GUIContent("Name"),
					contextMenuText = "Name",
					headerTextAlignment = TextAlignment.Center,
					autoResize = true,
					canSort = false,
					allowToggleVisibility = false
				},

				//Type column
				new MultiColumnHeaderState.Column {
					headerContent = new GUIContent("Type"),
					contextMenuText = "Type",
					headerTextAlignment = TextAlignment.Center,
					autoResize = true,
					canSort = false,
					allowToggleVisibility = false
				}
			};

			MultiColumnHeaderState state = new(columns);
			return state;
		}

	}

}