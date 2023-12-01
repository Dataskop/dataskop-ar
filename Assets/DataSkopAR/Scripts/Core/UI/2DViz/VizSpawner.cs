using UnityEngine;
using DataSkopAR.Data;
using Newtonsoft.Json;

namespace DataSkopAR.UI {

	public class VizSpawner : MonoBehaviour {

		[SerializeField] private RectTransform panel;
		[SerializeField] private GameObject panelObject;
		[SerializeField] private CanvasGroup canvasGroup;
		
#if !HAS_UNIWEBVIEW
		private UniWebView webView;

		// Start is called before the first frame update
		private void Start() {
			// Create a WebView component
			webView = panelObject.AddComponent<UniWebView>();

			// Set WebView panel
			webView.ReferenceRectTransform = panel;

			// Load local HTML file
			var url = UniWebViewHelper.StreamingAssetURLForPath("linechart.html");
			webView.Load(url);

		}

		public void OnInfoCardStateChanged(InfoCardState state) {
			if (state == InfoCardState.Fullscreen) {
				ShowWebView();
			}
			else {
				HideWebView();
			}
		}

		public void OnDataPointSelected(DataPoint dataPoint) {

			if (dataPoint == null) {
				webView.EvaluateJavaScript("noData()");
				return;
			}

			string jsonString = JsonConvert.SerializeObject(dataPoint.MeasurementDefinition.MeasurementResults);
			string escapedJsonString = jsonString.Replace("\"", "\\\"");
			webView.EvaluateJavaScript($"getData('{escapedJsonString}')");

		}

		private void ShowWebView() {
			canvasGroup.alpha = 1;
			canvasGroup.interactable = true;
			canvasGroup.blocksRaycasts = true;
			webView.Show();
		}

		private void HideWebView() {
			webView.Hide();
			canvasGroup.alpha = 0;
			canvasGroup.interactable = false;
			canvasGroup.blocksRaycasts = false;
		}

#endif

	}

}