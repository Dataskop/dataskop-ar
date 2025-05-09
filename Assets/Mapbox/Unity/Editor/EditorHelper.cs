﻿using System.Collections;

namespace Mapbox.Editor {

	using UnityEngine;
	using UnityEditor;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using Mapbox.Unity.Map;
	using Unity.MeshGeneration.Data;

	/// <summary>
	/// EditorHelper class provides methods for working with serialzed properties.
	/// Methods in this class are based on the spacepuppy-unity-framework, available at the url below.
	/// https://github.com/lordofduct/spacepuppy-unity-framework/tree/d8d592e212b26cad53264421d22c3d26c6799923/SpacepuppyBaseEditor.
	/// </summary>
	public static class EditorHelper {

		[UnityEditor.Callbacks.DidReloadScripts]
		private static void OnScriptsReloaded() {
			if (Application.isEditor) {
				AbstractMap abstractMap = UnityEngine.Object.FindObjectOfType<AbstractMap>();

				if (abstractMap == null) {
					return;
				}

				UnityTile[] unityTiles = abstractMap.GetComponentsInChildren<UnityTile>();

				for (int i = 0; i < unityTiles.Length; i++) {
					UnityEngine.Object.DestroyImmediate(unityTiles[i].gameObject);
				}

				abstractMap.DestroyChildObjects();

				if (EditorApplication.isPlaying) {
					abstractMap.ResetMap();
					return;
				}

				if (abstractMap.IsEditorPreviewEnabled == true) {
					if (EditorApplication.isPlayingOrWillChangePlaymode) {
						return;
					}
					else {
						abstractMap.DisableEditorPreview();
						abstractMap.EnableEditorPreview();
					}
				}
			}
		}

		public static void CheckForModifiedProperty<T>(SerializedProperty property, T targetObject,
			bool forceHasChanged = false) {
			MapboxDataProperty targetObjectAsDataProperty = GetMapboxDataPropertyObject(targetObject);

			if (targetObjectAsDataProperty != null) {
				targetObjectAsDataProperty.HasChanged =
					forceHasChanged || property.serializedObject.ApplyModifiedProperties();
			}
		}

		public static void CheckForModifiedProperty(SerializedProperty property, bool forceHasChanged = false) {
			CheckForModifiedProperty(property, GetTargetObjectOfProperty(property), forceHasChanged);
		}

		public static MapboxDataProperty GetMapboxDataPropertyObject<T>(T targetObject) {
			return targetObject as MapboxDataProperty;
		}

		public static bool DidModifyProperty<T>(SerializedProperty property, T targetObject) {
			MapboxDataProperty targetObjectAsDataProperty = targetObject as MapboxDataProperty;
			return property.serializedObject.ApplyModifiedProperties() && targetObjectAsDataProperty != null;
		}

		public static bool DidModifyProperty(SerializedProperty property) {
			return DidModifyProperty(property, GetTargetObjectOfProperty(property));
		}

		public static IEnumerable<SerializedProperty> GetChildren(this SerializedProperty property) {
			property = property.Copy();
			SerializedProperty nextElement = property.Copy();
			bool hasNextElement = nextElement.NextVisible(false);

			if (!hasNextElement) {
				nextElement = null;
			}

			property.NextVisible(true);

			while (true) {
				if (SerializedProperty.EqualContents(property, nextElement)) {
					yield break;
				}

				yield return property;

				bool hasNext = property.NextVisible(false);

				if (!hasNext) {
					break;
				}
			}
		}

		public static Type GetTargetType(this SerializedObject obj) {
			if (obj == null) {
				return null;
			}

			if (obj.isEditingMultipleObjects) {
				UnityEngine.Object c = obj.targetObjects[0];
				return c.GetType();
			}
			else {
				return obj.targetObject.GetType();
			}
		}

		/// <summary>
		/// Gets the object the property represents.
		/// </summary>
		/// <param name="prop"></param>
		/// <returns></returns>
		public static object GetTargetObjectOfProperty(SerializedProperty prop) {
			string path = prop.propertyPath.Replace(".Array.data[", "[");
			object obj = prop.serializedObject.targetObject;
			string[] elements = path.Split('.');

			foreach (string element in elements) {
				if (element.Contains("[")) {
					string elementName = element.Substring(0, element.IndexOf("[", StringComparison.CurrentCulture));
					int index = Convert.ToInt32(
						element.Substring(element.IndexOf("[", StringComparison.CurrentCulture)).Replace("[", "")
							.Replace("]", "")
					);

					obj = GetValue_Imp(obj, elementName, index);
				}
				else {
					obj = GetValue_Imp(obj, element);
				}
			}

			return obj;
		}

		public static void SetTargetObjectOfProperty(SerializedProperty prop, object value) {
			string path = prop.propertyPath.Replace(".Array.data[", "[");
			object obj = prop.serializedObject.targetObject;
			string[] elements = path.Split('.');

			foreach (string element in elements.Take(elements.Length - 1)) {
				if (element.Contains("[")) {
					string elementName = element.Substring(0, element.IndexOf("[", StringComparison.CurrentCulture));
					int index = Convert.ToInt32(
						element.Substring(element.IndexOf("[", StringComparison.CurrentCulture)).Replace("[", "")
							.Replace("]", "")
					);

					obj = GetValue_Imp(obj, elementName, index);
				}
				else {
					obj = GetValue_Imp(obj, element);
				}
			}

			if (ReferenceEquals(obj, null)) {
				return;
			}

			try {
				string element = elements.Last();

				if (element.Contains("[")) {
					Type tp = obj.GetType();
					string elementName = element.Substring(0, element.IndexOf("[", StringComparison.CurrentCulture));
					int index = Convert.ToInt32(
						element.Substring(element.IndexOf("[", StringComparison.CurrentCulture)).Replace("[", "")
							.Replace("]", "")
					);

					FieldInfo field = tp.GetField(
						elementName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
					);

					IList arr = field.GetValue(obj) as IList;
					arr[index] = value;
				}
				else {
					Type tp = obj.GetType();
					FieldInfo field = tp.GetField(
						element, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
					);

					if (field != null) {
						field.SetValue(obj, value);
					}
				}

			}
			catch {
				return;
			}
		}

		/// <summary>
		/// Gets the object that the property is a member of
		/// </summary>
		/// <param name="prop"></param>
		/// <returns></returns>
		public static object GetTargetObjectWithProperty(SerializedProperty prop) {
			string path = prop.propertyPath.Replace(".Array.data[", "[");
			object obj = prop.serializedObject.targetObject;
			string[] elements = path.Split('.');

			foreach (string element in elements.Take(elements.Length - 1)) {
				if (element.Contains("[")) {
					string elementName = element.Substring(0, element.IndexOf("[", StringComparison.CurrentCulture));
					int index = Convert.ToInt32(
						element.Substring(element.IndexOf("[", StringComparison.CurrentCulture)).Replace("[", "")
							.Replace("]", "")
					);

					obj = GetValue_Imp(obj, elementName, index);
				}
				else {
					obj = GetValue_Imp(obj, element);
				}
			}

			return obj;
		}

		private static object GetValue_Imp(object source, string name) {
			if (source == null) {
				return null;
			}

			Type type = source.GetType();

			while (type != null) {
				FieldInfo f = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

				if (f != null) {
					return f.GetValue(source);
				}

				PropertyInfo p = type.GetProperty(
					name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase
				);

				if (p != null) {
					return p.GetValue(source, null);
				}

				type = type.BaseType;
			}

			return null;
		}

		private static object GetValue_Imp(object source, string name, int index) {
			IEnumerable enumerable = GetValue_Imp(source, name) as IEnumerable;

			if (enumerable == null) {
				return null;
			}

			IEnumerator enm = enumerable.GetEnumerator();

			for (int i = 0; i <= index; i++) {
				if (!enm.MoveNext()) {
					return null;
				}
			}

			return enm.Current;
		}

		public static int GetChildPropertyCount(SerializedProperty property, bool includeGrandChildren = false) {
			SerializedProperty pstart = property.Copy();
			SerializedProperty pend = property.GetEndProperty();
			int cnt = 0;

			pstart.Next(true);

			while (!SerializedProperty.EqualContents(pstart, pend)) {
				cnt++;
				pstart.Next(includeGrandChildren);
			}

			return cnt;
		}

	}

}