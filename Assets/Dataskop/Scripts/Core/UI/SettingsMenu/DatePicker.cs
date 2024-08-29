#nullable enable

using System;
using System.Globalization;
using Dataskop.Data;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace Dataskop.UI {

	public class DatePicker : MonoBehaviour {

		public UnityEvent<TimeRange>? dateFilterButtonPressed;

		private bool dateFilterActive;
		private DateTime? fromDate;
		private DateTime? toDate;
		private VisualElement dateFromContainer = null!;
		private VisualElement dateToContainer = null!;
		private TextField dateFromInput = null!;
		private TextField dateToInput = null!;
		private Button dateFilterButton = null!;
		private CultureInfo culture = null!;

		private void Awake() {
			culture = CultureInfo.CurrentCulture;
			VisualElement root = GetComponent<UIDocument>().rootVisualElement;
			dateFromContainer = root.Q<VisualElement>("DateFromContainer");
			dateToContainer = root.Q<VisualElement>("DateToContainer");
			dateFromInput = dateFromContainer.Q<TextField>("DateInputFrom");
			dateFromInput.RegisterCallback<ChangeEvent<string>>(OnDateInputFromChanged);
			dateToInput = dateToContainer.Q<TextField>("DateInputTo");
			dateToInput.RegisterCallback<ChangeEvent<string>>(OnDateInputToChanged);
			dateFilterButton = root.Q<Button>("DateFilterButton");
			dateFilterButton.RegisterCallback<ClickEvent>(OnDateFilterButtonPressed);
		}

		private void OnDateInputFromChanged(ChangeEvent<string> e) {

			if (DateTime.TryParse(e.newValue, culture, DateTimeStyles.None, out DateTime newDate)) {
				fromDate = newDate;
				return;
			}

			fromDate = null;

		}

		private void OnDateInputToChanged(ChangeEvent<string> e) {

			if (DateTime.TryParse(e.newValue, culture, DateTimeStyles.None, out DateTime newDate)) {
				toDate = newDate;
				return;
			}

			toDate = null;

		}

		private void OnDateFilterButtonPressed(ClickEvent e) {

			if (fromDate != null && toDate != null) {
				TimeRange foundRange = new(fromDate.Value, toDate.Value);
				dateFilterButtonPressed?.Invoke(foundRange);
				dateFromInput.value = foundRange.StartTime.ToString("s", culture);
				dateToInput.value = foundRange.EndTime.ToString("s", culture);
			}
			else {
				NotificationHandler.Add(new Notification {
					Category = NotificationCategory.Error,
					Text = "Please enter valid start and end dates.",
					DisplayDuration = NotificationDuration.Flash,
					UniqueID = null
				});
			}

		}

	}

}