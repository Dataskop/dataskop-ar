using System;
using System.Collections.Generic;
using DataskopAR.Utils;
using Mapbox.Unity.Location;
using Mapbox.Unity.Utilities;
using UnityEngine;
using UnityEngine.Events;

namespace DataskopAR.Data {

	/// <summary>
	///     Responsible for tracking if the user is in a predefined location area.
	/// </summary>
	public class LocationAreaManager : MonoBehaviour {

#region Properties

		private List<LocationArea> LocationAreas { get; set; }

#endregion

#region Fields

		[Header("References")]
		[SerializeField] private LocationProviderFactory locationProviderFactory;
		[SerializeField] [Space] private LocationData[] locationData;

		[Header("Events")]
		public UnityEvent<LocationArea> userAreaLocated;

		private LocationArea lastLocatedArea;

#endregion

#region Methods

		private void OnEnable() {
			locationProviderFactory.DefaultLocationProvider.OnLocationUpdated += CheckUserLocationInAreas;
		}

		private void Start() {
			
			InitializeAreas(locationData);
			userAreaLocated?.Invoke(lastLocatedArea);

		}

		private void InitializeAreas(IEnumerable<LocationData> locations) {

			LocationAreas = new List<LocationArea>();

			foreach (LocationData data in locations)
			foreach (LocationData.Area area in data.areas) {

				LocationArea locArea = new LocationArea {
					AreaName = area.areaName,
					LocationName = data.locationName
				};

				foreach (string point in area.boundaryPoints)
					locArea.LatLonShapePoints.Add(Conversions.StringToLatLon(point));

				LocationAreas.Add(locArea);
			}

		}

		private void CheckUserLocationInAreas(Location userLocation) {

			foreach (LocationArea area in LocationAreas) {

				if (!GPSExtensions.IsCoordinateInPolygon(userLocation.LatitudeLongitude, area.LatLonShapePoints))
					continue;

				lastLocatedArea = area;
				userAreaLocated?.Invoke(lastLocatedArea);
				return;

			}

			if (lastLocatedArea == null)
				return;

			lastLocatedArea = null;
			userAreaLocated?.Invoke(lastLocatedArea);

		}

		private void OnDisable() {
			locationProviderFactory.DefaultLocationProvider.OnLocationUpdated -= CheckUserLocationInAreas;
		}

#endregion

	}

}