using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Dataskop.UI;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;

namespace Dataskop.Data {

	public class DataManager : MonoBehaviour {

		[Header("Events")]
		public UnityEvent<IReadOnlyCollection<Company>> projectListLoaded;
		public UnityEvent<Project> projectLoaded;

		[Header("References")]
		[SerializeField] private LoadingIndicator loadingIndicator;

		[Header("Values")]
		[SerializeField] private int fetchAmount;
		[SerializeField] private int fetchInterval;

		public readonly ApiRequestHandler RequestHandler = new();

		private IReadOnlyCollection<Company> Companies { get; set; }

		public Project SelectedProject { get; private set; }

		public int FetchAmount {
			get => fetchAmount;
			private set => fetchAmount = value;
		}

		private LoadingIndicator LoadingIndicator => loadingIndicator;

		private Stopwatch FetchTimer { get; set; }

		public bool ShouldRefetch { get; set; }

		private void Awake() {
			FetchAmount = PlayerPrefs.HasKey("fetchAmount") ? PlayerPrefs.GetInt("fetchAmount") : 2000;
			fetchInterval = PlayerPrefs.HasKey("fetchInterval") ? PlayerPrefs.GetInt("fetchInterval") : 10000;
		}

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

		public event Action<TimeRange> HasDateFiltered;

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
		private async void LoadAppData() {

			LoadingIndicator.Show();

			Companies = await RequestHandler.GetCompanies();

			if (Companies == null || Companies.Count == 0) {

				NotificationHandler.Add(new Notification {
					Category = NotificationCategory.Error,
					Text = "No Companies available!",
					DisplayDuration = NotificationDuration.Medium
				});

				LoadingIndicator.Hide();
				return;

			}

			foreach (Company company in Companies) {
				company.Projects = await RequestHandler.GetProjects(company);
			}

			projectListLoaded?.Invoke(Companies);
			LoadingIndicator.Hide();

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

			ShouldRefetch = false;
			LoadingIndicator.Show();

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

			SelectedProject.Devices = await RequestHandler.GetDevices(SelectedProject);

			if (SelectedProject.Devices?.Count == 0) {
				NotificationHandler.Add(new Notification {
					Category = NotificationCategory.Warning,
					Text = $"No Devices found in Project {SelectedProject.ID}!",
					DisplayDuration = NotificationDuration.Medium
				});

				LoadingIndicator.Hide();
				OnProjectDataLoaded(SelectedProject);
				return;
			}

			LoadingIndicator.Hide();
			await GetInitialProjectMeasurements();
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
					ShouldRefetch = false;
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
				else {
					NotificationHandler.Add(new Notification {
						Category = NotificationCategory.Error,
						Text = $"No Project with name '{projectName}' found!",
						DisplayDuration = NotificationDuration.Medium
					});

					LoadingIndicator.Hide();
					return;
				}

				SelectedProject.Devices = await RequestHandler.GetDevices(SelectedProject);

				if (SelectedProject.Devices?.Count == 0) {
					NotificationHandler.Add(new Notification {
						Category = NotificationCategory.Warning,
						Text = $"No Devices found in Project {SelectedProject.ID}!",
						DisplayDuration = NotificationDuration.Medium
					});

					OnProjectDataLoaded(SelectedProject);
					return;
				}

				await GetInitialProjectMeasurements();
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
		private static IEnumerable<Project> GetAvailableProjects(IEnumerable<Company> userCompanies) {

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

		private async Task GetInitialProjectMeasurements() {

			LoadingIndicator.Show();

			foreach (Device d in SelectedProject.Devices) {
				foreach (MeasurementDefinition md in d.MeasurementDefinitions) {
					md.FirstMeasurementResult = await RequestHandler.GetFirstMeasurementResult(md);
					int? count = await RequestHandler.GetCount(md);
					md.TotalMeasurements = count ?? -1;
					MeasurementResultRange newResults = await RequestHandler.GetMeasurementResults(md, FetchAmount, null, null);
					md.AddMeasurementResultRange(newResults, new TimeRange(newResults.Last().Timestamp, newResults.First().Timestamp));
				}
			}

			HasUpdatedMeasurementResults?.Invoke();
			LoadingIndicator.Hide();

		}

		private async Task UpdateProjectMeasurements() {

			LoadingIndicator.Show();

			foreach (Device d in SelectedProject.Devices) {
				foreach (MeasurementDefinition md in d.MeasurementDefinitions) {

					MeasurementResult latestResult = md.GetLatestMeasurementResult();

					MeasurementResultRange newResults =
						await RequestHandler.GetMeasurementResults(md, FetchAmount, latestResult.Timestamp, DateTime.Now);

					if (newResults == null || !newResults.SkipLast(1).Any()) {
						continue;
					}

					MeasurementResultRange allResults = new(newResults.SkipLast(1).Concat(md.GetLatestRange()));
					md.ReplaceMeasurementResultRange(0, allResults);

				}

			}

			HasUpdatedMeasurementResults?.Invoke();
			LoadingIndicator.Hide();

		}

		private async Task FilterByDate(TimeRange timeRange) {

			await UpdateProjectMeasurements();
			LoadingIndicator.Show();

			foreach (Device d in SelectedProject.Devices) {

				foreach (MeasurementDefinition md in d.MeasurementDefinitions) {

					DateTime clampedStartTime = timeRange.StartTime < md.FirstMeasurementResult.Timestamp
						? md.FirstMeasurementResult.Timestamp
						: timeRange.StartTime;

					DateTime clampedEndTime = timeRange.EndTime > md.LatestMeasurementResult.Timestamp
						? md.LatestMeasurementResult.Timestamp
						: timeRange.EndTime;

					TimeRange searchRange = new(clampedStartTime, clampedEndTime);
					TimeRange[] missingRanges = TimeRangeExtensions.GetTimeRangeGaps(searchRange, md.GetAvailableTimeRanges());

					if (missingRanges.Length < 1) {
						continue;
					}

					foreach (TimeRange t in missingRanges) {

						DateTime dynamicStartTime = t.StartTime;
						DateTime dynamicEndTime = t.EndTime;
						bool firstCheck = true;

						do {
							destroyCancellationToken.ThrowIfCancellationRequested();

							MeasurementResultRange results =
								await RequestHandler.GetMeasurementResults(md, FetchAmount, dynamicStartTime, dynamicEndTime);

							if (results.Count > 0 && results.Count < FetchAmount) {
								md.AddMeasurementResultRange(results, new TimeRange(dynamicStartTime, dynamicEndTime));
								dynamicEndTime = dynamicStartTime;
								continue;
							}

							if (firstCheck) {
								firstCheck = false;
							}
							else {
								dynamicEndTime = results.Count > 0 ? results.First().Timestamp : dynamicEndTime;
							}

							dynamicStartTime = results.Count > 0 ? results.Last().Timestamp : dynamicStartTime;

							md.AddMeasurementResultRange(results, new TimeRange(dynamicStartTime, dynamicEndTime));

							dynamicStartTime = t.StartTime;
							dynamicEndTime = results.Count > 0 ? results.Last().Timestamp - TimeSpan.FromSeconds(1)
								: dynamicStartTime;

						} while (dynamicEndTime > t.StartTime + TimeSpan.FromSeconds(1));

					}

					/*
					Debug.Log($"Result Ranges in {md.DeviceId} - {md.AttributeId} ({md.ID}):");
					foreach (var m in md.MeasurementResults) {
						Debug.Log(
							$"from {m.GetTimeRange().StartTime} to {m.GetTimeRange().EndTime} with {m.Count} results");
					}
					Debug.Log(" ----- ");
					*/

				}

			}

			//HasDateFiltered?.Invoke(timeRange);
			LoadingIndicator.Hide();

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

		private async void OnRefetchTimerElapsed() {
			await UpdateProjectMeasurements();
		}

		public async void OnRefetchButtonPressed() {
			await UpdateProjectMeasurements();
		}

		public void OnCooldownInputChanged(int milliseconds) {
			fetchInterval = milliseconds;
		}

		public void OnAmountInputChanged(int amount) {
			FetchAmount = amount;
		}

		public async void OnDateFilterPressed(TimeRange timeRange) {
			ShouldRefetch = false;
			await FilterByDate(timeRange);
		}

	}

}