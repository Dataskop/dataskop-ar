using DataskopAR.Data;
using UnityEngine.UIElements;

namespace DataskopAR.UI {

	public class InfoCardHeaderUI : InfoCardComponent {

#region Properties

		protected override VisualElement ComponentRoot { get; set; }
		private VisualElement InfoCard { get; set; }

#endregion

#region Methods

		public override void Init(VisualElement infoCard) {
			InfoCard = infoCard;
			ComponentRoot = InfoCard.Q<VisualElement>("HeaderContainer");
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

#endregion

	}

}