using System.Collections.Generic;
using Dataskop.Utils;
using Mapbox.Unity.Location;
using Mapbox.Unity.Utilities;
using UnityEngine;
using UnityEngine.Events;

namespace Dataskop.Data {

	/// <summary>
	/// Responsible for tracking if the user is in a predefined location area.
	/// </summary>
	public class LocationAreaManager : MonoBehaviour {

		[Header("References")]
		[SerializeField] private LocationProviderFactory locationProviderFactory;

		[SerializeField] [Space] private LocationData[] locationData;

		[Header("Events")]
		public UnityEvent<LocationArea> userAreaLocated;

		private LocationArea lastLocatedArea;

		private ISet<LocationArea> LocationAreas { get; set; }

		private void Start() {

			InitializeAreas(locationData);
			userAreaLocated?.Invoke(lastLocatedArea);

		}

		private void OnEnable() {
			locationProviderFactory.DefaultLocationProvider.OnLocationUpdated += CheckUserLocationInAreas;
		}

		private void OnDisable() {
			locationProviderFactory.DefaultLocationProvider.OnLocationUpdated -= CheckUserLocationInAreas;
		}

		private void InitializeAreas(IEnumerable<LocationData> locations) {

			LocationAreas = new HashSet<LocationArea>();

			foreach (LocationData data in locations)
			foreach (LocationData.Area area in data.areas) {

				LocationArea locArea = new() {
					AreaName = area.areaName,
					LocationName = data.locationName
				};

				foreach (string point in area.boundaryPoints) {
					locArea.LatLonShapePoints.Add(Conversions.StringToLatLon(point));
				}

				LocationAreas.Add(locArea);
			}

		}

		private void CheckUserLocationInAreas(Location userLocation) {

			foreach (LocationArea area in LocationAreas) {

				if (!GPSExtensions.IsCoordinateInPolygon(userLocation.LatitudeLongitude, area.LatLonShapePoints)) {
					continue;
				}

				lastLocatedArea = area;
				userAreaLocated?.Invoke(lastLocatedArea);
				return;

			}

			if (lastLocatedArea == null) {
				return;
			}

			lastLocatedArea = null;
			userAreaLocated?.Invoke(lastLocatedArea);

		}

	}

}