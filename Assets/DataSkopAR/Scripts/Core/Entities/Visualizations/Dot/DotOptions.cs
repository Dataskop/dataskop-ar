using UnityEngine;

namespace DataSkopAR.Entities.Visualizations {

	[CreateAssetMenu(fileName = "Options", menuName = "VisOptions/Add Dot Option...", order = 1)]
	public class DotOptions : ScriptableObject {

		public MaterialOption[] materialOptions;

	}

}