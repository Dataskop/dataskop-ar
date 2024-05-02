using UnityEngine;

namespace Dataskop.Entities.Visualizations {

	[CreateAssetMenu(fileName = "Options", menuName = "VisOptions/Add Bar Option...", order = 3)]
	public class BarOptions : ScriptableObject {

		public Color fillColor;
		public Style[] styles;

	}

}