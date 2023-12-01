using System;
using UnityEngine;

namespace DataSkopAR.Entities.Visualizations {

	[Serializable]
	public struct TimeSeriesConfig {

		public GameObject elementVis;
		[Tooltip("Max Amount: 20")]public int visibleHistoryCount;
		public float elementDistance;
		public float animationDuration;
		public bool isFading;

		public TimeSeriesConfig(GameObject elementVis, int visibleHistoryCount, float elementDistance,
			float animationDuration, bool isFading) {
			this.elementVis = elementVis;
			this.visibleHistoryCount = Mathf.Clamp(visibleHistoryCount, 0, 20);
			this.elementDistance = elementDistance;
			this.animationDuration = animationDuration;
			this.isFading = isFading;
		}

	}

}