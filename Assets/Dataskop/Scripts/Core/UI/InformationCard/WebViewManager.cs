using UnityEngine;
using System.IO;
using System.Collections;
using Dataskop.Entities;

namespace Dataskop.UI
{
    public class WebViewManager : MonoBehaviour
    {
        private WebViewObject webViewObject;
        private bool isDetailsTab = false;

        public string htmlFileName = "index.html";

        void Start()
        {
            // Check if WebViewObject is available
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS
            InitializeWebView();
#else
                Debug.LogError("WebView is not supported on this platform");
#endif
            // Ensure visibility is explicitly set
            Debug.Log("FLO: WebView Initialization Started");
        }

        private void InitializeWebView()
        {
            Debug.Log("FLO: InitializeWebView called");
            try
            {
                // Create WebView object
                webViewObject = new GameObject("WebViewObject").AddComponent<WebViewObject>();

                if (webViewObject == null)
                {
                    Debug.LogError(
                        "Failed to create WebViewObject. Ensure the unity-webview plugin is correctly imported.");
                    return;
                }

                webViewObject.Init(
                    cb: HandleWebViewCallback, // Callback messages from JavaScript
                    err: HandleWebViewError, // Error handling
                    httpErr: HandleHttpError, // HTTP error handling
                    started: HandleWebViewStarted, // When WebView starts
                    hooked: HandleWebViewHooked, // When content is loaded
                    ld: HandleWebViewLoaded // When page is fully loaded
                );

                webViewObject.SetMargins(150, (int)(Screen.height / 2), 150, 30);
                //webViewObject.SetVisibility(true);

                StartCoroutine(LoadWebViewContent());

            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error initializing WebView: {e.Message}");
            }
        }

        private IEnumerator LoadWebViewContent()
        {
            // Construct full path to HTML file in StreamingAssets
            var htmlPath = Path.Combine(Application.streamingAssetsPath, htmlFileName);
            Debug.Log($"FLO: Attempting to load HTML from: {htmlPath}");

            // For platforms that need file:// prefix
            webViewObject.LoadURL($"file://{htmlPath}");
            
            // Wait a moment to ensure initialization
            yield return new WaitForSeconds(0.5f);
        }

        public void SendSensorDataToWebView(object[] sensorData)
        {
            var jsonData = JsonUtility.ToJson(new Serialization<object>(sensorData));
            Debug.Log($"FLO: Sending sensor data to WebView: {jsonData}");
            webViewObject.EvaluateJS($"updateSensorData('{jsonData}')");
        }

        // Handle callback messages from JavaScript
        private static void HandleWebViewCallback(string message)
        {
            Debug.Log($"Received from WebView: {message}");

            // Parse specific messages from JavaScript
            if (message.StartsWith("unity:"))
            {
                var data = message[6..];
                ProcessJavaScriptMessage(data);
            }
        }

        private static void ProcessJavaScriptMessage(string message)
        {
            // Do something based on messages from JS :D
            switch (message)
            {
                case "userInteraction":
                    break;

                default:
                    if (message.StartsWith("DATA:"))
                    {
                        var jsonData = message[5..];
                        ProcessReceivedData(jsonData);
                    }

                    break;
            }
        }

        private static void ProcessReceivedData(string jsonData)
        {
            Debug.Log($"Received data from WebView: {jsonData}");
        }

        // Utility helper classes for JSON serialization
        [System.Serializable]
        public class Serialization<T>
        {
            public T[] items;

            public Serialization(T[] items)
            {
                this.items = items;
            }
        }

        // Various WebView event handlers
        private static void HandleWebViewError(string errorMessage)
        {
            Debug.LogError($"WebView Error: {errorMessage}");
        }

        private static void HandleHttpError(string errorMessage)
        {
            Debug.LogError($"HTTP Error in WebView: {errorMessage}");
        }

        private static void HandleWebViewStarted(string message)
        {
            Debug.Log($"WebView Started: {message}");
        }

        private static void HandleWebViewHooked(string message)
        {
            Debug.Log($"WebView Content Hooked: {message}");
        }

        private void HandleWebViewLoaded(string message)
        {
            Debug.Log($"WebView Fully Loaded: {message}");

            // Inject a JavaScript bridge for communication
            const string bridgeScript = @"
            window.Unity = {
                call: function(msg) {
                    window.location = 'unity:' + msg;
                }
            };
        ";
            webViewObject.EvaluateJS(bridgeScript);
        }

        public void ToggleWebViewVisibility(bool isVisible)
        {
            webViewObject.SetVisibility(isVisible);
        }

        public void onInformationCardStateChanged(InfoCardState state)
        {
            if (state == InfoCardState.Fullscreen && isDetailsTab)
            {
                ToggleWebViewVisibility(true);
            } else {
                ToggleWebViewVisibility(false);
            }
        }

        public void onDetailsTabPressed()
        {
            isDetailsTab = true;
        }

        public void onMapTabPressed()
        {
            isDetailsTab = false;
        }

        public void onDataPointSelected(DataPoint dataPoint)
        {
            if (dataPoint != null)
            {
                SendSensorDataToWebView(dataPoint.CurrentMeasurementRange.ToArray());
            }
        }
    }
}
