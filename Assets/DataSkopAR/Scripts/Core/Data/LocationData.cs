using System;
using UnityEngine;

namespace DataskopAR.Data {

	[CreateAssetMenu(fileName = "Location", menuName = "Locations/Add Location...", order = 1)]
	public class LocationData : ScriptableObject {

#region Subclasses

		[Serializable]
		public struct Area {

			public string areaName;
			[Tooltip("Format: lat, lon")] public string[] boundaryPoints;

		}

#endregion

#region Fields

		public string locationName;
		public Area[] areas;

#endregion

	}

}