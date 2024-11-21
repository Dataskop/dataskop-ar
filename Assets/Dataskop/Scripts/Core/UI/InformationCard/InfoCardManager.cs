using System;
using System.Collections.Generic;
using Dataskop.Data;
using Dataskop.Entities;
using Dataskop.Interaction;
using Dataskop.Utils;
using UnityEngine;
using UnityEngine.UIElements;

namespace Dataskop.UI {

	public class InfoCardManager : MonoBehaviour {

		[Header("References")]
		[SerializeField] private UIDocument informationCardUIDoc;
		[SerializeField] private InfoCardStateManager infoCardStateManager;
		[SerializeField] private InfoCardNotificationUI infoCardNotificationUI;
		[SerializeField] private InfoCardLocatorUI infoCardLocatorUI;
		[SerializeField] private InfoCardProjectDataUI infoCardProjectDataUI;
		[SerializeField] private InfoCardDataUI infoCardDataUI;
		[SerializeField] private InfoCardRefetchProgress infoCardRefetchProgress;
		[SerializeField] private InfoCardProjectSummary infoCardProjectSummary;
		[SerializeField] private InfoCardMap infoCardMap;
		[SerializeField] private DataManager dataManager;
		[SerializeField] private InputHandler inputHandler;

		private Coroutine uiInteractionRoutine;

		private VisualElement InfoCard { get; set; }

		private VisualElement DetailsContainer { get; set; }

		private VisualElement MapContainer { get; set; }

		private VisualElement DetailsTab { get; set; }

		private VisualElement MapTab { get; set; }

		private Label CallToAction { get; set; }

		private void Awake() {

			InfoCard = informationCardUIDoc.rootVisualElement.Q<VisualElement>("InfoCard");

			DetailsContainer = InfoCard.Q<VisualElement>("DetailsContainer");
			MapContainer = InfoCard.Q<VisualElement>("MapContainer");

			DetailsTab = InfoCard.Q<VisualElement>("DetailsTab");
			DetailsTab.RegisterCallback<ClickEvent>(_ => OnDetailsTabPressed());

			MapTab = InfoCard.Q<VisualElement>("MapTab");
			MapTab.RegisterCallback<ClickEvent>(_ => OnMapTabPressed());

			CallToAction = InfoCard.Q<Label>("CallToAction");
			CallToAction.visible = false;

			infoCardStateManager.Init(InfoCard);
			inputHandler.InfoCardPointerUpped += OnPointerInteraction;

			dataManager.HasLoadedProjectData += OnProjectDataUpdated;
			dataManager.HasUpdatedMeasurementResults += OnMeasurementResultsUpdated;
			ErrorHandler.OnErrorReceived += OnErrorReceived;

			infoCardNotificationUI.Init(InfoCard);
			infoCardProjectDataUI.Init(InfoCard);
			infoCardLocatorUI.Init(InfoCard);
			infoCardDataUI.Init(InfoCard);
			infoCardMap.Init(InfoCard);
			infoCardRefetchProgress.Init(InfoCard);
			infoCardProjectSummary.Init(InfoCard);

			dataManager.OnRefetchTimerProgressed += OnRefetchTimerProgressed;

		}

		public void OnProjectLoaded(Project _) {
			infoCardProjectDataUI.UpdateVisibility();
		}

		public void OnUserAreaLocated(LocationArea locationArea) {
			infoCardLocatorUI.OnUserAreaLocated(locationArea);
		}

		public void OnProjectDataUpdated(Project selectedProject) {
			infoCardProjectDataUI.UpdateProjectNameDisplay(
				selectedProject == null ? "N/A" : selectedProject.Information.Name
			);

			infoCardProjectDataUI.UpdateLastUpdatedDisplay(selectedProject?.GetLastUpdatedTime() ?? new DateTime());
		}

		public void OnMeasurementResultsUpdated() {

			infoCardProjectDataUI.UpdateLastUpdatedDisplay(
				dataManager.SelectedProject?.GetLastUpdatedTime() ?? new DateTime()
			);

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

		public void OnRefetchTimerProgressed(int refetchTimer, int currentProgress) {
			infoCardRefetchProgress.OnNewValueReceived(MathExtensions.Map01(currentProgress, 0, refetchTimer));
		}

		private void SetCallToActionState(bool newState) {
			CallToAction.visible = newState;
		}

		private void OnErrorReceived(object o, ErrorHandler.ErrorReceivedEventArgs e) {
			infoCardNotificationUI.OnErrorReceived(e.Error);
		}

		private void OnPointerInteraction(PointerInteraction pointerInteraction) {

			if (pointerInteraction.isSwipe) {
				infoCardStateManager.OnSwipe(pointerInteraction);
			}

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

	}

}
