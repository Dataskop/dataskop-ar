using Dataskop.Data;
using UnityEngine.UIElements;

namespace Dataskop.UI {

	public class InfoCardLocatorUI : InfoCardComponent {

		protected override VisualElement ComponentRoot { get; set; }

		private VisualElement InfoCard { get; set; }

		public override void Init(VisualElement infoCard) {
			InfoCard = infoCard;
			ComponentRoot = InfoCard.Q<VisualElement>("LocatorContainer");
		}

		public void OnUserAreaLocated(LocationArea locationArea) {

			Label location = ComponentRoot.Q<Label>("Location");
			Label area = ComponentRoot.Q<Label>("Area");

			if (locationArea == null) {
				location.text = "No Registered Location Detected.";
				area.text = "No Area Detected.";
				return;
			}

			location.text = locationArea.LocationName;
			area.text = locationArea.AreaName;

		}

	}

}