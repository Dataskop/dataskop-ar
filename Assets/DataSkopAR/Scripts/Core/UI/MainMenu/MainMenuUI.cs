using System.Collections;
using DataskopAR.Data;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;
using UnityEngine.XR.ARFoundation;
using Button = UnityEngine.UI.Button;

#if UNITY_ANDROID
using UnityEngine.Android;
#endif

#if UNITY_IOS
using UnityEngine.iOS;
#endif

namespace DataskopAR.UI {

	public class MainMenuUI : MonoBehaviour {

#region Fields

		[Header("References")]
		[SerializeField] private UIDocument mainMenuUIDoc;
		[SerializeField] private Button closeBtn;
		[SerializeField] private ARSession arSession;

		[Header("Events")]
		public UnityEvent<string> loginButtonPressed;
		public UnityEvent scanButtonPressed;

		private string token = string.Empty;

#endregion

#region Properties

		private VisualElement Root { get; set; }
		private TextField TokenTextField { get; set; }

		private string Token {
			get => token;
			set {
				token = value;
				TokenTextField.value = token;
			}
		}

		private Label VersionLabel { get; set; }
		private bool HasEnteredToken => TokenTextField?.value != string.Empty;

#endregion

#region Methods

		public void OnEnable() {
			Root = mainMenuUIDoc.rootVisualElement;

			Root.Q<UnityEngine.UIElements.Button>("btnScan").RegisterCallback<ClickEvent>(e => {
				closeBtn.gameObject.SetActive(true);
				scanButtonPressed?.Invoke();
				ToggleMainMenuScreen(false);
				ToggleCamera(true);
			});

			Root.Q<UnityEngine.UIElements.Button>("btnDemo")
				.RegisterCallback<ClickEvent>(e => { OnDemoButtonPressed(); });

			Root.Q<UnityEngine.UIElements.Button>("btnLogin")
				.RegisterCallback<ClickEvent>(e => { OnLoginButtonPressed(); });

			TokenTextField = Root.Q<TextField>("txtAPISecret");
			TokenTextField.value = "";

			VersionLabel = Root.Q<Label>("footerCopyright");
			VersionLabel.text = Version.ID + " ©FHSTP (2022, 2023)";
		}

		private IEnumerator Start() {

#if UNITY_IOS
			yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);
#elif UNITY_ANDROID
			Permission.RequestUserPermissions(new[] {
				Permission.Camera,
				Permission.FineLocation
			});
			yield return Permission.HasUserAuthorizedPermission(Permission.Camera);
#else
			yield break;
#endif

		}

		public void ToggleMainMenuScreen(bool status) {
			Root.style.visibility = new StyleEnum<Visibility>(status ? Visibility.Visible : Visibility.Hidden);
		}

		public void ToggleCamera(bool status) {
			arSession.enabled = status;
		}

		public void DisableCloseButton() {
			closeBtn.gameObject.SetActive(false);
		}

		public void OnQrCodeScanned(QrResult result) {

			DisableCloseButton();
			ToggleMainMenuScreen(true);
			Token = result.Code;

			NotificationHandler.Add(new Notification {
				Category = NotificationCategory.Check,
				Text = "Scanned QR successfully!",
				DisplayDuration = NotificationDuration.Short
			});

		}

		private void OnLoginButtonPressed() {

			if (HasEnteredToken) {
				Token = TokenTextField.value;
				loginButtonPressed?.Invoke(Token);
			}
			else {
				NotificationHandler.Add(new Notification {
					Category = NotificationCategory.Error,
					Text = "No token entered!",
					DisplayDuration = NotificationDuration.Short
				});
			}

		}

		public void OnTokenChecked(string checkedToken, bool isValid) {

			Token = checkedToken;

			if (isValid) {
				AccountManager.Login(Token);
				SceneMaster.LoadScene(2);
			}
			else {
				NotificationHandler.Add(new Notification {
					Category = NotificationCategory.Error,
					Text = "The provided token is not valid!",
					DisplayDuration = NotificationDuration.Short
				});
			}

		}

		private static void OnDemoButtonPressed() {
			AccountManager.Login("313f1398d57643ac90915b1b497db58141826a3d0c9a4f97a1cebc9f10db4e1e");
			SceneMaster.LoadScene(3);
		}

#endregion

	}

}