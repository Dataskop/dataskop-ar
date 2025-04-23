using System;
using Dataskop.Data;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace Dataskop.UI {

	public class SettingsMenuUI : MonoBehaviour {

		private const string MenuOpenAnimation = "settings-menu-open";
		private const string TogglerAnimation = "toggler-on";
		private const string KnobAnimation = "knob-on";
		private const string DefaultAmount = "2000";
		private const string DefaultCooldown = "10";
		private const string ProjectSelectionTitle = "Projects";
		private const string SettingsTitle = "Settings";

		[Header("Events")]
		public UnityEvent onToggleOcclusionButtonPressed;
		public UnityEvent onToggleMinimapButtonPressed;
		public UnityEvent onResetCalibrationButtonPressed;
		public UnityEvent onLogoutButtonPressed;
		public UnityEvent<bool> historyButtonPressed;
		public UnityEvent sidePanelOpened;
		public UnityEvent<int> amountInputChanged;
		public UnityEvent<int> cooldownInputChanged;

		[Header("References")]
		[SerializeField] private UIDocument menuDocument;

		[Header("Values")]
		[SerializeField] private Color selectedIconColor;
		[SerializeField] private Color deselectedIconColor;
		private bool isHistorySliderActive;
		private bool isProjectSelectorActive;
		private bool isSettingsMenuActive;

		private MenuView CurrentView { get; set; } = MenuView.Settings;

		private bool IsOpen { get; set; }

		private VisualElement Root { get; set; }

		private VisualElement MenuContainer { get; set; }

		private VisualElement SettingsMenuContainer { get; set; }

		private VisualElement ProjectSelectorContainer { get; set; }

		private Button SettingsMenuButton { get; set; }

		private Button ProjectSelectorButton { get; set; }

		private Button HistoryButton { get; set; }

		private Button ToggleOcclusionButton { get; set; }

		private Button ToggleMinimapButton { get; set; }

		private Button ToggleDateFilterButton { get; set; }

		private Button ResetCalibrationButton { get; set; }

		private Button LogoutButton { get; set; }

		private Button SortButton { get; set; }

		private VisualElement ProjectsIcon { get; set; }

		private VisualElement SettingsIcon { get; set; }

		private VisualElement HistoryIcon { get; set; }

		private Label VersionLabel { get; set; }

		private Label TitleLabel { get; set; }

		private TextField AmountInput { get; set; }

		private TextField CooldownInput { get; set; }

		private void Awake() {
			Root = menuDocument.rootVisualElement;

			MenuContainer = Root.Q<VisualElement>("MenuContainer");

			SettingsMenuContainer = Root.Q<VisualElement>("SettingsMenuContainer");

			ProjectSelectorContainer = Root.Q<VisualElement>("ProjectSelectionContainer");

			SettingsMenuButton = Root.Q<Button>("SettingsMenuButton");
			SettingsMenuButton.RegisterCallback<ClickEvent>(_ => ToggleMenu(MenuView.Settings));

			SettingsIcon = SettingsMenuButton.Q<VisualElement>("Icon");

			ProjectSelectorButton = Root.Q<Button>("ProjectSelectorButton");
			ProjectSelectorButton.RegisterCallback<ClickEvent>(_ => ToggleMenu(MenuView.Projects));

			ProjectsIcon = ProjectSelectorButton.Q<VisualElement>("Icon");

			HistoryButton = Root.Q<Button>("HistoryButton");
			HistoryButton.RegisterCallback<ClickEvent>(_ => ToggleHistoryView());

			HistoryIcon = HistoryButton.Q<VisualElement>("Icon");

			ToggleOcclusionButton = SettingsMenuContainer.Q<Button>("Option_Occlusion");
			ToggleOcclusionButton.RegisterCallback<ClickEvent>(_ => ToggleOcclusion());

			ToggleMinimapButton = SettingsMenuContainer.Q<Button>("Option_Minimap");
			ToggleMinimapButton.RegisterCallback<ClickEvent>(_ => ToggleMinimap());

			ToggleDateFilterButton = SettingsMenuContainer.Q<Button>("Option_DateFilter");

			ResetCalibrationButton = SettingsMenuContainer.Q<Button>("ResetCalibrationButton");
			ResetCalibrationButton.RegisterCallback<ClickEvent>(_ => ResetCalibrationPressed());

			LogoutButton = SettingsMenuContainer.Q<Button>("LogoutButton");
			LogoutButton.RegisterCallback<ClickEvent>(_ => LogoutButtonPressed());

			SortButton = MenuContainer.Q<Button>("SortButton");

			VersionLabel = SettingsMenuContainer.Q<Label>("Version");
			VersionLabel.text = "DataskopAR - " + Version.ID;

			TitleLabel = Root.Q<Label>("MenuTitle");

			AmountInput = SettingsMenuContainer.Q<TextField>("AmountInput");
			AmountInput.RegisterCallback<ChangeEvent<string>>(OnFetchAmountInputChanged);
			AmountInput.SetValueWithoutNotify(
				PlayerPrefs.HasKey("fetchAmount") ? PlayerPrefs.GetInt("fetchAmount").ToString()
					: DefaultAmount
			);

			CooldownInput = SettingsMenuContainer.Q<TextField>("CooldownInput");
			CooldownInput.RegisterCallback<ChangeEvent<string>>(OnFetchIntervalInputChanged);
			CooldownInput.SetValueWithoutNotify(
				PlayerPrefs.HasKey("fetchInterval")
					? (PlayerPrefs.GetInt("fetchInterval") / 1000).ToString() : DefaultCooldown
			);
		}

		private void OnDisable() {
			SettingsMenuButton.UnregisterCallback<ClickEvent>(_ => ToggleMenu(MenuView.Settings));
			ProjectSelectorButton.UnregisterCallback<ClickEvent>(_ => ToggleMenu(MenuView.Projects));
			ResetCalibrationButton.UnregisterCallback<ClickEvent>(_ => ResetCalibrationPressed());
		}

		private void ToggleMenu(MenuView requestedView) {

			if (IsOpen) {

				if (CurrentView == requestedView) {

					MenuContainer.RemoveFromClassList(MenuOpenAnimation);
					SettingsIcon.style.unityBackgroundImageTintColor = new StyleColor(deselectedIconColor);
					ProjectsIcon.style.unityBackgroundImageTintColor = new StyleColor(deselectedIconColor);

					SettingsMenuButton.style.borderBottomColor = deselectedIconColor;
					SettingsMenuButton.style.borderLeftColor = deselectedIconColor;
					SettingsMenuButton.style.borderRightColor = deselectedIconColor;
					SettingsMenuButton.style.borderTopColor = deselectedIconColor;

					ProjectSelectorButton.style.borderBottomColor = deselectedIconColor;
					ProjectSelectorButton.style.borderLeftColor = deselectedIconColor;
					ProjectSelectorButton.style.borderRightColor = deselectedIconColor;
					ProjectSelectorButton.style.borderTopColor = deselectedIconColor;

					IsOpen = false;
					return;
				}

				DisplayView(requestedView);

			}
			else {
				MenuContainer.AddToClassList(MenuOpenAnimation);
				IsOpen = true;
				DisplayView(requestedView);
				sidePanelOpened?.Invoke();
			}

		}

		private void DisplayView(MenuView view) {

			switch (view) {
				case MenuView.Projects:
					SettingsIcon.style.unityBackgroundImageTintColor = new StyleColor(deselectedIconColor);
					SettingsMenuButton.style.borderBottomColor = deselectedIconColor;
					SettingsMenuButton.style.borderLeftColor = deselectedIconColor;
					SettingsMenuButton.style.borderRightColor = deselectedIconColor;
					SettingsMenuButton.style.borderTopColor = deselectedIconColor;

					SettingsMenuContainer.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);

					TitleLabel.text = ProjectSelectionTitle;
					ProjectsIcon.style.unityBackgroundImageTintColor = new StyleColor(selectedIconColor);
					ProjectSelectorContainer.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
					ProjectSelectorButton.style.borderBottomColor = selectedIconColor;
					ProjectSelectorButton.style.borderLeftColor = selectedIconColor;
					ProjectSelectorButton.style.borderRightColor = selectedIconColor;
					ProjectSelectorButton.style.borderTopColor = selectedIconColor;

					SortButton.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);

					CurrentView = MenuView.Projects;

					break;
				case MenuView.Settings:
					ProjectsIcon.style.unityBackgroundImageTintColor = new StyleColor(deselectedIconColor);
					ProjectSelectorContainer.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
					ProjectSelectorButton.style.borderBottomColor = deselectedIconColor;
					ProjectSelectorButton.style.borderLeftColor = deselectedIconColor;
					ProjectSelectorButton.style.borderRightColor = deselectedIconColor;
					ProjectSelectorButton.style.borderTopColor = deselectedIconColor;

					TitleLabel.text = SettingsTitle;
					SettingsIcon.style.unityBackgroundImageTintColor = new StyleColor(selectedIconColor);
					SettingsMenuContainer.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);

					SettingsMenuButton.style.borderBottomColor = selectedIconColor;
					SettingsMenuButton.style.borderLeftColor = selectedIconColor;
					SettingsMenuButton.style.borderRightColor = selectedIconColor;
					SettingsMenuButton.style.borderTopColor = selectedIconColor;

					SortButton.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);

					CurrentView = MenuView.Settings;
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(view), view, null);
			}

		}

		public void OnCalibrationFinished() {
			ProjectSelectorButton.visible = true;
			ToggleMenu(MenuView.Projects);
		}

		private void ToggleHistoryView() {

			isHistorySliderActive = !isHistorySliderActive;
			historyButtonPressed?.Invoke(isHistorySliderActive);
			HistoryIcon.style.unityBackgroundImageTintColor =
				new StyleColor(isHistorySliderActive ? selectedIconColor : deselectedIconColor);

			HistoryButton.style.borderBottomColor = isHistorySliderActive ? selectedIconColor : deselectedIconColor;
			HistoryButton.style.borderLeftColor = isHistorySliderActive ? selectedIconColor : deselectedIconColor;
			HistoryButton.style.borderRightColor = isHistorySliderActive ? selectedIconColor : deselectedIconColor;
			HistoryButton.style.borderTopColor = isHistorySliderActive ? selectedIconColor : deselectedIconColor;

		}

		public void OnDateFiltered() {
			isHistorySliderActive = true;
			HistoryIcon.style.unityBackgroundImageTintColor = new StyleColor(selectedIconColor);
			HistoryButton.style.borderBottomColor = selectedIconColor;
			HistoryButton.style.borderLeftColor = selectedIconColor;
			HistoryButton.style.borderRightColor = selectedIconColor;
			HistoryButton.style.borderTopColor = selectedIconColor;
		}

		public void HideSettings() {

			if (IsOpen) {
				ToggleMenu(CurrentView);
			}

		}

		private void ToggleOcclusion() {
			Toggle(ToggleOcclusionButton);
			onToggleOcclusionButtonPressed?.Invoke();
		}

		private void ToggleMinimap() {
			Toggle(ToggleMinimapButton);
			onToggleMinimapButtonPressed?.Invoke();
		}

		private static void Toggle(VisualElement pressedButton) {
			pressedButton.Q<VisualElement>(null, "knob-off").ToggleInClassList(KnobAnimation);
			pressedButton.Q<VisualElement>(null, "toggler-off").ToggleInClassList(TogglerAnimation);
		}

		private void ResetCalibrationPressed() {
			onResetCalibrationButtonPressed?.Invoke();
		}

		private void LogoutButtonPressed() {
			onLogoutButtonPressed?.Invoke();
			AccountManager.Logout();
		}

		private void OnFetchAmountInputChanged(ChangeEvent<string> e) {

			if (string.IsNullOrEmpty(e.newValue)) {
				return;
			}

			if (int.TryParse(e.newValue, out int value)) {
				int validValue = Mathf.Clamp(value, 1, 2000);
				amountInputChanged?.Invoke(validValue);
			}
			else {
				AmountInput.value = DefaultAmount;
			}

		}

		private void OnFetchIntervalInputChanged(ChangeEvent<string> e) {

			if (string.IsNullOrEmpty(e.newValue)) {
				return;
			}

			if (int.TryParse(e.newValue, out int value)) {
				int milliseconds = value * 1000;
				int validValue = Mathf.Clamp(milliseconds, 2000, 900000);
				cooldownInputChanged?.Invoke(validValue);
			}
			else {
				CooldownInput.value = DefaultCooldown;
			}

		}

		public void OnInfoCardStateChanged(InfoCardState state) {

			if (state == InfoCardState.Fullscreen) {
				HideSettings();
			}

		}

		public void OnProjectLoaded() {
			isHistorySliderActive = false;
			HistoryIcon.style.unityBackgroundImageTintColor = new StyleColor(deselectedIconColor);
			HistoryButton.style.borderBottomColor = deselectedIconColor;
			HistoryButton.style.borderLeftColor = deselectedIconColor;
			HistoryButton.style.borderRightColor = deselectedIconColor;
			HistoryButton.style.borderTopColor = deselectedIconColor;
			HistoryButton.visible = true;
		}

	}

	public enum MenuView {

		Settings,
		Projects

	}

}