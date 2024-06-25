using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Dataskop.Data {

	public class VisualizationRepository : MonoBehaviour {

		[SerializeField] private GameObject dotVis;
		[SerializeField] private GameObject bubbleVis;
		[SerializeField] private GameObject barVis;
		private readonly List<string> availableVisTypes = new();
		private readonly Dictionary<VisualizationType, GameObject> visTypeDict = new();

		private void Start() {

			visTypeDict.Add(VisualizationType.Dot, dotVis);
			visTypeDict.Add(VisualizationType.Bubble, bubbleVis);
			visTypeDict.Add(VisualizationType.Bar, barVis);

			foreach (VisualizationType visType in visTypeDict.Keys.ToList()) {
				availableVisTypes.Add(visType.ToString());
			}

		}

		public List<VisualizationType> GetAvailableVisualizations() {
			return visTypeDict.Keys.ToList();
		}

		public bool IsAvailable(string visName) {
			return availableVisTypes.Contains(visName);
		}

		public GameObject GetVisualization(VisualizationType type) {
			return visTypeDict[type];
		}

		public GameObject GetVisualization(string visName) {
			Enum.TryParse(visName, out VisualizationType visType);
			return GetVisualization(visType);
		}

	}

}