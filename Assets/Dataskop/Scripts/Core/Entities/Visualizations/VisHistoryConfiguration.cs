using System;

namespace Dataskop.Entities.Visualizations {

	[Serializable]
	public struct VisHistoryConfiguration {
		
		public float elementDistance;
		public float animationDuration;
		public bool isFading;

		public VisHistoryConfiguration(float elementDistance,
			float animationDuration, bool isFading) {
			this.elementDistance = elementDistance;
			this.animationDuration = animationDuration;
			this.isFading = isFading;
		}

	}

}