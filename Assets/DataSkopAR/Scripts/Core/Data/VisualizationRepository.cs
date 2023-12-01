using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DataSkopAR.Data {

	public class VisualizationRepository : MonoBehaviour {

#region Fields

		[SerializeField] private GameObject dotVis;
		[SerializeField] private GameObject bubbleVis;
		[SerializeField] private GameObject barVis;
		private readonly Dictionary<VisualizationType, GameObject> visTypeDict = new();
		private readonly List<string> availableVisTypes = new();

#endregion

#region Methods

		private void Start() {
			visTypeDict.Add(VisualizationType.dot, dotVis);
			visTypeDict.Add(VisualizationType.bubble, bubbleVis);
			visTypeDict.Add(VisualizationType.bar, barVis);

			foreach (VisualizationType visType in visTypeDict.Keys.ToList()) availableVisTypes.Add(visType.ToString());

		}

		public GameObject GetVisualization(VisualizationType type) {
			return visTypeDict[type];
		}

		public List<VisualizationType> GetAvailableVisualizations() {
			return visTypeDict.Keys.ToList();
		}

		public bool IsAvailable(string visName) {

			if (availableVisTypes.Contains(visName))
				return true;

			return false;

		}

		public GameObject GetVisualizationByName(string visName) {
			Enum.TryParse(visName, out VisualizationType visType);
			return GetVisualization(visType);
		}

#endregion

	}

}