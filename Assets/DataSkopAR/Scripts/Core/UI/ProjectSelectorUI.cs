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

		private bool isDescending;

#endregion

#region Properties

		private VisualElement MenuContainer { get; set; }

		private ICollection<Button> ProjectButtons { get; set; }

		private Button SortButton { get; set; }

		private ScrollView ProjectsContainer { get; set; }

		private StyleColor SelectedColor { get; set; }

		private StyleColor DeselectedColor { get; set; }

		private IReadOnlyCollection<Company> Companies { get; set; }

#endregion

#region Methods

		private void Awake() {

			MenuContainer = settingsMenuUIDoc.rootVisualElement.Q<VisualElement>("MenuContainer");
			SortButton = MenuContainer.Q<Button>("SortButton");
			SortButton.RegisterCallback<ClickEvent>(_ => OnSortButtonPressed());

			ProjectsContainer = MenuContainer.Q<ScrollView>("ProjectSelectionContainer");

			ProjectButtons = new List<Button>();
			SelectedColor = new StyleColor(selectedIconColor);
			DeselectedColor = new StyleColor(deselectedIconColor);

		}

		public void OnProjectListLoaded(IReadOnlyCollection<Company> companies) {
			Companies = companies;
			UpdateCompaniesWithProjects(Companies);
		}

		private void UpdateCompaniesWithProjects(IEnumerable<Company> companies) {

			ProjectsContainer.Clear();

			foreach (Company company in companies) {

				TemplateContainer groupOfProjectsTemplateContainer = groupOfProjectsTemplate.Instantiate();
				Label label = groupOfProjectsTemplateContainer.Q<Label>("company-name");
				label.text = company.Information.Name;

				IEnumerable<Project> sortedCompanyProjects = isDescending
					? company.Projects.OrderByDescending(x => x.Information.Name)
					: company.Projects.OrderBy(x => x.Information.Name);

				foreach (Project project in sortedCompanyProjects) {

					if (project.Properties != null) {

						if (project.Properties.IsDemo && !AppOptions.DemoMode) {
							continue;
						}

						if (!project.Properties.IsDemo && AppOptions.DemoMode) {
							continue;
						}

					}
					else {
						// Only show projects with possible null properties in the normal app, omit them in the demo scene.
						if (AppOptions.DemoMode) {
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

				ProjectsContainer.Add(groupOfProjectsTemplateContainer);
			}

			if (ProjectsContainer.childCount != 0) {
				return;
			}

			Label noProjectsText = new() {
				text = "No Projects found!"
			};

			ProjectsContainer.Add(noProjectsText);

		}

		private void OnSortButtonPressed() {
			isDescending = !isDescending;
			UpdateCompaniesWithProjects(Companies);
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