using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using DataskopAR.UI;
using JetBrains.Annotations;
using UnityEngine;
using Newtonsoft.Json;
using UnityEngine.Events;
using Debug = UnityEngine.Debug;

namespace DataskopAR.Data {

	public class DataManager : MonoBehaviour {

#region Constants

		public static readonly ApiRequestHandler RequestHandler = ApiRequestHandler.Instance;

#endregion

#region Events

		/// <summary>
		/// Invoked when companies and their projects are loaded, without additional info on single projects.
		/// </summary>
		public event Action<IReadOnlyCollection<Company>> HasLoadedProjectList;

		/// <summary>
		/// Invoked once data for the selected project finished loading.
		/// </summary>
		public event Action<Project> HasLoadedProjectData;

		public event Action HasUpdatedMeasurementResults;

		[Header("Events")]
		public UnityEvent<int> fetchedAmountChanged;

#endregion

#region Fields

		[Header("References")]
		[SerializeField] private LoadingIndicator loadingIndicator;

		[Header("Values")]
		[SerializeField] private int fetchAmount = 1;
		[SerializeField] private int fetchInterval = 30000;

#endregion

#region Properties

		private IReadOnlyCollection<Company> Companies { get; set; }
		public Project SelectedProject { get; private set; }

		public int FetchAmount {
			get => fetchAmount;
			private set => fetchAmount = value;
		}

		private LoadingIndicator LoadingIndicator => loadingIndicator;
		private Stopwatch FetchTimer { get; set; }
		private bool ShouldRefetch { get; set; }

#endregion

#region Methods

		public void Initialize() {

#if UNITY_EDITOR

			if (!AccountManager.IsLoggedIn) {

				if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer) {
					AccountManager.Login(Environment.GetEnvironmentVariable("DATASKOP_TOKEN", EnvironmentVariableTarget.User));
				}

			}

#endif

			if (!AccountManager.IsLoggedIn) {

				NotificationHandler.Add(new Notification {
					Category = NotificationCategory.Error,
					Text = "You are not logged in! Logout and enter a valid Token!",
					DisplayDuration = NotificationDuration.Medium
				});

				return;
			}

			UserData.Instance.Token = AccountManager.GetLoginToken();

			if (UserData.Instance.Token == string.Empty) {

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
		/// Starts process of loading data for the application.
		/// </summary>
		private async void LoadAppData() {

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

			HasLoadedProjectList?.Invoke(Companies);
			LoadingIndicator.Hide();

		}

		private static async Task<IReadOnlyCollection<Company>> UpdateCompanies() {

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
		/// Loads a project based on its ID.
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
		/// Loads a project based on a given QR-Code-Result
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
		/// Gets available Projects from a set of Companies.
		/// </summary>
		/// <param name="userCompanies">A collection of companies</param>
		/// <returns>An Enumerable of available projects for the given companies.</returns>
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
			LoadingIndicator.Hide();

			NotificationHandler.Add(new Notification {
				Category = NotificationCategory.Check,
				Text = "Project loaded!",
				DisplayDuration = NotificationDuration.Medium
			});

			ShouldRefetch = true;
			FetchTimer = new Stopwatch();
			FetchTimer.Start();
			RefetchDataTimer();

		}

		private async void RefetchDataTimer() {

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
			fetchedAmountChanged?.Invoke(FetchAmount);

			LoadingIndicator.Hide();

		}

		private async void OnRefetchTimerElapsed() {
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
			FetchAmount = Mathf.Clamp(newValue, 1, 1000);
		}

		private void OnDisable() {
			ShouldRefetch = false;
			FetchTimer?.Stop();
		}

#endregion

	}

}