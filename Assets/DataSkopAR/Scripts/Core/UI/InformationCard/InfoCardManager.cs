using System;
using DataSkopAR.Data;
using DataSkopAR.Interaction;
using UnityEngine;
using UnityEngine.UIElements;

namespace DataSkopAR.UI {

	public class InfoCardManager : MonoBehaviour {

#region Properties

		private VisualElement InfoCard { get; set; }
		private VisualElement SwipeArea { get; set; }

#endregion

#region Fields

		[Header("References")]
		[SerializeField] private UIDocument informationCardUIDoc;
		[SerializeField] private InfoCardStateManager infoCardStateManager;
		[SerializeField] private InfoCardNotificationUI infoCardNotificationUI;
		[SerializeField] private InfoCardHeaderUI infoCardHeaderUI;
		[SerializeField] private InfoCardProjectDataUI infoCardProjectDataUI;
		[SerializeField] private InfoCardDataUI infoCardDataUI;
		[SerializeField] private DataManager dataManager;

		private Coroutine uiInteractionRoutine;

#endregion

#region Methods

		private void OnEnable() {

			InfoCard = informationCardUIDoc.rootVisualElement.Q<VisualElement>("InfoCard");

			InfoCard.RegisterCallback<PointerDownEvent>((e) => { UIInteractionDetection.IsPointerOverUi = true; });

			SwipeArea = InfoCard.Q<VisualElement>("SwipeArea");

			SwipeArea.RegisterCallback<PointerDownEvent>((e) => { UIInteractionDetection.HasPointerStartedOverSwipeArea = true; });

#if UNITY_EDITOR

			SwipeArea.RegisterCallback<PointerEnterEvent>((e) => { UIInteractionDetection.HasPointerStartedOverSwipeArea = true; });
			InfoCard.RegisterCallback<PointerEnterEvent>((e) => { UIInteractionDetection.IsPointerOverUi = true; });
			SwipeArea.RegisterCallback<PointerLeaveEvent>((e) => { UIInteractionDetection.HasPointerStartedOverSwipeArea = false; });
			InfoCard.RegisterCallback<PointerLeaveEvent>((e) => { UIInteractionDetection.IsPointerOverUi = false; });

#endif

			infoCardStateManager.Init(InfoCard);

			dataManager.HasLoadedProjectData += OnProjectDataUpdated;
			dataManager.HasUpdatedMeasurementResults += OnMeasurementResultsUpdated;
			ErrorHandler.OnErrorReceived += OnErrorReceived;
			SwipeDetector.OnSwipe += OnSwiped;

			infoCardNotificationUI.Init(InfoCard);
			infoCardProjectDataUI.Init(InfoCard);
			infoCardHeaderUI.Init(InfoCard);
			infoCardDataUI.Init(InfoCard);

		}

		public void OnUserAreaLocated(LocationArea locationArea) {
			infoCardHeaderUI.OnUserAreaLocated(locationArea);
		}

		public void OnProjectDataUpdated(Project selectedProject) {

			infoCardProjectDataUI.UpdateProjectNameDisplay(selectedProject == null ? "N/A" : selectedProject.Information.Name);
			infoCardProjectDataUI.UpdateLastUpdatedDisplay(selectedProject?.GetLastUpdatedTime() ?? new DateTime());

		}

		public void OnMeasurementResultsUpdated() {
			infoCardProjectDataUI.UpdateLastUpdatedDisplay(dataManager.SelectedProject?.GetLastUpdatedTime() ?? new DateTime());
		}

		public void OnSidePanelOpened() {
			infoCardStateManager.HideInfo();
		}

		public void OnDataPointSelected(DataPoint dp) {
			infoCardStateManager.OnDataPointSelected(dp);
			infoCardDataUI.UpdateDataPointData(dp);
		}

		public void OnDataPointSoftSelected(DataPoint dp) {
			infoCardStateManager.OnDataPointSoftSelected(dp);
			infoCardDataUI.UpdateDataPointData(dp);
		}

		private void OnErrorReceived(object o, ErrorHandler.ErrorReceivedEventArgs e) {
			infoCardNotificationUI.OnErrorReceived(e.Error);
		}

		private void OnSwiped(Swipe swipe) {
			infoCardStateManager.OnSwipe(swipe);
		}

		public void SetInfoCardVisibility(bool isVisible) {
			InfoCard.style.visibility = new StyleEnum<Visibility>(isVisible ? Visibility.Visible : Visibility.Hidden);
		}

		private void OnDisable() {
			SwipeDetector.OnSwipe -= OnSwiped;
			ErrorHandler.OnErrorReceived -= OnErrorReceived;
			dataManager.HasLoadedProjectData -= OnProjectDataUpdated;
			dataManager.HasUpdatedMeasurementResults -= OnMeasurementResultsUpdated;
		}

#endregion

	}

}