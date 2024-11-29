using System;
using System.Linq;
using Dataskop.Data;
using Dataskop.Interaction;
using UnityEngine;

namespace Dataskop.Entities.Visualizations {

	public class RadialBar : MonoBehaviour, IVisualization {

		[Header("References")]
		[SerializeField] private GameObject visObjectPrefab;
		[SerializeField] private Transform visObjectsContainer;
		[SerializeField] private GameObject noResultsIndicator;
		[SerializeField] private RadialBarDataDisplay focusedDataDisplay;
		[Header("Vis Values")]
		[SerializeField] private Vector3 offset;
		[SerializeField] private float scaleFactor;
		[SerializeField] private Color32[] availableColors;

#pragma warning disable CS0067
		public event Action SwipedDown;

		public event Action SwipedUp;

		public event Action<int> VisObjectHovered;

		public event Action<int> VisObjectSelected;

		public event Action<int> VisObjectDeselected;

		public event Action<IVisObject> FocusedVisObjectChanged;

#pragma warning restore CS0067

		public DataPoint DataPoint { get; private set; }

		public bool HasHistoryEnabled { get; private set; }

		public IVisObject[] VisObjects { get; set; }

		public IVisObject FocusedVisObject => VisObjects[DataPoint.FocusedIndex];

		public VisualizationOption VisOption { get; set; }

		public VisHistoryConfiguration VisHistoryConfiguration { get; set; }

		public bool IsSelected { get; private set; }

		public Transform VisOrigin { get; set; }

		public MeasurementType[] AllowedMeasurementTypes { get; set; } = {
			MeasurementType.Float
		};

		public Vector3 Offset { get; private set; }

		public VisualizationType Type { get; set; }

		public MeasurementResult LatestResultBeforeUpdate { get; private set; }

		private float Scale { get; set; }

		private MeasurementResultRange[] CurrentRanges { get; set; }

		public void Initialize(DataPoint dp) {

			DataPoint = dp;
			VisOrigin = transform;
			Scale = scaleFactor;
			Offset = offset;
			VisHistoryConfiguration = new VisHistoryConfiguration(0, 0, false);
			Type = VisualizationType.RadialBar;
			VisOrigin.localScale *= Scale;
			VisOrigin.root.localPosition = Offset;
			HasHistoryEnabled = false;

			CurrentRanges = DataPoint.Device.MeasurementDefinitions
				.Select(md => md.MeasurementResults.First())
				.ToArray();

			if (CurrentRanges.Length == 0) {
				return;
			}

			if (!CurrentRanges.All(x => x.Count > 0)) {
				noResultsIndicator.SetActive(true);
				VisObjects = Array.Empty<IVisObject>();
				return;
			}

			noResultsIndicator.SetActive(false);

			// Only display the latest values, only one VisObject is needed.
			VisObjects = new IVisObject[1];

			GameObject visObject = Instantiate(
				visObjectPrefab, transform.position, Quaternion.identity, visObjectsContainer
			);

			VisObjects[DataPoint.FocusedIndex] = visObject.GetComponent<IVisObject>();
			VisObjects[DataPoint.FocusedIndex].HasHovered += OnVisObjectHovered;
			VisObjects[DataPoint.FocusedIndex].HasSelected += OnVisObjectSelected;
			VisObjects[DataPoint.FocusedIndex].HasDeselected += OnVisObjectDeselected;
			//VisObjects[DataPoint.FocusedIndex].VisCollider.enabled = true;

			OnFocusedIndexChanged(DataPoint.FocusedIndex);
		}

		public void OnTimeSeriesToggled(bool isActive) {
			// This visualization currently does not support Time Series at all.
			return;
		}

		public void OnFocusedIndexChanged(int index) {

			index = 0;
			MeasurementResult[] focusedResult = CurrentRanges.Select(mrr => mrr.First()).ToArray();

			UpdateVisObject(
				VisObjects[index], index, focusedResult, true,
				IsSelected ? VisObjectState.Selected : VisObjectState.Deselected
			);
		}

		public void OnSwipeInteraction(PointerInteraction pointerInteraction) {

			if (pointerInteraction.startingGameObject != null) {
				if (!VisObjects.Contains(pointerInteraction.startingGameObject.GetComponent<IVisObject>())) {
					return;
				}

			}

			switch (pointerInteraction.Direction.y) {
				case > 0.20f:
					focusedDataDisplay.OnSwipe(Vector2.up);
					break;
				case < -0.20f:
					focusedDataDisplay.OnSwipe(Vector2.down);
					break;
			}

		}

		public void OnMeasurementResultRangeUpdated() {
			ClearVisObjects();

			if (CurrentRanges.Length == 0) {
				focusedDataDisplay.Hide();
				return;
			}

			if (!CurrentRanges.All(x => x.Count > 0)) {
				noResultsIndicator.SetActive(true);
				VisObjects = Array.Empty<IVisObject>();
				focusedDataDisplay.Hide();
				return;
			}

			noResultsIndicator.SetActive(false);

			VisObjects = new IVisObject[1];

			GameObject visObject = Instantiate(
				visObjectPrefab, VisOrigin.position, visObjectsContainer.localRotation,
				visObjectsContainer
			);

			VisObjects[DataPoint.FocusedIndex] = visObject.GetComponent<IVisObject>();
			VisObjects[DataPoint.FocusedIndex].HasHovered += OnVisObjectHovered;
			VisObjects[DataPoint.FocusedIndex].HasSelected += OnVisObjectSelected;
			VisObjects[DataPoint.FocusedIndex].HasDeselected += OnVisObjectDeselected;
			VisObjects[DataPoint.FocusedIndex].VisCollider.enabled = true;

			UpdateVisObject(
				VisObjects[DataPoint.FocusedIndex], DataPoint.FocusedIndex,
				CurrentRanges.Select(mrr => mrr.First()).ToArray(),
				true,
				IsSelected ? VisObjectState.Selected : VisObjectState.Deselected
			);

			OnTimeSeriesToggled(true);
			focusedDataDisplay.Show();
		}

		public void OnMeasurementResultsUpdated(int newIndex) {
			return;
		}

		public void ApplyStyle(VisualizationStyle style) {
			return;
		}

		public void Despawn() {
			ClearVisObjects();
			DataPoint = null;
			Destroy(gameObject);
		}

		private void OnVisObjectHovered(int index) {

			/*

			index = 0;
			IVisObject visObject = VisObjects[index];

			if (index == DataPoint.FocusedIndex) {

				if (!IsSelected) {

					if (visObject == null) {
						return;
					}

					visObject.ChangeState(VisObjectState.Hovered);
					focusedDataDisplay.Hover(true);
				}

			}
			else {

				visObject.ChangeState(VisObjectState.Hovered);

			}

			VisObjectHovered?.Invoke(index);

			*/

		}

		private void OnVisObjectSelected(int index) {

			/*
			index = 0;
			IsSelected = true;

			if (index == DataPoint.FocusedIndex) {
				focusedDataDisplay.Select();
				focusedDataDisplay.SetDisplayData(VisObjects[index].CurrentData);
				VisObjects[index].ChangeState(VisObjectState.Selected);
			}

			VisObjectSelected?.Invoke(index);
			*/

		}

		private void OnVisObjectDeselected(int index) {

			/*
			index = 0;

			if (index == DataPoint?.FocusedIndex) {

				if (VisObjects[index] == null) {
					return;
				}

				if (IsSelected) {
					IsSelected = false;
				}

				VisObjects[index].ChangeState(VisObjectState.Deselected);
				focusedDataDisplay.Deselect(true);

			}

			VisObjectDeselected?.Invoke(index);
			*/

		}

		private void UpdateVisObject(IVisObject target, int index, MeasurementResult[] results, bool focused,
			VisObjectState state) {

			if (target == null) {
				return;
			}

			target.Index = index;
			target.SetFocus(focused);

			VisObjectData[] dataSet = new VisObjectData[results.Length];

			for (int i = 0; i < results.Length; i++) {
				dataSet[i].Result = results[i];
				dataSet[i].Type = MeasurementType.Float;
				dataSet[i].Attribute =
					DataPoint.Device.Attributes.First(x => x.ID == results[i].MeasurementDefinition.AttributeId);

				dataSet[i].AuthorSprite = null;
				dataSet[i].Color = availableColors[i];
			}

			target.ApplyData(dataSet);
			target.ChangeState(state);
			focusedDataDisplay.SetDisplayData(dataSet);

		}

		private void ClearVisObjects() {

			for (int i = 0; i < VisObjects.Length; i++) {

				if (VisObjects[i] == null) {
					continue;
				}

				VisObjects[i].HasHovered -= OnVisObjectHovered;
				VisObjects[i].HasSelected -= OnVisObjectSelected;
				VisObjects[i].HasDeselected -= OnVisObjectDeselected;
				VisObjects[i].Delete();
				VisObjects[i] = null;

			}

		}

	}

}
