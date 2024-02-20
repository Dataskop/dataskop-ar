using System;
using System.Collections.Generic;
using System.Linq;
using DataskopAR.Data;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace DataskopAR.UI {

	public class ProjectSelectorUI : MonoBehaviour {

#region Fields

		[Header("Events")]
		public UnityEvent onProjectSelected;

		[Header("References")]
		[SerializeField] private UIDocument settingsMenuUIDoc;
		[SerializeField] private VisualTreeAsset groupOfProjectsTemplate;
		[SerializeField] private VisualTreeAsset projectTemplate;
		[SerializeField] private DataManager dataManager;

		[Header("Values")]
		[SerializeField] private Color selectedIconColor;
		[SerializeField] private Color deselectedIconColor;

		private bool isDescending = false;

#endregion

#region Properties

		private VisualElement MenuContainer { get; set; }
		private ICollection<Button> ProjectButtons { get; set; }
		private Button SortButton { get; set; }
		private StyleColor SelectedColor { get; set; }
		private StyleColor DeselectedColor { get; set; }

#endregion

#region Methods

		private void Awake() {
			MenuContainer = settingsMenuUIDoc.rootVisualElement.Q<VisualElement>("MenuContainer");
			SortButton = MenuContainer.Q<Button>("SortButton");
			SortButton.RegisterCallback<ClickEvent>(_ => OnSortButtonPressed());

			ProjectButtons = new List<Button>();
			SelectedColor = new StyleColor(selectedIconColor);
			DeselectedColor = new StyleColor(deselectedIconColor);

		}

		public void UpdateCompaniesWithProjects(IReadOnlyCollection<Company> companies) {

			ScrollView contentContainer = MenuContainer.Q<ScrollView>("ProjectSelectionContainer");

			foreach (Company company in companies) {

				TemplateContainer groupOfProjectsTemplateContainer = groupOfProjectsTemplate.Instantiate();
				Label label = groupOfProjectsTemplateContainer.Q<Label>("company-name");
				label.text = company.Information.Name;

				IEnumerable<Project> sortedCompanyProjects = company.Projects.OrderBy(x => x.Information.Name);

				foreach (Project project in sortedCompanyProjects) {

					if (project.Properties != null) {

						if (project.Properties.IsDemo && !DataPointsManager.IsDemoScene) {
							continue;
						}

						if (!project.Properties.IsDemo && DataPointsManager.IsDemoScene) {
							continue;
						}

					}
					else {
						// Only show projects with possible null properties in the normal app, omit them in the demo scene.
						if (DataPointsManager.IsDemoScene) {
							continue;
						}
					}

					TemplateContainer projectTemplateContainer = projectTemplate.Instantiate();
					VisualElement container = projectTemplateContainer.Q<VisualElement>("ProjectButtonContainer");
					Button button = container.Q<Button>("project-button");
					ProjectButtons.Add(button);
					button.text = project.Information.Name;

					button.clickable.clicked += () => {
						onProjectSelected?.Invoke();
						dataManager.LoadProject(project.ID);
					};

					VisualElement companyProjectsBox = groupOfProjectsTemplateContainer.Q<VisualElement>("company-projects-box");
					companyProjectsBox.Add(projectTemplateContainer);
				}

				if (groupOfProjectsTemplateContainer.Q<VisualElement>("company-projects-box").childCount == 0) {
					continue;
				}

				contentContainer.Add(groupOfProjectsTemplateContainer);
			}

			if (contentContainer.childCount == 0) {
				Label noProjectsText = new() {
					text = "No Projects found!"
				};

				contentContainer.Add(noProjectsText);
			}

		}

		private void OnSortButtonPressed() {

			isDescending = !isDescending;

		}

		public void MarkSelectedProject(Project currentProject) {

			if (ProjectButtons.Count == 0) {
				return;
			}

			foreach (Button b in ProjectButtons) {
				if (currentProject?.Information.Name == b.text) {
					b.style.borderBottomColor = SelectedColor;
					b.style.borderRightColor = SelectedColor;
					b.style.borderLeftColor = SelectedColor;
				}
				else {
					b.style.borderBottomColor = DeselectedColor;
					b.style.borderRightColor = DeselectedColor;
					b.style.borderLeftColor = DeselectedColor;
				}
			}

		}

#endregion

	}

}