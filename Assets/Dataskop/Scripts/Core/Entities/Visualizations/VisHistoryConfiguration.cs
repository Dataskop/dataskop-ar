using System;
using UnityEngine;

namespace Dataskop.Entities.Visualizations {

	[Serializable]
	public struct VisHistoryConfiguration {

		public int visibleHistoryCount;
		public float elementDistance;
		public float animationDuration;
		public bool isFading;

		public VisHistoryConfiguration(int visibleHistoryCount, float elementDistance,
			float animationDuration, bool isFading) {
			this.visibleHistoryCount = visibleHistoryCount;
			this.elementDistance = elementDistance;
			this.animationDuration = animationDuration;
			this.isFading = isFading;
		}

	}

}