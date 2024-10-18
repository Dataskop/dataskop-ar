using Dataskop.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Dataskop {

	public class HistoryControllerUI : MonoBehaviour {

		[SerializeField] private UIDocument historyUIDocument;
		[SerializeField] private HistorySliderUI historySlider;
		[SerializeField] private CachedRangesUI cachedRangesDisplay;

		// Start is called before the first frame update
		void Start() {
			historyUIDocument.rootVisualElement.visible = false;
		}

	}

}