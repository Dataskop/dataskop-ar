using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace Dataskop.UI {

	public class InfoCardProjectDataUI : InfoCardComponent {

		[Header("Events")]
		public UnityEvent updateMeasurementsButtonPressed;

		protected override VisualElement ComponentRoot { get; set; }

		private VisualElement InfoCard { get; set; }

		public override void Init(VisualElement infoCard) {
			InfoCard = infoCard;
			ComponentRoot = InfoCard.Q<VisualElement>("MetaInformation");

			ComponentRoot.Q<Button>("UpdateProject")
				.RegisterCallback<ClickEvent>(e => { updateMeasurementsButtonPressed?.Invoke(); });
		}

		internal void UpdateProjectNameDisplay(string projectName) {
			Label projectNameLabel = ComponentRoot.Q<Label>("ProjectName");
			projectNameLabel.text = projectName;
		}

		internal void UpdateLastUpdatedDisplay(DateTime lastUpdateTime) {
			lastUpdateTime = lastUpdateTime.ToLocalTime();
			Label lastUpdatedLabel = ComponentRoot.Q<Label>("LastUpdated");
			lastUpdatedLabel.text =
				$"Last Updated: {lastUpdateTime.ToShortDateString()} {lastUpdateTime.ToLongTimeString()}";
		}

		internal void UpdateVisibility() {
			ComponentRoot.Q<Label>("LastUpdated").visible = true;
			ComponentRoot.Q<Button>("UpdateProject").visible = true;
		}

	}

}