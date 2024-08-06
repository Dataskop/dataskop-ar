using System;
using System.Collections;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using ZXing;

namespace Dataskop.Interaction {

	public class ArQrReader : MonoBehaviour {

		[Header("Events")]
		public UnityEvent<QrResult> onQrCodeRead;

		[Header("References")]
		[SerializeField] private ARCameraManager arCamManager;

		private readonly WaitForSeconds qrReadCooldown = new(10f);

		private BarcodeReader QrReader { get; set; }

		private Texture2D ReadTexture { get; set; }

		private bool HasReadQrCode { get; set; }

		private bool ShouldLookForQrCode { get; set; }

		private void Start() {
			QrReader = new BarcodeReader();
			ShouldLookForQrCode = true;
		}

		private void OnEnable() {
			arCamManager.frameReceived += GetImageAsync;
		}

		private void OnDisable() {
			arCamManager.frameReceived -= GetImageAsync;
		}

		private void GetImageAsync(ARCameraFrameEventArgs e) {

			if (!ShouldLookForQrCode)
				return;

			if (HasReadQrCode)
				return;

			if (Time.frameCount % 30 == 0) {

				if (!arCamManager.TryAcquireLatestCpuImage(out XRCpuImage image)) {
					return;
				}

				StartCoroutine(ProcessImage(image));
				image.Dispose();

			}

		}

		private IEnumerator ProcessImage(XRCpuImage image) {

			XRCpuImage.AsyncConversion request = image.ConvertAsync(new XRCpuImage.ConversionParams {
				inputRect = new RectInt(0, 0, image.width, image.height),
				outputDimensions = new Vector2Int(image.width / 2, image.height / 2), //downsample by 2 to go fast
				outputFormat = TextureFormat.RGB24,
				transformation = XRCpuImage.Transformation.MirrorY
			});

			while (!request.status.IsDone())
				yield return null;

			if (request.status != XRCpuImage.AsyncConversionStatus.Ready) {
				Debug.LogErrorFormat("Request failed with status {0}", request.status);
				request.Dispose();
				yield break;
			}

			// get image data to apply it to the 2D Texture
			NativeArray<byte> rawData = request.GetData<byte>();

			if (ReadTexture == null)
				ReadTexture = new Texture2D(request.conversionParams.outputDimensions.x, request.conversionParams.outputDimensions.y,
					request.conversionParams.outputFormat, false);

			ReadTexture.LoadRawTextureData(rawData);
			ReadTexture.Apply();

			// don't care for the qr code if one has already been read during the wait for the async calls
			if (!HasReadQrCode)
				try {
					Result result = QrReader.Decode(ReadTexture.GetPixels32(), ReadTexture.width, ReadTexture.height);

					if (result != null) {
						QrResult qr = new(result.Text);
						onQrCodeRead?.Invoke(qr);
						HasReadQrCode = true;
						StartCoroutine(EnableQrReading());
					}

				}
				catch (Exception exception) {
					Debug.LogError(exception.Message);
				}

			request.Dispose();

		}

		public void SetQrScanStatus(bool shouldScan) {
			ShouldLookForQrCode = shouldScan;
		}

		private IEnumerator EnableQrReading() {
			yield return qrReadCooldown;
			HasReadQrCode = false;
		}

	}

}