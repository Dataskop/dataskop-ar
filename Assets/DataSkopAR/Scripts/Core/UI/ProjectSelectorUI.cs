using System.Collections.Generic;
using DataSkopAR.Data;
using Mapbox.Unity.Map;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace DataSkopAR.UI {

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

#endregion

#region Properties

		private VisualElement ProjectSelectionPopUpRoot { get; set; }
		private ICollection<Button> ProjectButtons { get; set; }
		private StyleColor SelectedColor { get; set; }
		private StyleColor DeselectedColor { get; set; }

#endregion

#region Methods

		private void OnEnable() {
			dataManager.HasLoadedProjectList += UpdateCompaniesWithProjects;
			dataManager.HasLoadedProjectData += MarkSelectedProject;
		}

		private void Start() {
			ProjectButtons = new List<Button>();
			SelectedColor = new StyleColor(selectedIconColor);
			DeselectedColor = new StyleColor(deselectedIconColor);
		}

		private void UpdateCompaniesWithProjects(IReadOnlyCollection<Company> companies) {

			ProjectSelectionPopUpRoot = settingsMenuUIDoc.rootVisualElement;
			VisualElement contentContainer = ProjectSelectionPopUpRoot.Q<VisualElement>("ProjectsContainer");

			foreach (Company company in companies) {

				TemplateContainer groupOfProjectsTemplateContainer = groupOfProjectsTemplate.Instantiate();
				Label label = groupOfProjectsTemplateContainer.Q<Label>("company-name");
				label.text = company.Information.Name;

				foreach (Project project in company.Projects) {

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
				Label noProjectsText = new () {
					text = "No Projects found!"
				};
				
				contentContainer.Add(noProjectsText);
			}

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

		private void OnDisable() {
			dataManager.HasLoadedProjectList -= UpdateCompaniesWithProjects;
			dataManager.HasLoadedProjectData -= MarkSelectedProject;
		}

#endregion

	}

}