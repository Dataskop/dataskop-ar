using System;
using UnityEngine;

namespace Dataskop.Data {

	[CreateAssetMenu(fileName = "Location", menuName = "Locations/Add Location...", order = 1)]
	public class LocationData : ScriptableObject {

		public string locationName;
		public Area[] areas;

		[Serializable]
		public struct Area {

			public string areaName;
			[Tooltip("Format: lat, lon")] public string[] boundaryPoints;

		}

	}

}