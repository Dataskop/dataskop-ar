using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DataskopAR.UI {

	public class UiDetector : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

#region Fields

		[SerializeField] private RectTransform infoCardSwipeArea;

#endregion

#region Properties

		public static bool IsOverSwipeHandle { get; private set; }

#endregion

#region Methods

		public void OnPointerEnter(PointerEventData eventData) {
			IsOverSwipeHandle = true;
		}

		public void OnPointerExit(PointerEventData eventData) {
			IsOverSwipeHandle = false;
		}

		public void OnInformationCardStateChanged(InfoCardState newState) {

			Vector3 position = infoCardSwipeArea.position;

			position = newState switch {
				InfoCardState.Collapsed => new Vector3(position.x, 0, position.z),
				InfoCardState.Short => new Vector3(position.x, 670, position.z),
				InfoCardState.Fullscreen => new Vector3(position.x, 2050, position.z),
				_ => throw new ArgumentOutOfRangeException(nameof(newState), newState, null)
			};

			infoCardSwipeArea.position = position;
		}

#endregion

	}

}