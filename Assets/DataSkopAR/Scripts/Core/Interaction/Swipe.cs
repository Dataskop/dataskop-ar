using UnityEngine;

namespace DataskopAR.Interaction {

	public struct Swipe {

#region Properties

		public Vector2 StartPoint { get; set; }

		public Vector2 EndPoint { get; set; }

		public float Distance { get; set; }

		public float XDistance { get; set; }

		public float YDistance { get; set; }

		public GameObject StartingGameObject { get; set; }

		public GameObject EndingGameObject { get; set; }

		public Vector2 Direction { get; set; }

		public bool HasStartedOverSwipeAreaInUI { get; set; }

		public bool HasStartedOverSlider { get; set; }

#endregion

	}

}