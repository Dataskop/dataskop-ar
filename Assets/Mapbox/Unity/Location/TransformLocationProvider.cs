namespace Mapbox.Unity.Location {

	using System;
	using Map;
	using Utilities;
	using Utils;
	using UnityEngine;

	/// <summary>
	/// The TransformLocationProvider is responsible for providing mock location and heading data
	/// for testing purposes in the Unity editor.
	/// This is achieved by querying a Unity <see href="https://docs.unity3d.com/ScriptReference/Transform.html">Transform</see> every frame.
	/// You might use this to to update location based on a touched position, for example.
	/// </summary>
	public class TransformLocationProvider : AbstractEditorLocationProvider {

		/// <summary>
		/// The transform that will be queried for location and heading data.
		/// </summary>
		[SerializeField] private Transform _targetTransform;

		/// <summary>
		/// Sets the target transform.
		/// Use this if you want to switch the transform at runtime.
		/// </summary>
		public Transform TargetTransform
		{
			set => _targetTransform = value;
		}

		protected override void SetLocation() {
			AbstractMap _map = LocationProviderFactory.Instance.mapManager;
			_currentLocation.UserHeading = _targetTransform.eulerAngles.y;
			_currentLocation.LatitudeLongitude =
				_targetTransform.GetGeoPosition(_map.CenterMercator, _map.WorldRelativeScale);
			_currentLocation.Accuracy = _accuracy;
			_currentLocation.Timestamp = UnixTimestampUtils.To(DateTime.UtcNow);
			_currentLocation.IsLocationUpdated = true;
			_currentLocation.IsUserHeadingUpdated = true;
		}

	}

}