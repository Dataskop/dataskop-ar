using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace Dataskop.Data {

	public class ApiRequestHandler {

		private const string BACKEND_URL = "https://backend.dataskop.at/api";

		public async Task<IReadOnlyCollection<Company>> GetCompanies() {

			string url = $"{BACKEND_URL}/company/list";
			string rawResponse = await GetResponse(url);

			try {
				List<Company> companies = JsonConvert.DeserializeObject<List<Company>>(rawResponse);
				return companies;
			}
			catch (Exception e) {
				Debug.LogError(e.Message);
				return null;
			}

		}

		public async Task<IReadOnlyCollection<Project>> GetProjects(Company company) {

			string url = $"{BACKEND_URL}/company/projects/{company.ID}";
			string rawResponse = await GetResponse(url);

			try {
				List<Project> projects = JsonConvert.DeserializeObject<List<Project>>(rawResponse);
				return projects;
			}
			catch (Exception e) {
				Debug.LogError(e.Message);
				return null;
			}

		}

		public async Task<IReadOnlyCollection<Device>> GetDevices(Project project) {

			string url = $"{BACKEND_URL}/project/measurementdefinitions/{project.ID}";
			string rawResponse = await GetResponse(url);

			try {
				ICollection<MeasurementDefinition> projectMeasurementDefinitions =
					JsonConvert.DeserializeObject<ICollection<MeasurementDefinition>>(rawResponse);

				List<Device> devices = new();

				foreach (MeasurementDefinition measurementDefinition in projectMeasurementDefinitions) {

					Device foundDevice = devices.FirstOrDefault(
						device => measurementDefinition.DeviceId != null && measurementDefinition.DeviceId == device.ID
					);

					if (foundDevice == null) {

						int deviceId = devices.Count;
						Device newDevice = new(deviceId.ToString(), $"Device_{deviceId:000}", new List<MeasurementDefinition> {
							measurementDefinition
						});

						devices.Add(newDevice);
					}
					else {
						foundDevice.MeasurementDefinitions.Add(measurementDefinition);
					}

				}
				return devices;
			}
			catch {

				NotificationHandler.Add(new Notification {
					Category = NotificationCategory.Error,
					Text = $"Could not fetch MeasurementDefinitions of Project {project.ID}",
					DisplayDuration = NotificationDuration.Medium
				});

				return null;

			}

		}

		/// <summary>
		///     Fetches a list of measurement results belonging to the measurement definition.
		/// </summary>
		public async Task<IReadOnlyCollection<MeasurementResult>> GetMeasurementResults(MeasurementDefinition measurementDefinition,
			int amount) {

			string url = $"{BACKEND_URL}/measurementresult/query/advanced/{measurementDefinition.ID}/{amount}/0?orderby=timestamp%20desc";
			string response = await GetResponse(url);

			try {
				MeasurementResultsResponse resultsResponse = JsonConvert.DeserializeObject<MeasurementResultsResponse>(response);
				int totalCount = resultsResponse.Count;
				List<MeasurementResult> measurementResults = resultsResponse?.MeasurementResults.ToList();

				if (amount > totalCount) {
					NotificationHandler.Add(new Notification {
						Category = NotificationCategory.Warning,
						Text = $"Tried to fetch {amount} for {measurementDefinition.ID}, but could only find {totalCount}!",
						DisplayDuration = NotificationDuration.Short
					});
				}

				return measurementResults;
			}
			catch {
				NotificationHandler.Add(new Notification {
					Category = NotificationCategory.Error,
					Text = $"Could not fetch Measurement Results for Definition {measurementDefinition.ID}!",
					DisplayDuration = NotificationDuration.Medium
				});
				return null;
			}

		}

		private async Task<string> GetResponse(string url) {

			if (Application.internetReachability == NetworkReachability.NotReachable) {
				HandleClientOffline();
				return "";
			}

			using UnityWebRequest request = UnityWebRequest.Get(url);
			request.SetRequestHeader("Content-Type", "application/json");
			request.SetRequestHeader("Authorization", UserData.Instance.Token!);
			UnityWebRequestAsyncOperation operation = request.SendWebRequest();

			while (!operation.isDone) {
				await Task.Yield();
			}

			if (request.result == UnityWebRequest.Result.Success) {
				return request.downloadHandler.text;
			}

			HandleWebRequestErrors(request);
			return "This did not work!";

		}

		/// <summary>
		///     Handles the case when the client is offline.
		/// </summary>
		private void HandleClientOffline() {
			NotificationHandler.Add(new Notification {
				Category = NotificationCategory.Error,
				Text = "You are offline. Make sure you are connected to the internet and try again.",
				DisplayDuration = NotificationDuration.Medium
			});
		}

		/// <summary>
		///     Handles UnityWebRequest errors.
		/// </summary>
		private void HandleWebRequestErrors(UnityWebRequest request) {

			if (request.result == UnityWebRequest.Result.ConnectionError) {
				NotificationHandler.Add(new Notification {
					Category = NotificationCategory.Error,
					Text = "No connection to the server.",
					DisplayDuration = NotificationDuration.Medium
				});
				return;
			}

			if (request.result == UnityWebRequest.Result.ProtocolError) {
				NotificationHandler.Add(new Notification {
					Category = NotificationCategory.Error,
					Text = request.responseCode == 401 ? "Your access token is invalid." : "No data available.",
					DisplayDuration = NotificationDuration.Medium
				});
				return;
			}

			NotificationHandler.Add(new Notification {
				Category = NotificationCategory.Error,
				Text = "An unexpected error occurred.",
				DisplayDuration = NotificationDuration.Medium
			});

		}

	}

}