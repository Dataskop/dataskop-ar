using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace DataskopAR.UI {

	public class SettingsMenuUI : MonoBehaviour {

#region Constants

		private const string MenuOpenAnimation = "settings-menu-open";
		private const string TogglerAnimation = "toggler-on";
		private const string KnobAnimation = "knob-on";
		private const string DefaultAmount = "10";
		private const string DefaultCooldown = "30";
		private const string ProjectSelectionTitle = "Projects";
		private const string SettingsTitle = "Settings";

#endregion

#region Fields

		[Header("Events")]
		public UnityEvent onToggleBuildingsButtonPressed;
		public UnityEvent onToggleCompassButtonPressed;
		public UnityEvent onToggleOcclusionButtonPressed;
		public UnityEvent onToggleFloorCalibrationButtonPressed;
		public UnityEvent onCompassCalibrationButtonPressed;
		public UnityEvent onResetCalibrationButtonPressed;
		public UnityEvent onLogoutButtonPressed;
		public UnityEvent<bool> onSettingsMenuToggle;
		public UnityEvent<bool> onProjectSelectorToggle;
		public UnityEvent historyButtonPressed;
		public UnityEvent sidePanelOpened;
		public UnityEvent<int> amountInputChanged;
		public UnityEvent<int> cooldownInputChanged;

		[Header("References")]
		[SerializeField] private UIDocument settingsMenuUIDoc;

		[Header("Values")]
		[SerializeField] private Color selectedIconColor;
		[SerializeField] private Color deselectedIconColor;

		private bool isSettingsMenuActive;
		private bool isProjectSelectorActive;
		private bool isHistorySliderActive;

#endregion

#region Properties

		private VisualElement Root { get; set; }
		private VisualElement SettingsMenuContainer { get; set; }
		private VisualElement ProjectSelectorContainer { get; set; }
		private VisualElement AppButtonsContainer { get; set; }
		private Button SettingsMenuButton { get; set; }
		private Button ProjectSelectorButton { get; set; }
		private Button HistoryButton { get; set; }
		private Button ToggleBuildingsButton { get; set; }
		private Button ToggleCompassButton { get; set; }
		private Button ToggleOcclusionButton { get; set; }
		private Button ToggleFloorCalibrationButton { get; set; }
		private Button CompassCalibrationButton { get; set; }
		private Button ResetCalibrationButton { get; set; }
		private Button LogoutButton { get; set; }
		private Label VersionLabel { get; set; }
		private Label TitleLabel { get; set; }

		private TextField AmountInput { get; set; }
		private TextField CooldownInput { get; set; }

#endregion

#region Methods

		private void OnEnable() {
			Root = settingsMenuUIDoc.rootVisualElement;

			SettingsMenuContainer = Root.Q<VisualElement>("SettingsMenuContainer");
			SettingsMenuContainer.RegisterCallback<PointerDownEvent>(_ => { UIInteractionDetection.IsPointerOverUi = true; });

			ProjectSelectorContainer = Root.Q<VisualElement>("ProjectSelectionContainer");
			ProjectSelectorContainer.RegisterCallback<PointerDownEvent>(_ => { UIInteractionDetection.IsPointerOverUi = true; });

			AppButtonsContainer = Root.Q<VisualElement>("AppButtonsContainer");
			AppButtonsContainer.RegisterCallback<PointerEnterEvent>(_ => { UIInteractionDetection.IsPointerOverUi = true; });

#if UNITY_EDITOR

			SettingsMenuContainer.RegisterCallback<PointerEnterEvent>(_ => { UIInteractionDetection.IsPointerOverUi = true; });
			SettingsMenuContainer.RegisterCallback<PointerLeaveEvent>(_ => { UIInteractionDetection.IsPointerOverUi = false; });

			ProjectSelectorContainer.RegisterCallback<PointerEnterEvent>(_ => { UIInteractionDetection.IsPointerOverUi = true; });
			ProjectSelectorContainer.RegisterCallback<PointerLeaveEvent>(_ => { UIInteractionDetection.IsPointerOverUi = false; });

			AppButtonsContainer.RegisterCallback<PointerEnterEvent>(_ => { UIInteractionDetection.IsPointerOverUi = true; });
			AppButtonsContainer.RegisterCallback<PointerLeaveEvent>(_ => { UIInteractionDetection.IsPointerOverUi = false; });

#endif

			SettingsMenuButton = Root.Q<Button>("SettingsMenuButton");
			SettingsMenuButton.RegisterCallback<ClickEvent>(_ => ToggleSettingsMenu());

			ProjectSelectorButton = Root.Q<Button>("ProjectSelectorButton");
			ProjectSelectorButton.RegisterCallback<ClickEvent>(_ => ToggleProjectSelectionMenu());

			HistoryButton = Root.Q<Button>("HistoryButton");
			HistoryButton.RegisterCallback<ClickEvent>(e => ToggleHistoryView());

			ToggleBuildingsButton = SettingsMenuContainer.Q<Button>("Option_01");
			ToggleBuildingsButton.RegisterCallback<ClickEvent>(_ => ToggleBuildings());

			ToggleCompassButton = SettingsMenuContainer.Q<Button>("Option_02");
			ToggleCompassButton.RegisterCallback<ClickEvent>(_ => ToggleCompass());

			ToggleOcclusionButton = SettingsMenuContainer.Q<Button>("Option_03");
			ToggleOcclusionButton.RegisterCallback<ClickEvent>(_ => ToggleOcclusion());

			ToggleFloorCalibrationButton = SettingsMenuContainer.Q<Button>("Option_04");
			ToggleFloorCalibrationButton.RegisterCallback<ClickEvent>(_ => ToggleFloorCalibration());

			CompassCalibrationButton = SettingsMenuContainer.Q<Button>("CalibrateCompassButton");
			CompassCalibrationButton.RegisterCallback<ClickEvent>(_ => CompassCalibrationPressed());

			ResetCalibrationButton = SettingsMenuContainer.Q<Button>("ResetCalibrationButton");
			ResetCalibrationButton.RegisterCallback<ClickEvent>(_ => ResetCalibrationPressed());

			LogoutButton = SettingsMenuContainer.Q<Button>("LogoutButton");
			LogoutButton.RegisterCallback<ClickEvent>(_ => LogoutButtonPressed());

			VersionLabel = SettingsMenuContainer.Q<Label>("Version");
			VersionLabel.text = "DataskopAR - " + Version.ID;

			TitleLabel = Root.Q<Label>("MenuTitle");

			AmountInput = SettingsMenuContainer.Q<TextField>("AmountInput");
			AmountInput.RegisterCallback<ChangeEvent<string>>(AmountInputChanged);

			CooldownInput = SettingsMenuContainer.Q<TextField>("CooldownInput");
			CooldownInput.RegisterCallback<ChangeEvent<string>>(CooldownInputChanged);

		}

		public void ToggleSettingsMenu() {
			isSettingsMenuActive = !isSettingsMenuActive;

			if (isSettingsMenuActive) {

				TitleLabel.text = SettingsTitle;

				if (isProjectSelectorActive) {
					ProjectSelectorContainer.RemoveFromClassList(MenuOpenAnimation);

					ProjectSelectorButton.Q<VisualElement>("Icon").style.unityBackgroundImageTintColor =
						new StyleColor(deselectedIconColor);
					isProjectSelectorActive = false;
				}

				SettingsMenuContainer.AddToClassList(MenuOpenAnimation);
				SettingsMenuButton.Q<VisualElement>("Icon").style.unityBackgroundImageTintColor =
					new StyleColor(selectedIconColor);

				onSettingsMenuToggle?.Invoke(true);
			}
			else {
				SettingsMenuContainer.RemoveFromClassList(MenuOpenAnimation);
				SettingsMenuButton.Q<VisualElement>("Icon").style.unityBackgroundImageTintColor =
					new StyleColor(deselectedIconColor);
			}

			if (isSettingsMenuActive || isProjectSelectorActive) sidePanelOpened?.Invoke();

		}

		public void ToggleProjectSelectionMenu() {
			isProjectSelectorActive = !isProjectSelectorActive;

			if (isProjectSelectorActive) {

				TitleLabel.text = ProjectSelectionTitle;

				if (isSettingsMenuActive) {
					SettingsMenuContainer.RemoveFromClassList(MenuOpenAnimation);
					SettingsMenuButton.Q<VisualElement>("Icon").style.unityBackgroundImageTintColor =
						new StyleColor(deselectedIconColor);
					isSettingsMenuActive = false;
				}

				ProjectSelectorContainer.AddToClassList(MenuOpenAnimation);
				ProjectSelectorButton.Q<VisualElement>("Icon").style.unityBackgroundImageTintColor =
					new StyleColor(selectedIconColor);
				onProjectSelectorToggle?.Invoke(true);
			}
			else {
				ProjectSelectorContainer.RemoveFromClassList(MenuOpenAnimation);

				ProjectSelectorButton.Q<VisualElement>("Icon").style.unityBackgroundImageTintColor =
					new StyleColor(deselectedIconColor);
			}

			if (isSettingsMenuActive || isProjectSelectorActive) sidePanelOpened?.Invoke();
		}

		public void ToggleHistoryView() {

			isHistorySliderActive = !isHistorySliderActive;
			historyButtonPressed?.Invoke();
			HistoryButton.Q<VisualElement>("Icon").style.unityBackgroundImageTintColor =
				new StyleColor(isHistorySliderActive ? selectedIconColor : deselectedIconColor);

		}

		public void OnDataPointsResultsUpdated() {

			if (isHistorySliderActive) {
				ToggleHistoryView();
			}

		}

		public void HideSettings() {
			SettingsMenuContainer.RemoveFromClassList(MenuOpenAnimation);
			SettingsMenuButton.Q<VisualElement>("Icon").style.unityBackgroundImageTintColor =
				new StyleColor(deselectedIconColor);
			ProjectSelectorContainer.RemoveFromClassList(MenuOpenAnimation);

			ProjectSelectorButton.Q<VisualElement>("Icon").style.unityBackgroundImageTintColor =
				new StyleColor(deselectedIconColor);
			isSettingsMenuActive = false;
			isProjectSelectorActive = false;
		}

		private void ToggleBuildings() {
			Toggle(ToggleBuildingsButton);
			onToggleBuildingsButtonPressed?.Invoke();
		}

		private void ToggleCompass() {
			Toggle(ToggleCompassButton);
			onToggleCompassButtonPressed?.Invoke();
		}

		private void ToggleOcclusion() {
			Toggle(ToggleOcclusionButton);
			onToggleOcclusionButtonPressed?.Invoke();
		}

		private void ToggleFloorCalibration() {
			Toggle(ToggleFloorCalibrationButton);
			onToggleFloorCalibrationButtonPressed?.Invoke();
		}

		private static void Toggle(VisualElement pressedButton) {
			pressedButton.Q<VisualElement>(null, "knob-off").ToggleInClassList(KnobAnimation);
			pressedButton.Q<VisualElement>(null, "toggler-off").ToggleInClassList(TogglerAnimation);
		}

		private void CompassCalibrationPressed() {
			onCompassCalibrationButtonPressed?.Invoke();
		}

		private void ResetCalibrationPressed() {
			onResetCalibrationButtonPressed?.Invoke();
		}

		private void LogoutButtonPressed() {
			onLogoutButtonPressed?.Invoke();
			AccountManager.Logout();
		}

		private void AmountInputChanged(ChangeEvent<string> e) {

			if (string.IsNullOrEmpty(e.newValue)) {
				return;
			}

			if (int.TryParse(e.newValue, out int value)) {
				amountInputChanged?.Invoke(value);
			}
			else {
				AmountInput.value = DefaultAmount;
			}

		}

		private void CooldownInputChanged(ChangeEvent<string> e) {

			if (string.IsNullOrEmpty(e.newValue)) {
				return;
			}

			if (int.TryParse(e.newValue, out int value)) {
				cooldownInputChanged?.Invoke(value);
			}
			else {
				CooldownInput.value = DefaultCooldown;
			}

		}

		public void OnInfoCardFullscreen(InfoCardState state) {

			if (state != InfoCardState.Fullscreen) return;

			HideSettings();

		}

		private void OnDisable() {
			SettingsMenuButton.UnregisterCallback<ClickEvent>(_ => ToggleSettingsMenu());
			ProjectSelectorButton.UnregisterCallback<ClickEvent>(_ => ToggleProjectSelectionMenu());
			CompassCalibrationButton.UnregisterCallback<ClickEvent>(_ => CompassCalibrationPressed());
			ResetCalibrationButton.UnregisterCallback<ClickEvent>(_ => ResetCalibrationPressed());
		}

#endregion

	}

}