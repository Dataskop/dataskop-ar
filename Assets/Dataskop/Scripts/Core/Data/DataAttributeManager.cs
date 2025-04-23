using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Dataskop.Data {

	public class DataAttributeManager : MonoBehaviour {

		[Header("Events")]
		public UnityEvent<Project> onAvailableAttributesUpdated;
		public UnityEvent<DataAttribute> onSelectedAttributeChanged;
		public UnityEvent<DataAttribute> onDefaultAttributeSet;

		[Header("References")]
		[SerializeField] private DataManager dataManager;

		public DataAttribute SelectedAttribute { get; private set; }

		private IReadOnlyCollection<DataAttribute> ProjectAttributes { get; set; }

		private void OnEnable() {
			dataManager.HasLoadedProjectData += SetDataAttributes;
		}

		private void OnDisable() {
			dataManager.HasLoadedProjectData -= SetDataAttributes;
		}

		public event Action<DataAttribute> SelectedAttributeChanged;

		private void SetDataAttributes(Project selectedProject) {

			if (selectedProject.Properties == null) {

				NotificationHandler.Add(
					new Notification {
						Category = NotificationCategory.Warning,
						Text = "Project has no additional properties!",
						DisplayDuration = NotificationDuration.Medium
					}
				);

				return;
			}

			ProjectAttributes = new List<DataAttribute>();

			if (selectedProject.Properties.Attributes.Count == 0) {

				NotificationHandler.Add(
					new Notification {
						Category = NotificationCategory.Error,
						Text = "Selected Project has no attributes!",
						DisplayDuration = NotificationDuration.Medium
					}
				);

				return;
			}

			List<DataAttribute> attributes = selectedProject.Properties.Attributes.ToList();
			ProjectAttributes = attributes;
			// Select the first one that is not ALL
			SetDefaultAttribute(ProjectAttributes.First().ID);
			onAvailableAttributesUpdated?.Invoke(selectedProject);

		}

		private void SetDefaultAttribute(string attributeId) {
			SelectedAttribute = ProjectAttributes.SingleOrDefault(attribute => attribute.ID == attributeId);
			onDefaultAttributeSet?.Invoke(SelectedAttribute);
		}

		public void SetSelectedAttribute(string attributeId) {

			if (SelectedAttribute.ID == attributeId) {
				return;
			}

			SelectedAttribute = ProjectAttributes.SingleOrDefault(attribute => attribute.ID == attributeId);

			if (SelectedAttribute == null) {
				return;
			}

			onSelectedAttributeChanged?.Invoke(SelectedAttribute);
			SelectedAttributeChanged?.Invoke(SelectedAttribute);

		}

	}

}