using System;
using UnityEngine;

namespace Dataskop.Entities.Visualizations {

	[Serializable]
	public struct VisHistoryConfiguration {

		public GameObject elementVis;
		public int visibleHistoryCount;
		public float elementDistance;
		public float animationDuration;
		public bool isFading;

		public VisHistoryConfiguration(GameObject elementVis, int visibleHistoryCount, float elementDistance,
			float animationDuration, bool isFading) {
			this.elementVis = elementVis;
			this.visibleHistoryCount = visibleHistoryCount;
			this.elementDistance = elementDistance;
			this.animationDuration = animationDuration;
			this.isFading = isFading;
		}

	}

}