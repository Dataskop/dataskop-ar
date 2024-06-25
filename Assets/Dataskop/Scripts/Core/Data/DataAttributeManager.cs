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

		/// <summary>
		///     Currently selected Attribute
		/// </summary>
		public DataAttribute SelectedAttribute { get; private set; }

		/// <summary>
		///     List of available Attributes in a project.
		/// </summary>
		public ICollection<DataAttribute> ProjectAttributes { get; set; }

		private void OnEnable() {
			dataManager.HasLoadedProjectData += SetDataAttributes;
		}

		private void OnDisable() {
			dataManager.HasLoadedProjectData -= SetDataAttributes;
		}

		public event Action<DataAttribute> selectedAttributeChanged;

		public void SetDataAttributes(Project selectedProject) {

			if (selectedProject.Properties == null) {

				NotificationHandler.Add(new Notification {
					Category = NotificationCategory.Warning,
					Text = "Project has no additional properties!",
					DisplayDuration = NotificationDuration.Medium
				});

				return;
			}

			ProjectAttributes = new List<DataAttribute>();

			if (selectedProject.Properties.Attributes.Count == 0) {

				NotificationHandler.Add(new Notification {
					Category = NotificationCategory.Error,
					Text = "Selected Project has no attributes!",
					DisplayDuration = NotificationDuration.Medium
				});

				return;
			}

			ClearProjectAttributes();

			foreach (DataAttribute attribute in selectedProject.Properties.Attributes) {
				AddDataAttribute(attribute);
			}

			SetDefaultAttribute(selectedProject.Properties.Attributes.First().ID);
			onAvailableAttributesUpdated?.Invoke(selectedProject);

		}

		public void SetDefaultAttribute(string attributeId) {
			SelectedAttribute = ProjectAttributes.SingleOrDefault(attribute => attribute.ID == attributeId);
			onDefaultAttributeSet?.Invoke(SelectedAttribute);
		}

		public void SetSelectedAttribute(string attributeId) {

			if (SelectedAttribute.ID == attributeId)
				return;

			SelectedAttribute = ProjectAttributes.SingleOrDefault(attribute => attribute.ID == attributeId);

			if (SelectedAttribute != null) {
				onSelectedAttributeChanged?.Invoke(SelectedAttribute);
				selectedAttributeChanged?.Invoke(SelectedAttribute);
			}

		}

		public void AddDataAttribute(DataAttribute newDataAttribute) {
			ProjectAttributes.Add(newDataAttribute);
		}

		public void ClearProjectAttributes() {
			ProjectAttributes.Clear();
		}

	}

}