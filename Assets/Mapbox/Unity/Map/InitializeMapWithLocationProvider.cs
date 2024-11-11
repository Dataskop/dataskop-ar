namespace Mapbox.Unity.Map {

	using System.Collections;
	using Location;
	using UnityEngine;

	public class InitializeMapWithLocationProvider : MonoBehaviour {

		[SerializeField] private AbstractMap _map;

		private ILocationProvider _locationProvider;

		private void Awake() {
			// Prevent double initialization of the map. 
			_map.InitializeOnStart = false;
		}

		protected virtual IEnumerator Start() {
			yield return null;
			_locationProvider = LocationProviderFactory.Instance.DefaultLocationProvider;
			_locationProvider.OnLocationUpdated += LocationProvider_OnLocationUpdated;
		}

		private void LocationProvider_OnLocationUpdated(Location location) {
			_locationProvider.OnLocationUpdated -= LocationProvider_OnLocationUpdated;
			_map.Initialize(location.LatitudeLongitude, _map.AbsoluteZoom);
		}

	}

}