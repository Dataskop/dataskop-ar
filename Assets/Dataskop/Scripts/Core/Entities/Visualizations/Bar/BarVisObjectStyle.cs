using UnityEngine;

namespace Dataskop.Entities.Visualizations {

	[CreateAssetMenu(fileName = "Options", menuName = "VisOptions/Add Bar Option...", order = 3)]
	public class BarVisObjectStyle : ScriptableObject, IVisObjectStyle {

		public VisObjectStyle[] styles;
		public Material focusedFillMaterial;
		public Material historyFillMaterial;

		public VisObjectStyle[] Styles => styles;

	}

}