using System.Collections;
using System.Collections.Generic;
using DataskopAR.Data;
using DataskopAR.Entities.Visualizations;
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

		private InfoCardState CurrentCardState { get; set; }

		private InfoCardState PreviousCardState { get; set; }

		private bool IsLocked { get; set; }

#endregion

#region Methods

		public void Init(VisualElement infoCard) {

			InfoCard = infoCard;

			InfoCardStateTransitionClasses = new Dictionary<InfoCardState, string> {
				{ InfoCardState.Collapsed, "info-card-collapsed" },
				{ InfoCardState.Short, "info-card-short" },
				{ InfoCardState.Fullscreen, "info-card-full" }
			};

			CurrentCardState = InfoCardState.Collapsed;
			PreviousCardState = InfoCardState.Collapsed;

			InfoCard.ToggleInClassList(InfoCardStateTransitionClasses[CurrentCardState]);

		}

		public void OnSwipe(Swipe swipe) {

			if (!InfoCard.visible) return;

			if (!swipe.HasStartedOverSwipeAreaInUI)
				return;

			if (IsLocked && !swipe.HasStartedOverSwipeAreaInUI)
				return;

			// Only move info card if user did not swipe over a selected vis
			if (swipe.StartingGameObject != null) {
				Visualization vis = swipe.StartingGameObject.GetComponent<Visualization>();

				if (vis != null)
					if (vis.IsSelected)
						return;
			}

			SetPreviousState();

			if (swipe.Direction.y > 0f)
				CurrentCardState = CurrentCardState switch {
					InfoCardState.Collapsed => InfoCardState.Short,
					InfoCardState.Short => InfoCardState.Fullscreen,
					_ => CurrentCardState
				};

			if (swipe.Direction.y < 0)
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