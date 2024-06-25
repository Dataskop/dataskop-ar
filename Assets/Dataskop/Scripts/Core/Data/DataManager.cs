using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Dataskop.UI;
using JetBrains.Annotations;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Events;
using Debug = UnityEngine.Debug;

namespace Dataskop.Data {

	public class DataManager : MonoBehaviour {

		public static readonly ApiRequestHandler RequestHandler = ApiRequestHandler.Instance;

		[Header("Events")]
		public UnityEvent<int> fetchedAmountChanged;
		public UnityEvent<IReadOnlyCollection<Company>> projectListLoaded;
		public UnityEvent<Project> projectLoaded;

		[Header("References")]
		[SerializeField] private LoadingIndicator loadingIndicator;

		[Header("Values")]
		[SerializeField] private int fetchAmount = 1;
		[SerializeField] private int fetchInterval = 30000;

		private IReadOnlyCollection<Company> Companies { get; set; }

		public Project SelectedProject { get; private set; }

		public int FetchAmount {
			get => fetchAmount;
			private set => fetchAmount = value;
		}

		private LoadingIndicator LoadingIndicator => loadingIndicator;

		private Stopwatch FetchTimer { get; set; }

		private bool ShouldRefetch { get; set; }

		private void OnDisable() {
			ShouldRefetch = false;
			FetchTimer?.Stop();
		}

		/// <summary>
		///     Invoked when companies and their projects are loaded, without additional info on single projects.
		/// </summary>
#pragma warning disable CS0067 // Event is never used
		public event Action<IReadOnlyCollection<Company>> HasLoadedProjectList;
#pragma warning restore CS0067 // Event is never used

		/// <summary>
		///     Invoked once data for the selected project finished loading.
		/// </summary>
		public event Action<Project> HasLoadedProjectData;

		/// <summary>
		///     Invoked when measurement results has been updated.
		/// </summary>
		public event Action HasUpdatedMeasurementResults;

		public void Initialize() {

			if (!AccountManager.IsLoggedIn) {

				NotificationHandler.Add(new Notification {
					Category = NotificationCategory.Error,
					Text = "You are not logged in! Logout and enter a valid Token!",
					DisplayDuration = NotificationDuration.Medium
				});

				return;
			}

			UserData.Instance.Token = AccountManager.TryGetLoginToken();

			if (UserData.Instance.Token == null) {

				NotificationHandler.Add(new Notification {
					Category = NotificationCategory.Error,
					Text = "Token was empty! Logout and enter a valid Token!",
					DisplayDuration = NotificationDuration.Medium
				});

				return;

			}

			LoadAppData();

		}

		/// <summary>
		///     Starts process of loading data for the application.
		/// </summary>
		async private void LoadAppData() {

			LoadingIndicator.Show();

			Companies = await UpdateCompanies();

			if (Companies == null || Companies.Count == 0) {

				NotificationHandler.Add(new Notification {
					Category = NotificationCategory.Error,
					Text = "No Companies available!",
					DisplayDuration = NotificationDuration.Medium
				});

				LoadingIndicator.Hide();
				return;

			}

			foreach (Company c in Companies) {
				await c.UpdateProjects();
			}

			projectListLoaded?.Invoke(Companies);
			LoadingIndicator.Hide();

		}

		async private static Task<IReadOnlyCollection<Company>> UpdateCompanies() {

			const string url = "https://backend.dataskop.at/api/company/list";
			string rawResponse = await RequestHandler.Get(url);

			try {
				List<Company> companies = JsonConvert.DeserializeObject<List<Company>>(rawResponse);
				return companies;
			}
			catch (Exception e) {
				Debug.LogError(e.Message);
				return null;
			}

		}

		/// <summary>
		///     Loads a project based on its ID.
		/// </summary>
		/// <param name="projectId">The ID of the project to be loaded.</param>
		public async void LoadProject(int projectId) {

			// If project is already selected do not load again.
			if (SelectedProject?.ID == projectId) {
				return;
			}

			if (!LoadingIndicator.IsLoading) {
				LoadingIndicator.Show();
			}

			SelectedProject = GetAvailableProjects(Companies).FirstOrDefault(project => project.ID == projectId);

			if (SelectedProject == null) {
				NotificationHandler.Add(new Notification {
					Category = NotificationCategory.Error,
					Text = $"No Project with ID {projectId} found!",
					DisplayDuration = NotificationDuration.Medium
				});

				LoadingIndicator.Hide();
				return;
			}

			await SelectedProject.UpdateDevices();
			await UpdateProjectMeasurements();

			OnProjectDataLoaded(SelectedProject);

		}

		/// <summary>
		///     Loads a project based on a given QR-Code-Result
		/// </summary>
		/// <param name="result">A QrResult</param>
		[UsedImplicitly]
		public async void LoadProject(QrResult result) {

			if (result.Code.Contains('@')) {

				string[] splitResult = result.Code.Split('@', 2);
				string projectName = splitResult[0];

				// If project is already selected do not load again.
				if (SelectedProject?.Information.Name == projectName) {
					return;
				}

				NotificationHandler.Add(new Notification {
					Category = NotificationCategory.Check,
					Text = "Project Code scanned!",
					DisplayDuration = NotificationDuration.Short
				});

				if (!LoadingIndicator.IsLoading) {
					LoadingIndicator.Show();
				}

				SelectedProject = GetAvailableProjects(Companies)
					.FirstOrDefault(project => project.Information.Name == projectName);

				if (SelectedProject != null) {

					if (!SelectedProject.Properties.IsDemo) {

						NotificationHandler.Add(new Notification {
							Category = NotificationCategory.Error,
							Text = $"Can not load '{projectName}'! Project is not a demo project.",
							DisplayDuration = NotificationDuration.Medium
						});

						LoadingIndicator.Hide();
						return;

					}

				}

				if (SelectedProject == null) {
					NotificationHandler.Add(new Notification {
						Category = NotificationCategory.Error,
						Text = $"No Project with name '{projectName}' found!",
						DisplayDuration = NotificationDuration.Medium
					});

					LoadingIndicator.Hide();
					return;
				}

				await SelectedProject.UpdateDevices();
				await UpdateProjectMeasurements();

				OnProjectDataLoaded(SelectedProject);

			}
			else {
				LoadingIndicator.Hide();
				NotificationHandler.Add(new Notification {
					Category = NotificationCategory.Error,
					Text = "Not a valid Project QR Code!",
					DisplayDuration = NotificationDuration.Medium
				});
			}

		}

		/// <summary>
		///     Gets available Projects from a set of Companies.
		/// </summary>
		/// <param name="userCompanies">A collection of companies</param>
		/// <returns>A collection of available projects for the given companies.</returns>
		public static IEnumerable<Project> GetAvailableProjects(IEnumerable<Company> userCompanies) {

			List<Project> availableProjects = new();

			foreach (Company c in userCompanies) {

				if (c.Projects != null) {
					availableProjects.AddRange(c.Projects);
				}

			}

			return availableProjects;
		}

		/// <summary>
		///     Triggers events when loading of the project is done.
		/// </summary>
		/// <param name="selectedProject">The whole project to be sent in the event when done loading.</param>
		private void OnProjectDataLoaded(Project selectedProject) {

			HasLoadedProjectData?.Invoke(selectedProject);
			projectLoaded?.Invoke(selectedProject);

			LoadingIndicator.Hide();

			NotificationHandler.Add(new Notification {
				Category = NotificationCategory.Check,
				Text = "Project loaded!",
				DisplayDuration = NotificationDuration.Short
			});

			ShouldRefetch = true;
			FetchTimer = new Stopwatch();
			FetchTimer.Start();
			RefetchDataTimer();

		}

		async private void RefetchDataTimer() {

			while (ShouldRefetch) {

				if (FetchTimer?.ElapsedMilliseconds > fetchInterval) {
					OnRefetchTimerElapsed();
					FetchTimer?.Restart();
				}

				await Task.Yield();

			}

		}

		public async Task UpdateProjectMeasurements() {

			LoadingIndicator.Show();

			await SelectedProject.UpdateDeviceMeasurements(FetchAmount);
			HasUpdatedMeasurementResults?.Invoke();

			int fetchedResultsAmount = SelectedProject.Devices.First().MeasurementDefinitions.First().MeasurementResults.Count;

			if (fetchedResultsAmount != FetchAmount) {
				FetchAmount = fetchedResultsAmount;
			}

			fetchedAmountChanged?.Invoke(FetchAmount);

			LoadingIndicator.Hide();

		}

		async private void OnRefetchTimerElapsed() {
			await UpdateProjectMeasurements();
		}

		public async void OnRefetchButtonPressed() {
			await UpdateProjectMeasurements();
		}

		public void OnCooldownInputChanged(int newValue) {
			int milliseconds = newValue * 1000;
			fetchInterval = Mathf.Clamp(milliseconds, 2000, 360000);
		}

		public void OnAmountInputChanged(int newValue) {
			FetchAmount = Mathf.Clamp(newValue, 1, 999);
		}

	}

}