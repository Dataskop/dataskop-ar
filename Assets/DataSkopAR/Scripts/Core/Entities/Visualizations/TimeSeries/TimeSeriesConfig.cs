using System;
using UnityEngine;

namespace DataskopAR.Entities.Visualizations {

	[Serializable]
	public struct TimeSeriesConfig {

		public GameObject elementVis;
		public int visibleHistoryCount;
		public float elementDistance;
		public float animationDuration;
		public bool isFading;

		public TimeSeriesConfig(GameObject elementVis, int visibleHistoryCount, float elementDistance,
			float animationDuration, bool isFading) {
			this.elementVis = elementVis;
			this.visibleHistoryCount = visibleHistoryCount;
			this.elementDistance = elementDistance;
			this.animationDuration = animationDuration;
			this.isFading = isFading;
		}

	}

}