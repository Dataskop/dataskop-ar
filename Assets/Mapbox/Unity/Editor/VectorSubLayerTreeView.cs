﻿namespace Mapbox.Editor {

	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEditor.IMGUI.Controls;
	using UnityEditor;
	using Mapbox.Unity.Map;

	public class VectorSubLayerTreeView : TreeView {

		public SerializedProperty Layers;

		public VectorSubLayerTreeView(TreeViewState state)
			: base(state) {
			showAlternatingRowBackgrounds = true;
			showBorder = true;
			Reload();
		}

		protected override TreeViewItem BuildRoot() {
			// The root item is required to have a depth of -1, and the rest of the items increment from that.
			TreeViewItem root = new() {
				id = -1,
				depth = -1,
				displayName = "Root"
			};

			List<TreeViewItem> items = new();
			int index = 0;

			if (Layers != null) {
				for (int i = 0; i < Layers.arraySize; i++) {
					string name = Layers.GetArrayElementAtIndex(i).FindPropertyRelative("coreOptions.sublayerName")
						.stringValue;

					items.Add(
						new TreeViewItem {
							id = index,
							depth = 0,
							displayName = name
						}
					);

					index++;
				}
			}

			// Utility method that initializes the TreeViewItem.children and .parent for all items.
			SetupParentsAndChildrenFromDepths(root, items);

			// Return root of the tree
			return root;
		}

		protected override bool CanRename(TreeViewItem item) {
			return true;
		}

		protected override void RenameEnded(RenameEndedArgs args) {
			if (Layers != null) {
				SerializedProperty layer = Layers.GetArrayElementAtIndex(args.itemID);

				if (string.IsNullOrEmpty(args.newName.Trim())) {
					layer.FindPropertyRelative("coreOptions.sublayerName").stringValue = args.originalName;
				}
				else {
					layer.FindPropertyRelative("coreOptions.sublayerName").stringValue = args.newName;
				}
			}
		}

	}

}