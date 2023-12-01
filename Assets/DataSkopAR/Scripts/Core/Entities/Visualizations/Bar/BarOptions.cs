using UnityEngine;

namespace DataSkopAR.Entities.Visualizations {

	[CreateAssetMenu(fileName = "Options", menuName = "VisOptions/Add Bar Option...", order = 3)]
	public class BarOptions : ScriptableObject {

		public Gradient fillGradientDefault;
		public MaterialOption[] materialOptions;

	}

}