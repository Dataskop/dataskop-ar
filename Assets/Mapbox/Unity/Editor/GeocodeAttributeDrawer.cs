namespace Mapbox.Editor {

	using UnityEngine;
	using UnityEditor;
	using Unity.Utilities;
	using Mapbox.Unity.Map;

	/// <summary>
	/// Custom property drawer for geocodes <para/>
	/// Includes a search window to enable search of Lat/Lon via geocoder. 
	/// Requires a Mapbox token be set for the project
	/// </summary>
	[CustomPropertyDrawer(typeof(GeocodeAttribute))]
	public class GeocodeAttributeDrawer : PropertyDrawer {

		private const string searchButtonContent = "Search";

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			float buttonWidth = EditorGUIUtility.singleLineHeight * 4;

			Rect fieldRect = new(
				position.x, position.y, position.width - buttonWidth, EditorGUIUtility.singleLineHeight
			);

			Rect buttonRect = new(
				position.x + position.width - buttonWidth, position.y, buttonWidth, EditorGUIUtility.singleLineHeight
			);

			EditorGUI.PropertyField(fieldRect, property);

			if (GUI.Button(buttonRect, searchButtonContent)) {
				object objectToUpdate = EditorHelper.GetTargetObjectWithProperty(property);
				GeocodeAttributeSearchWindow.Open(property, objectToUpdate);
			}
		}

	}

}