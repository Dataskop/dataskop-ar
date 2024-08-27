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
			catch {
				NotificationHandler.Add(new Notification {
					Category = NotificationCategory.Error,
					Text = "Could not fetch companies!",
					DisplayDuration = NotificationDuration.Medium
				});

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
			catch {

				NotificationHandler.Add(new Notification {
					Category = NotificationCategory.Error,
					Text = $"Could not fetch measurement definitions of company: {company.ID}",
					DisplayDuration = NotificationDuration.Medium
				});

				return null;
			}

		}

		public async Task<IReadOnlyCollection<Device>> GetDevices(Project project) {

			string url = $"{BACKEND_URL}/project/measurementdefinitions/{project.ID}";
			string rawResponse = await GetResponse(url);

			try {
				IReadOnlyCollection<MeasurementDefinition> projectMeasurementDefinitions =
					JsonConvert.DeserializeObject<IReadOnlyCollection<MeasurementDefinition>>(rawResponse);

				List<Device> devices = new();

				foreach (MeasurementDefinition measurementDefinition in projectMeasurementDefinitions) {

					Device foundDevice = devices.FirstOrDefault(
						device => measurementDefinition.DeviceId != null && measurementDefinition.DeviceId == device.ID
					);

					if (foundDevice == null) {

						string deviceId = measurementDefinition.DeviceId;
						Device newDevice = new(deviceId, $"Device_{deviceId}", new List<MeasurementDefinition> {
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
					Text = $"Could not fetch measurement definitions of project: {project.ID}",
					DisplayDuration = NotificationDuration.Medium
				});

				return null;

			}

		}

		public async Task<MeasurementResultRange> GetMeasurementResults(MeasurementDefinition measurementDefinition,
			int amount, DateTime? from, DateTime? to) {

			string url =
				$"{BACKEND_URL}/measurementresult/query/advanced/{measurementDefinition.ID}/{amount}/0?orderby=timestamp%20desc";

			if (from != null && to != null) {
				url += $"&startTime={from.Value:s}&endTime={to.Value:s}";
			}

			string response = await GetResponse(url);

			try {
				MeasurementResultsResponse resultsResponse = JsonConvert.DeserializeObject<MeasurementResultsResponse>(response);
				MeasurementResultRange measurementResults = resultsResponse?.MeasurementResults;

				foreach (MeasurementResult mr in measurementResults!) {
					mr.MeasurementDefinition = measurementDefinition;
				}

				return measurementResults;

			}
			catch {
				NotificationHandler.Add(new Notification {
					Category = NotificationCategory.Error,
					Text = $"Could not fetch measurement results for definition: {measurementDefinition.ID}!",
					DisplayDuration = NotificationDuration.Medium
				});
				return null;
			}

		}

		public async Task<MeasurementResult> GetFirstMeasurementResult(MeasurementDefinition measurementDefinition) {

			string url = $"{BACKEND_URL}/measurementresult/query/{measurementDefinition.ID}/1/0";
			string response = await GetResponse(url);

			try {
				MeasurementResultsResponse resultsResponse = JsonConvert.DeserializeObject<MeasurementResultsResponse>(response);
				MeasurementResult measurementResult = resultsResponse?.MeasurementResults.FirstOrDefault();
				return measurementResult;
			}
			catch {
				NotificationHandler.Add(new Notification {
					Category = NotificationCategory.Error,
					Text = $"Could not fetch measurement results for definition: {measurementDefinition.ID}!",
					DisplayDuration = NotificationDuration.Medium
				});
				return null;
			}

		}

		public async Task<int?> GetCount(MeasurementDefinition measurementDefinition) {

			string url = $"{BACKEND_URL}/measurementresult/query/{measurementDefinition.ID}/1/0";
			string response = await GetResponse(url);

			try {
				MeasurementResultsResponse resultsResponse = JsonConvert.DeserializeObject<MeasurementResultsResponse>(response);
				return resultsResponse.Count;
			}
			catch {
				NotificationHandler.Add(new Notification {
					Category = NotificationCategory.Error,
					Text = $"Could not fetch measurement results for definition: {measurementDefinition.ID}!",
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

		private void HandleClientOffline() {
			NotificationHandler.Add(new Notification {
				Category = NotificationCategory.Error,
				Text = "You are offline. Make sure you are connected to the internet and try again.",
				DisplayDuration = NotificationDuration.Medium
			});
		}

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