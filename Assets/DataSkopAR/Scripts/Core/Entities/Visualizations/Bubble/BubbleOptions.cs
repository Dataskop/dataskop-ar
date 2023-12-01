using UnityEngine;

namespace DataSkopAR.Entities.Visualizations {

	[CreateAssetMenu(fileName = "Options", menuName = "VisOptions/Add Bubble Option...", order = 2)]
	public class BubbleOptions : ScriptableObject {

		public MaterialOption[] materialOptions;

	}

}