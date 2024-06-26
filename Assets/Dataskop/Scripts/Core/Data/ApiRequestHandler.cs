﻿using System;
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
						devices.Add(new Device(
							measurementDefinition.DeviceId,
							measurementDefinition.DeviceId,
							new List<MeasurementDefinition> {
								measurementDefinition
							}
						));
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

			//TODO: Change to Advanced Query

			string countURL = $"{BACKEND_URL}/measurementresult/query/{measurementDefinition.ID}/1/0";
			string countResponse = await GetResponse(countURL);
			int totalCount;

			try {
				MeasurementResultsResponse response = JsonConvert.DeserializeObject<MeasurementResultsResponse>(countResponse);
				totalCount = response.Count;
			}
			catch {
				NotificationHandler.Add(new Notification {
					Category = NotificationCategory.Error,
					Text = $"Could not fetch Measurement Results for Definition {measurementDefinition.ID}!",
					DisplayDuration = NotificationDuration.Medium
				});
				throw;
			}

			if (amount > totalCount) {

				amount = totalCount;

				NotificationHandler.AddUnique(new Notification {
					Category = NotificationCategory.Warning,
					Text = $"Amount fetched too high. Clamping to {totalCount}!",
					DisplayDuration = NotificationDuration.Medium,
					UniqueID = 2
				});

			}

			string url = $"{BACKEND_URL}/measurementresult/query/{measurementDefinition.ID}/{amount}/{totalCount - amount}";
			string rawResponse = await GetResponse(url);

			try {
				MeasurementResultsResponse response = JsonConvert.DeserializeObject<MeasurementResultsResponse>(rawResponse);
				List<MeasurementResult> measurementResults = response?.MeasurementResults.OrderByDescending(x => x.Timestamp).ToList();
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

		/// <summary>
		///     Performs a GET request to a given API endpoint.
		/// </summary>
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