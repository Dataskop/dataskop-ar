using System;
using System.Collections.Generic;
using System.Linq;

namespace DataskopAR.Data {

	public static class NotificationHandler {

#region Events

		public static Action notificationAdded;

#endregion

#region Properties

		private static Queue<Notification> Notifications { get; } = new();

		public static int QueueCount => Notifications.Count;

#endregion

#region Methods

		/// <summary>
		///     Adds a notification to the queue.
		/// </summary>
		/// <param name="notification">The notification to be added</param>
		public static void Add(Notification notification) {
			Notifications.Enqueue(notification);
			notificationAdded?.Invoke();
		}

		/// <summary>
		///     Adds a notification only if a notification with the same UniqueID does not exist yet.
		/// </summary>
		/// <param name="notification">The notification to be added</param>
		public static void AddUnique(Notification notification) {

			if (notification.UniqueID == null) {
				return;
			}

			Notification n = Notifications.ToArray().FirstOrDefault(n => n.UniqueID == notification.UniqueID);

			if (n == null) {
				Notifications.Enqueue(notification);
			}

		}

		/// <summary>
		///     Takes the upcoming notification in the queue.
		/// </summary>
		/// <returns></returns>
		public static Notification Take() {
			return Notifications.Dequeue();
		}

#endregion

	}

}