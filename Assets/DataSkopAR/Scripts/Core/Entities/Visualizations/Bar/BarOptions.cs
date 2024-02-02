using UnityEngine;

namespace DataskopAR.Entities.Visualizations {

	[CreateAssetMenu(fileName = "Options", menuName = "VisOptions/Add Bar Option...", order = 3)]
	public class BarOptions : ScriptableObject {

		public Color fillColor;
		public MaterialOption[] materialOptions;

	}

}