using System;
using System.Collections.Generic;
using DataskopAR.Data;
using DataskopAR.Interaction;
using UnityEngine;
using UnityEngine.UIElements;

namespace DataskopAR.UI {

	public class InfoCardManager : MonoBehaviour {

#region Fields

		[Header("References")]
		[SerializeField] private UIDocument informationCardUIDoc;
		[SerializeField] private InfoCardStateManager infoCardStateManager;
		[SerializeField] private InfoCardNotificationUI infoCardNotificationUI;
		[SerializeField] private InfoCardHeaderUI infoCardHeaderUI;
		[SerializeField] private InfoCardProjectDataUI infoCardProjectDataUI;
		[SerializeField] private InfoCardDataUI infoCardDataUI;
		[SerializeField] private InfoCardMap infoCardMap;
		[SerializeField] private DataManager dataManager;

		private Coroutine uiInteractionRoutine;

#endregion

#region Properties

		private VisualElement InfoCard { get; set; }

		private VisualElement DetailsContainer { get; set; }

		private VisualElement MapContainer { get; set; }

		private VisualElement DetailsTab { get; set; }

		private VisualElement MapTab { get; set; }

		private Label CallToAction { get; set; }

#endregion

#region Methods

		private void OnEnable() {

			InfoCard = informationCardUIDoc.rootVisualElement.Q<VisualElement>("InfoCard");

			InfoCard.RegisterCallback<PointerDownEvent>(e => {
					UIInteractionDetection.IsPointerOverUi = true;
					UIInteractionDetection.HasPointerStartedOverSwipeArea = true;
				}
			);

#if UNITY_EDITOR

			InfoCard.RegisterCallback<PointerEnterEvent>(e => {
				UIInteractionDetection.IsPointerOverUi = true;
				UIInteractionDetection.HasPointerStartedOverSwipeArea = true;
			});
			InfoCard.RegisterCallback<PointerLeaveEvent>(e => {
				UIInteractionDetection.IsPointerOverUi = false;
				UIInteractionDetection.HasPointerStartedOverSwipeArea = false;
			});

#endif

			DetailsContainer = InfoCard.Q<VisualElement>("DetailsContainer");
			MapContainer = InfoCard.Q<VisualElement>("MapContainer");

			DetailsTab = InfoCard.Q<VisualElement>("DetailsTab");
			DetailsTab.RegisterCallback<ClickEvent>(_ => OnDetailsTabPressed());

			MapTab = InfoCard.Q<VisualElement>("MapTab");
			MapTab.RegisterCallback<ClickEvent>(_ => OnMapTabPressed());

			CallToAction = InfoCard.Q<Label>("CallToAction");
			CallToAction.visible = false;

			infoCardStateManager.Init(InfoCard);

			dataManager.HasLoadedProjectData += OnProjectDataUpdated;
			dataManager.HasUpdatedMeasurementResults += OnMeasurementResultsUpdated;
			ErrorHandler.OnErrorReceived += OnErrorReceived;
			SwipeDetector.OnSwipe += OnSwiped;

			infoCardNotificationUI.Init(InfoCard);
			infoCardProjectDataUI.Init(InfoCard);
			infoCardHeaderUI.Init(InfoCard);
			infoCardDataUI.Init(InfoCard);
			infoCardMap.Init(InfoCard);

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
			SetCallToActionState(false);

		}

		public void OnDataPointSoftSelected(DataPoint dp) {

			infoCardDataUI.UpdateDataPointData(dp);

			if (infoCardStateManager.CurrentCardState != InfoCardState.Collapsed) {
				return;
			}

			SetCallToActionState(dp != null);

		}

		public void OnInfoCardStateChanged(InfoCardState newState) {

			if (newState != InfoCardState.Collapsed) {
				SetCallToActionState(false);
			}

		}

		private void SetCallToActionState(bool newState) {
			CallToAction.visible = newState;
		}

		private void OnErrorReceived(object o, ErrorHandler.ErrorReceivedEventArgs e) {
			infoCardNotificationUI.OnErrorReceived(e.Error);
		}

		private void OnSwiped(Swipe swipe) {
			infoCardStateManager.OnSwipe(swipe);
		}

		public void OnMinimapTapped() {
			OnMapTabPressed();
			infoCardStateManager.ShowInfo();
		}

		public void SetInfoCardVisibility(bool isVisible) {
			InfoCard.style.visibility = new StyleEnum<Visibility>(isVisible ? Visibility.Visible : Visibility.Hidden);
		}

		private void OnDetailsTabPressed() {
			MapContainer.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
			DetailsContainer.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
			HighlightTab(DetailsTab);
		}

		private void OnMapTabPressed() {
			DetailsContainer.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
			MapContainer.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
			HighlightTab(MapTab);
		}

		private void HighlightTab(VisualElement pressedTab) {

			IEnumerable<VisualElement> availableTabs = InfoCard.Q<VisualElement>("TabContainer").Children();

			foreach (VisualElement element in availableTabs) {

				if (element == pressedTab) {
					pressedTab.RemoveFromClassList("tab");
					pressedTab.AddToClassList("tab-selected");
					continue;
				}

				if (!element.ClassListContains("tab-selected")) {
					continue;
				}

				element.RemoveFromClassList("tab-selected");
				element.AddToClassList("tab");

			}

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