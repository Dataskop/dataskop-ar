using System;
using System.Runtime.InteropServices.ComTypes;
using UnityEngine;
using UnityEngine.UIElements;

namespace Dataskop.UI {

	public class CachedRangesUI : MonoBehaviour {

		[Header("References")]
		[SerializeField] private UIDocument historyMenuDoc;

		[Header("Icons")]
		[SerializeField] private Sprite hourIcon;
		[SerializeField] private Sprite daysIcon;

		private VisualElement cachedRangeContainer;
		private VisualElement topDragger;
		private VisualElement bottomDragger;
		private VisualElement rangeContainer;
		private VisualElement cachedRangesDisplay;

		private VisualElement SwitchUnitsIcon { get; set; }

		private Button SwitchUnitsButton { get; set; }

		private Label UltimateEndTime { get; set; }

		private Label UltimateStartTime { get; set; }

		private Label EndRangeLabel { get; set; }

		private Label StartRangeLabel { get; set; }

		private MinMaxSlider MinMaxSlider { get; set; }

		private void OnEnable() {

			cachedRangeContainer = historyMenuDoc.rootVisualElement.Q<VisualElement>("RangeContainer");

			UltimateEndTime = cachedRangeContainer.Q<Label>("LabelMinDate");
			UltimateStartTime = cachedRangeContainer.Q<Label>("LabelMaxDate");

			EndRangeLabel = cachedRangeContainer.Q<Label>("LabelMinValue");
			StartRangeLabel = cachedRangeContainer.Q<Label>("LabelMaxValue");

			MinMaxSlider = cachedRangeContainer.Q<MinMaxSlider>("MinMaxSlider");

			topDragger = cachedRangeContainer.Q<VisualElement>("unity-thumb-max");
			bottomDragger = cachedRangeContainer.Q<VisualElement>("unity-thumb-min");

			topDragger.hierarchy.Add(StartRangeLabel);
			bottomDragger.hierarchy.Add(EndRangeLabel);

			cachedRangesDisplay = cachedRangeContainer.Q<VisualElement>("CachedRangesDisplay");

			SwitchUnitsButton = cachedRangeContainer.Q<Button>("UnitSwitch");
			SwitchUnitsButton.RegisterCallback<ClickEvent>(_ => ToggleUnitSwitch());

			SwitchUnitsIcon = SwitchUnitsButton.Q<VisualElement>("Icon");

		}

		private void Start() {
			SetVisibility(RangeContainer, false);
		}

		public void OnDateFiltered() {
			IsActive = true;

			if (SelectedDataPoint) {
				SetVisibility(historyContainer, IsActive);
				SetVisibility(CurrentTimeLabel, IsActive);
				SetVisibility(RangeContainer, IsActive);
				StartCoroutine(GenerateTicks(GetMeasurementCount()));
			}
		}

		private void SetVisibility(VisualElement element, bool isVisible) {
			element.style.display = new StyleEnum<DisplayStyle>(isVisible ? DisplayStyle.Flex : DisplayStyle.None);
		}

	}

}