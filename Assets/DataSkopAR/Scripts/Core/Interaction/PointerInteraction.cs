#nullable enable

using DataskopAR.UI;
using UnityEngine;

namespace DataskopAR.Interaction {

	public struct PointerInteraction {

#region Fields

		public Vector2 startPosition;

		public Vector2 endPosition;

		public GameObject? startingGameObject;

		public GameObject? endingGameObject;

		public bool isDownPhase;

		public bool isUpPhase;

		public bool isSwipe;

		public bool isUI;

		public UISection uiStartSection;

		public UISection uiEndSection;

		public float startTime;

		public float endTime;

#endregion

#region Properties

		public float Distance => Mathf.Abs(Vector2.Distance(startPosition, endPosition));

		public Vector2 Direction => (startPosition - endPosition).normalized;

		public float XDistance => Mathf.Abs(endPosition.x - startPosition.x);

		public float YDistance => Mathf.Abs(endPosition.y - startPosition.y);

		public float DeltaTime => endTime - startTime;

#endregion

#region Methods

		public new string ToString() {
			return
				$"StartPoint: {startPosition}\nEndPoint: {endPosition}\nDistance: {Distance}\nStart on: {startingGameObject}\nEnd on: {endingGameObject} \nDirection: {Direction}\nDownPhase: {isDownPhase}\nUpPhase: {isUpPhase}\nSwipe: {isSwipe}\nUI: {isUI}";
		}

#endregion

	}

}