#nullable enable

using Dataskop.UI;
using UnityEngine;

namespace Dataskop.Interaction {

	public struct PointerInteraction {

 

		public int pointerId;

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

  

 

		public float Distance => Mathf.Abs(Vector2.Distance(startPosition, endPosition));

		public Vector2 Direction => (endPosition - startPosition).normalized;

		public float XDistance => Mathf.Abs(endPosition.x - startPosition.x);

		public float YDistance => Mathf.Abs(endPosition.y - startPosition.y);

		public float DeltaTime => endTime - startTime;

  

 

		public new string ToString() {
			return
				$"StartPoint: {startPosition}\nEndPoint: {endPosition}\nDistance: {Distance}\nStart on: {startingGameObject}\nEnd on: {endingGameObject} \nDirection: {Direction}\nDownPhase: {isDownPhase}\nUpPhase: {isUpPhase}\nSwipe: {isSwipe}\nUI: {isUI}";
		}

  

	}

}