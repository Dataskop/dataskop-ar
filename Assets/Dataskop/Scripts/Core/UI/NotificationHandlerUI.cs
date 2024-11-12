using System.Collections;
using System.Collections.Generic;
using Dataskop.Data;
using UnityEngine;
using UnityEngine.UIElements;

namespace Dataskop.UI {

	public class NotificationHandlerUI : MonoBehaviour {

		private const string MenuOpenAnimation = "notification-animation";

		[Header("References")]
		[SerializeField] private UIDocument notificationUiDocument;

		[Header("Values")]
		[SerializeField] private Sprite[] notificationIcons = new Sprite[4];
		[SerializeField] private NotificationIconColors iconColors;

		private Coroutine notificationCoroutine;

		private Dictionary<NotificationCategory, NotificationStyle> NotificationStyles { get; set; }

		private VisualElement Root { get; set; }

		private VisualElement NotificationEl { get; set; }

		private VisualElement IconElement { get; set; }

		private Label MessageTextElement { get; set; }

		private void Start() {
			NotificationStyles = new Dictionary<NotificationCategory, NotificationStyle> {
				{
					NotificationCategory.Check, new NotificationStyle {
						Icon = notificationIcons[0],
						Color = iconColors.check
					}
				}, {
					NotificationCategory.Info, new NotificationStyle {
						Icon = notificationIcons[1],
						Color = iconColors.info
					}
				}, {
					NotificationCategory.Warning, new NotificationStyle {
						Icon = notificationIcons[2],
						Color = iconColors.warning
					}
				}, {
					NotificationCategory.Error, new NotificationStyle {
						Icon = notificationIcons[3],
						Color = iconColors.error
					}
				}
			};
		}

		private void OnEnable() {
			NotificationHandler.notificationAdded += OnNotificationAdded;
			Root = notificationUiDocument.rootVisualElement;
			NotificationEl = Root.Q<VisualElement>("notification-box");
			IconElement = Root.Q<VisualElement>("icon");
			MessageTextElement = Root.Q<Label>("text");
		}

		private void OnDisable() {
			NotificationHandler.notificationAdded -= OnNotificationAdded;
		}

		private void OnNotificationAdded() {
			notificationCoroutine ??= StartCoroutine(DisplayNotifications());
		}

		private IEnumerator DisplayNotifications() {

			while (NotificationHandler.QueueCount > 0) {
				NotificationEl.RemoveFromClassList(MenuOpenAnimation);
				yield return new WaitForSeconds(0.1f);
				Notification currentNotification = NotificationHandler.Take();
				StyleNotification(currentNotification);
				NotificationEl.AddToClassList(MenuOpenAnimation);
				yield return new WaitForSeconds(currentNotification.DisplayDuration);
			}

			NotificationEl.RemoveFromClassList(MenuOpenAnimation);
			yield return null;
			notificationCoroutine = null;

		}

		private void StyleNotification(Notification notification) {
			IconElement.style.backgroundImage = new StyleBackground(NotificationStyles[notification.Category].Icon);
			IconElement.style.unityBackgroundImageTintColor =
				new StyleColor(NotificationStyles[notification.Category].Color);

			MessageTextElement.text = notification.Text;
		}

	}

}