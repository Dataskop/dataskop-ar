using System.Collections;
using System.Collections.Generic;
using DataskopAR.Data;
using DataskopAR.Interaction;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace DataskopAR.UI {

	public class InfoCardStateManager : MonoBehaviour {

#region Events

		[Header("Events")]
		public UnityEvent<InfoCardState> infoCardStateChanged;

#endregion

#region Fields

		[SerializeField] private float stickyTime;
		private Coroutine stickyCoroutine;

#endregion

#region Properties

		private Dictionary<InfoCardState, string> InfoCardStateTransitionClasses { get; set; }

		private VisualElement InfoCard { get; set; }

		public InfoCardState CurrentCardState { get; private set; }

		private InfoCardState PreviousCardState { get; set; }

		private bool IsLocked { get; set; }

#endregion

#region Methods

		public void Init(VisualElement infoCard) {

			InfoCard = infoCard;

			InfoCardStateTransitionClasses = new Dictionary<InfoCardState, string> {
				{
					InfoCardState.Collapsed, "info-card-collapsed"
				}, {
					InfoCardState.Short, "info-card-short"
				}, {
					InfoCardState.Fullscreen, "info-card-full"
				}
			};

			CurrentCardState = InfoCardState.Collapsed;
			PreviousCardState = InfoCardState.Collapsed;

			InfoCard.ToggleInClassList(InfoCardStateTransitionClasses[CurrentCardState]);

		}

		public void OnSwipe(PointerInteraction pointerInteraction) {

			if (!InfoCard.visible) return;

			SetPreviousState();

			if (pointerInteraction.Direction.y > 0.20f)
				CurrentCardState = CurrentCardState switch {
					InfoCardState.Collapsed => InfoCardState.Short,
					InfoCardState.Short => InfoCardState.Fullscreen,
					_ => CurrentCardState
				};

			if (pointerInteraction.Direction.y < -0.20f)
				CurrentCardState = CurrentCardState switch {
					InfoCardState.Fullscreen => InfoCardState.Short,
					InfoCardState.Short => InfoCardState.Collapsed,
					_ => CurrentCardState
				};

			UpdateInformationCardState(CurrentCardState);
		}

		private void UpdateInformationCardState(InfoCardState infoCardState) {

			if (infoCardState == PreviousCardState) return;

			InfoCard.ToggleInClassList(InfoCardStateTransitionClasses[PreviousCardState]);
			InfoCard.ToggleInClassList(InfoCardStateTransitionClasses[infoCardState]);

			infoCardStateChanged?.Invoke(CurrentCardState);

		}

		public void OnDataPointSelected(DataPoint selectedDataPoint) {

			if (selectedDataPoint != null) {
				ShowInfo();
				ChangeLockedState(true);
			}
			else {
				ChangeLockedState(false);
				HideInfo();
			}

		}

		public void OnDataPointSoftSelected(DataPoint softSelectedDataPoint) {

			if (IsLocked)
				return;

			if (softSelectedDataPoint != null) {

				if (stickyCoroutine != null) {
					StopCoroutine(stickyCoroutine);
					stickyCoroutine = null;
				}

				ShowInfo();
			}
			else {
				stickyCoroutine ??= StartCoroutine(StickyState(stickyTime));
			}
		}

		public void ChangeLockedState(bool isLocked) {
			IsLocked = isLocked;
		}

		internal void ShowInfo() {
			SetPreviousState();
			CurrentCardState = InfoCardState.Short;
			UpdateInformationCardState(InfoCardState.Short);
		}

		internal void HideInfo() {
			SetPreviousState();
			CurrentCardState = InfoCardState.Collapsed;
			UpdateInformationCardState(InfoCardState.Collapsed);
		}

		private void SetPreviousState() {
			PreviousCardState = CurrentCardState;
		}

		private IEnumerator StickyState(float time) {
			yield return new WaitForSeconds(time);
			HideInfo();
			stickyCoroutine = null;
		}

#endregion

	}

}