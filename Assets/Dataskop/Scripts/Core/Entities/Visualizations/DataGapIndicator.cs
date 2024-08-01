using UnityEngine;

namespace Dataskop.Entities.Visualizations {

	public class DataGapIndicator : MonoBehaviour {

		[SerializeField] private GameObject lineSegment;
		[SerializeField] private float width;
		[SerializeField] private float gap;
		[SerializeField] private float segmentWidth;

		private void Start() {
			for (int i = 0; i < width / (segmentWidth + gap); i++) {
				GameObject seg = Instantiate(lineSegment, transform);
				seg.GetComponent<RectTransform>().SetLocalPositionAndRotation(
					new Vector3(0 - width / 2 - gap / 2 + (segmentWidth + gap) * i, 0, 0),
					Quaternion.identity);
				seg.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, segmentWidth);
			}
		}

	}

}