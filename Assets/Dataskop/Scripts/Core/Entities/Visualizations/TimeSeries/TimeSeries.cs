using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Dataskop.Data;
using UnityEngine;

namespace Dataskop.Entities.Visualizations {

	public class TimeSeries : MonoBehaviour {

		private Coroutine spawnRoutine;

		protected List<TimeElement> TimeElements { get; private set; }

		public TimeSeriesConfig Configuration { get; private set; }

		public DataPoint DataPoint { get; protected set; }

		private List<MeasurementResult> MeasurementResults { get; set; }

		private Vector3 VisOrigin { get; set; }

		private int SwipeCount { get; set; }

		private int ResultsCount => MeasurementResults?.Count ?? 0;

		public bool IsSpawned { get; private set; }

		public void Start() {
			TimeElements = new List<TimeElement>();
		}

		public event Action TimeSeriesBeforeSpawn;

		public event Action TimeSeriesSpawned;

		public event Action<TimeElement> TimeElementSpawned;

		public event Action TimeSeriesDespawned;

		public event Action<TimeElement> TimeElementMoved;

		public event Action TimeSeriesStartMoved;

		public void Spawn(TimeSeriesConfig config, DataPoint dp, Transform container) {

			if (spawnRoutine != null) {
				StopCoroutine(spawnRoutine);
				DespawnSeries();
				spawnRoutine = null;
			}

			spawnRoutine = StartCoroutine(SpawnSeries(config, dp, container));

		}

		private IEnumerator SpawnSeries(TimeSeriesConfig config, DataPoint dp, Transform container) {

			DataPoint = dp;
			Configuration = config;

			if (DataPoint.MeasurementDefinition?.MeasurementResults == null)
				yield break;

			TimeSeriesBeforeSpawn?.Invoke();
			IsSpawned = true;
			DataPoint.CurrentMeasurementResult = DataPoint.MeasurementDefinition.GetLatestMeasurementResult();
			MeasurementResults = DataPoint.MeasurementDefinition.MeasurementResults.ToList();
			SwipeCount = 0;

			Transform visTransform = dp.Vis.VisTransform;
			Vector3 visPosition = visTransform.position;
			VisOrigin = visPosition;

			for (int i = 0; i < ResultsCount - 1; i++) {

				Vector3 elementPos = new(visPosition.x, visPosition.y + config.elementDistance * (i + 1), visPosition.z);
				GameObject newElement = Instantiate(Configuration.elementVis, elementPos, visTransform.rotation);
				newElement.transform.SetParent(container);

				TimeElement timeElement = newElement.GetComponent<TimeElement>();

				timeElement.Series = this;
				timeElement.NextTargetPosition = timeElement.transform.position;
				timeElement.DistanceToDataPoint = i + 1;
				timeElement.MeasurementResult = MeasurementResults?[timeElement.DistanceToDataPoint];
				timeElement.SetDisplayData();
				timeElement.gameObject.SetActive(ShouldDrawTimeElement(Configuration.visibleHistoryCount, timeElement));
				TimeElements.Add(timeElement);
				TimeElementSpawned?.Invoke(timeElement);
				yield return new WaitForEndOfFrame();

			}

			DataPoint.Vis.SwipedUp += OnSwipedUp;
			DataPoint.Vis.SwipedDown += OnSwipedDown;
			TimeSeriesSpawned?.Invoke();

		}

		public void DespawnSeries() {

			if (!IsSpawned) {
				return;
			}

			if (TimeElements == null) {
				return;
			}

			if (spawnRoutine != null) {
				StopCoroutine(spawnRoutine);
				spawnRoutine = null;
			}

			foreach (TimeElement e in TimeElements) {
				Destroy(e.gameObject);
			}

			TimeElements.Clear();
			DataPoint.Vis.SwipedUp -= OnSwipedUp;
			DataPoint.Vis.SwipedDown -= OnSwipedDown;
			DataPoint.CurrentMeasurementResult = DataPoint.MeasurementDefinition.GetLatestMeasurementResult();
			TimeSeriesDespawned?.Invoke();
			IsSpawned = false;

		}

		private void OnSwipedUp() {

			if (SwipeCount == 0)
				return;

			for (int index = TimeElements.Count - 1; index >= 0; index--) {
				StartCoroutine(MoveTimeElement(index, Vector3.up, Configuration.animationDuration));
			}

			SwipeCount--;

		}

		private void OnSwipedDown() {

			if (SwipeCount == ResultsCount - 1)
				return;

			for (int index = 0; index < TimeElements.Count; index++) {
				StartCoroutine(MoveTimeElement(index, Vector3.down, Configuration.animationDuration));
			}

			SwipeCount++;

		}

		protected bool ShouldDrawTimeElement(int amountPerDirection, TimeElement timeElement) {

			if (DataPoint.MeasurementDefinition.MeasurementResults == null) return false;

			MeasurementResult currentMr = DataPoint.CurrentMeasurementResult;

			int indexOfCurrent = MeasurementResults.IndexOf(currentMr);
			int indexOfElement = MeasurementResults.IndexOf(timeElement.MeasurementResult);
			timeElement.DistanceToDataPoint = Mathf.Abs(indexOfCurrent - indexOfElement);

			return timeElement.DistanceToDataPoint <= amountPerDirection;

		}

		private IEnumerator MoveTimeElement(int index, Vector3 direction, float moveDuration) {

			TimeElement e = TimeElements[index];

			Vector3 startPosition = e.transform.position;

			if (Vector3.Distance(startPosition, e.NextTargetPosition) > 0.0005f) {
				startPosition = e.NextTargetPosition;
			}

			Vector3 targetPosition = startPosition + Configuration.elementDistance * direction;

			// Check if TimeElement moves to lower or upper part
			if (Vector3.Distance(targetPosition, VisOrigin) < 0.05f) {

				targetPosition = startPosition + 2 * Configuration.elementDistance * direction;

				switch (direction.y) {
					case < 0:
						e.MeasurementResult =
							MeasurementResults[MeasurementResults.IndexOf(DataPoint.CurrentMeasurementResult) - 1];
						e.DistanceToDataPoint -= 2;
						break;
					case > 0:
						e.MeasurementResult =
							MeasurementResults[MeasurementResults.IndexOf(DataPoint.CurrentMeasurementResult) + 1];
						e.DistanceToDataPoint += 2;
						break;
				}

			}
			else {
				switch (direction.y) {
					case < 0:
						e.DistanceToDataPoint--;
						break;
					case > 0:
						e.DistanceToDataPoint++;
						break;
				}
			}

			e.NextTargetPosition = targetPosition;
			TimeSeriesStartMoved?.Invoke();

			float t = 0;
			while (t < moveDuration) {

				if (e == null) {
					yield break;
				}

				e.transform.position = Vector3.Lerp(startPosition, targetPosition, t / moveDuration);

				t += Time.deltaTime;
				yield return null;

			}

			e.transform.position = targetPosition;
			e.gameObject.SetActive(ShouldDrawTimeElement(Configuration.visibleHistoryCount, e));
			TimeElementMoved?.Invoke(e);

		}

		public void OnMeasurementResultsUpdated(MeasurementResult[] results) {

			MeasurementResults = results.ToList();

			for (int i = 0; i <= TimeElements.Count - 1; i++) {
				TimeElements[i].MeasurementResult = results[i];
			}

		}

	}

}