using System.Collections;
using Dataskop.Data;
using Dataskop.Interaction;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;
using UnityEngine.XR.ARFoundation;
#if UNITY_ANDROID
using UnityEngine.Android;
#endif

#if UNITY_IOS
using UnityEngine.iOS;
#endif

namespace Dataskop.UI {

	public class MainMenuUI : MonoBehaviour {

		[Header("References")]
		[SerializeField] private UIDocument mainMenuUIDoc;
		[SerializeField] private ArQrReader qrReader;
		[SerializeField] private ARSession arSession;

		[Header("Events")]
		public UnityEvent<string> loginButtonPressed;
		private string defaultToken = string.Empty;

		private string token = string.Empty;

		private VisualElement Container { get; set; }

		private TextField TokenTextField { get; set; }

		private Button CloseButton { get; set; }

		private Button DemoUserButton { get; set; }

		private Label VersionLabel { get; set; }

		private string Token
		{
			get => token;

			set
			{
				token = value;
				TokenTextField.value = token;
			}
		}

		private bool HasEnteredToken => TokenTextField?.value != string.Empty;

		private IEnumerator Start() {

			FPSManager.SetApplicationTargetFrameRate(30);

#if UNITY_IOS
			yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);
#elif UNITY_ANDROID
			Permission.RequestUserPermissions(
				new[] {
					Permission.Camera, Permission.FineLocation
				}
			);
			yield return Permission.HasUserAuthorizedPermission(Permission.Camera);
#else
			yield break;
#endif

		}

		public void OnEnable() {

			Container = mainMenuUIDoc.rootVisualElement.Q<VisualElement>("container");
			CloseButton = mainMenuUIDoc.rootVisualElement.Q<Button>("closeButton");

			Container.Q<Button>("btnScan").RegisterCallback<ClickEvent>(
				_ =>
				{
					ToggleElement(CloseButton, true);
					ToggleElement(Container, false);
					ToggleCamera(true);
					qrReader.SetQrScanStatus(true);
				}
			);

			Container.Q<Button>("btnDemo")
				.RegisterCallback<ClickEvent>(_ => { OnDemoButtonPressed(); });

			Container.Q<Button>("btnLogin")
				.RegisterCallback<ClickEvent>(_ => { OnLoginButtonPressed(); });

			DemoUserButton = Container.Q<Button>("demoUserButton");
			DemoUserButton.RegisterCallback<ClickEvent>(_ => { TokenTextField.value = defaultToken; });

			TokenTextField = Container.Q<TextField>("txtAPISecret");
			defaultToken = TokenTextField.value;

			if (AccountManager.IsLoggedIn && AccountManager.TryGetLoginToken() != TokenTextField.value) {
				TokenTextField.value = AccountManager.TryGetLoginToken();
			}

			VersionLabel = Container.Q<Label>("footerCopyright");
			VersionLabel.text = Version.ID + " ©FHSTP (2022-2024)";

			CloseButton.RegisterCallback<ClickEvent>(
				_ =>
				{
					qrReader.SetQrScanStatus(false);
					ToggleCamera(false);
					ToggleElement(CloseButton, false);
					ToggleElement(Container, true);
				}
			);

			AppOptions.DemoMode = false;
		}

		private void ToggleElement(VisualElement element, bool status) {
			element.style.display = new StyleEnum<DisplayStyle>(status ? DisplayStyle.Flex : DisplayStyle.None);
		}

		private void ToggleCamera(bool status) {
			arSession.enabled = status;
		}

		public void OnQrCodeScanned(QrResult result) {

			qrReader.SetQrScanStatus(false);
			ToggleCamera(false);
			ToggleElement(CloseButton, false);
			ToggleElement(Container, true);

			Token = result.Code;

			NotificationHandler.Add(
				new Notification {
					Category = NotificationCategory.Check,
					Text = "Scanned QR successfully!",
					DisplayDuration = NotificationDuration.Short
				}
			);

		}

		private void OnLoginButtonPressed() {

			if (HasEnteredToken) {
				Token = TokenTextField.value;
				loginButtonPressed?.Invoke(Token);
			}
			else {
				NotificationHandler.Add(
					new Notification {
						Category = NotificationCategory.Error,
						Text = "No token entered!",
						DisplayDuration = NotificationDuration.Short
					}
				);
			}

		}

		public void OnTokenChecked(string checkedToken, bool isValid) {

			Token = checkedToken;

			if (isValid) {
				AccountManager.Login(Token);
				SceneHandler.LoadScene("World");
			}
			else {
				NotificationHandler.Add(
					new Notification {
						Category = NotificationCategory.Error,
						Text = "The provided token is not valid!",
						DisplayDuration = NotificationDuration.Short
					}
				);
			}

		}

		private void OnDemoButtonPressed() {

			if (HasEnteredToken) {
				Token = TokenTextField.value;
			}
			else {
				NotificationHandler.Add(
					new Notification {
						Category = NotificationCategory.Error,
						Text = "Please enter a Demo Token!",
						DisplayDuration = NotificationDuration.Short
					}
				);
			}

			AppOptions.DemoMode = true;
			AccountManager.Login(Token);
			SceneHandler.LoadScene("World");
		}

	}

}