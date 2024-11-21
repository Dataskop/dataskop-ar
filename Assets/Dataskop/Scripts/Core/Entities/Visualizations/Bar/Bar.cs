using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Dataskop.Data;
using Dataskop.Interaction;
using UnityEngine;

namespace Dataskop.Entities.Visualizations {

	public class Bar : MonoBehaviour, IVisualization {

		[Header("References")]
		[SerializeField] private GameObject visObjectPrefab;
		[SerializeField] private Transform visObjectsContainer;
		[SerializeField] private BarVisObjectStyle visObjectStyle;
		[SerializeField] private GameObject dataGapIndicatorPrefab;
		[SerializeField] private GameObject noResultsIndicator;
		[SerializeField] private BarDataDisplay focusedDataDisplay;
		[SerializeField] private BarDataDisplay hoverDataDisplay;

		[Header("Vis Values")]
		[SerializeField] private Vector3 offset;
		[SerializeField] private float scaleFactor;
		[SerializeField] private VisHistoryConfiguration visHistoryConfig;
		private readonly List<GameObject> dataGapIndicators = new();

		private Coroutine historyMove;
		private bool isRotated;
		private Vector3 moveTarget = Vector3.zero;
		private Vector3 origin;

		private float Scale { get; set; }

		private int PreviousIndex { get; set; }

		private IVisObjectStyle VisObjectStyle { get; set; }

		private MeasurementResultRange CurrentRange => DataPoint.CurrentMeasurementRange;

		public event Action SwipedDown;

		public event Action SwipedUp;

		public event Action<int> VisObjectHovered;

		public event Action<int> VisObjectSelected;

		public event Action<int> VisObjectDeselected;

		public event Action<IVisObject> FocusedVisObjectChanged;

		public IVisObject[] VisObjects { get; set; }

		public IVisObject FocusedVisObject => VisObjects[DataPoint.FocusedIndex];

		public DataPoint DataPoint { get; private set; }

		public VisualizationOption VisOption { get; set; }

		public VisHistoryConfiguration VisHistoryConfiguration { get; set; }

		public bool IsSelected { get; private set; }

		public bool HasHistoryEnabled { get; private set; }

		public Transform VisOrigin { get; set; }

		public MeasurementType[] AllowedMeasurementTypes { get; set; } = {
			MeasurementType.Float, MeasurementType.Bool
		};

		public Vector3 Offset { get; private set; }

		public VisualizationType Type { get; set; }

		public void Initialize(DataPoint dp) {

			DataPoint = dp;
			VisOrigin = transform;
			Scale = scaleFactor;
			Offset = offset;
			VisHistoryConfiguration = visHistoryConfig;
			VisObjectStyle = visObjectStyle;
			Type = VisualizationType.Bar;
			VisOrigin.localScale *= Scale;
			VisOrigin.root.localPosition = Offset;
			PreviousIndex = DataPoint.FocusedIndex;

			if (CurrentRange.Count < 1) {
				noResultsIndicator.SetActive(true);
				VisObjects = Array.Empty<IVisObject>();
				focusedDataDisplay.Hide();
				hoverDataDisplay.Hide();
				return;
			}

			noResultsIndicator.SetActive(false);

			VisObjects = new IVisObject[CurrentRange.Count];

			GameObject visObject = Instantiate(
				visObjectPrefab, transform.position, Quaternion.identity, visObjectsContainer
			);

			VisObjects[DataPoint.FocusedIndex] = visObject.GetComponent<IVisObject>();
			VisObjects[DataPoint.FocusedIndex].HasHovered += OnVisObjectHovered;
			VisObjects[DataPoint.FocusedIndex].HasSelected += OnVisObjectSelected;
			VisObjects[DataPoint.FocusedIndex].HasDeselected += OnVisObjectDeselected;
			VisObjects[DataPoint.FocusedIndex].VisCollider.enabled = true;

			OnFocusedIndexChanged(DataPoint.FocusedIndex);

		}

		public void OnFocusedIndexChanged(int index) {

			if (!AllowedMeasurementTypes.Contains(DataPoint.MeasurementDefinition.MeasurementType)) {
				NotificationHandler.Add(
					new Notification {
						Category = NotificationCategory.Error,
						Text = $"Value Type not supported by {Type} visualization.",
						DisplayDuration = 5f
					}
				);

				return;
			}

			if (!HasHistoryEnabled) {

				MeasurementResult focusedResult = CurrentRange[index];

				if (DataPoint.FocusedIndex != PreviousIndex) {
					VisObjects[DataPoint.FocusedIndex] = VisObjects[PreviousIndex];
					VisObjects[PreviousIndex] = null;
					PreviousIndex = DataPoint.FocusedIndex;
				}

				UpdateVisObject(
					VisObjects[DataPoint.FocusedIndex], DataPoint.FocusedIndex, focusedResult, true,
					IsSelected ? VisObjectState.Selected : VisObjectState.Deselected
				);

			}
			else {

				int objectCountDistance = Mathf.Abs(PreviousIndex - DataPoint.FocusedIndex);

				if (historyMove != null) {
					StopCoroutine(historyMove);
					visObjectsContainer.transform.position = moveTarget;
				}

				historyMove = StartCoroutine(
					MoveHistory(
						PreviousIndex < DataPoint.FocusedIndex ? Vector3.down : Vector3.up,
						objectCountDistance
					)
				);

				// VisObjects above current result
				for (int i = 1; i < VisObjects.Length - DataPoint.FocusedIndex; i++) {
					int targetIndex = DataPoint.FocusedIndex + i;
					MeasurementResult newResultToAssign = CurrentRange[targetIndex];
					IVisObject targetObject = VisObjects[targetIndex];
					UpdateVisObject(targetObject, targetIndex, newResultToAssign, false, VisObjectState.Deselected);
				}

				// VisObjects below current result
				for (int i = 1; i <= DataPoint.FocusedIndex; i++) {
					int targetIndex = DataPoint.FocusedIndex - i;
					MeasurementResult newResultToAssign = CurrentRange[targetIndex];
					IVisObject targetObject = VisObjects[targetIndex];
					UpdateVisObject(targetObject, targetIndex, newResultToAssign, false, VisObjectState.Deselected);
				}

				MeasurementResult focusedResult = CurrentRange[DataPoint.FocusedIndex];
				UpdateVisObject(
					VisObjects[DataPoint.FocusedIndex], DataPoint.FocusedIndex, focusedResult, true,
					IsSelected ? VisObjectState.Selected : VisObjectState.Deselected
				);

				focusedDataDisplay.MoveTo(VisObjects[DataPoint.FocusedIndex].VisObjectTransform.position);
				PreviousIndex = DataPoint.FocusedIndex;

			}

			FocusedVisObjectChanged?.Invoke(FocusedVisObject);

		}

		public void OnTimeSeriesToggled(bool isActive) {

			if (CurrentRange.Count < 1) {
				HasHistoryEnabled = isActive;
				return;
			}

			if (isActive) {

				MeasurementResultRange currentResults = DataPoint.CurrentMeasurementRange;
				float distance = visHistoryConfig.elementDistance;

				// VisObjects above current result
				for (int i = 1; i < VisObjects.Length - DataPoint.FocusedIndex; i++) {

					int targetIndex = DataPoint.FocusedIndex + i;
					MeasurementResult result = currentResults[targetIndex];

					if (result == null) {
						continue;
					}

					Vector3 spawnPos = new(
						VisOrigin.position.x, VisOrigin.position.y + distance * i, VisOrigin.position.z
					);

					VisObjects[targetIndex] = SpawnVisObject(
						targetIndex, spawnPos, result, false, VisObjectState.Deselected
					);

					VisObjects[targetIndex].OnHistoryToggle(true);

					MeasurementResult res2 = currentResults[targetIndex - 1];

					if (res2 == null) {
						continue;
					}

					if (!DataPoint.MeasurementDefinition.IsDataGap(result, res2)) {
						continue;
					}

					float yPos = (spawnPos.y - VisObjects[targetIndex - 1].VisObjectTransform.position.y) / 2f;
					GameObject indicator = Instantiate(
						dataGapIndicatorPrefab, new Vector3(spawnPos.x, spawnPos.y - yPos, spawnPos.z),
						visObjectsContainer.localRotation,
						visObjectsContainer
					);

					dataGapIndicators.Add(indicator);

				}

				// VisObjects below current result
				for (int i = 1; i <= DataPoint.FocusedIndex; i++) {

					int targetIndex = DataPoint.FocusedIndex - i;
					MeasurementResult result = currentResults[targetIndex];

					if (result == null) {
						continue;
					}

					Vector3 spawnPos = new(
						VisOrigin.position.x, VisOrigin.position.y - distance * i, VisOrigin.position.z
					);

					VisObjects[targetIndex] = SpawnVisObject(
						targetIndex, spawnPos, result, false, VisObjectState.Deselected
					);

					VisObjects[targetIndex].OnHistoryToggle(true);

					MeasurementResult res2 = currentResults[targetIndex + 1];

					if (res2 == null) {
						continue;
					}

					if (!DataPoint.MeasurementDefinition.IsDataGap(result, res2)) {
						continue;
					}

					float yPos = (spawnPos.y - VisObjects[targetIndex + 1].VisObjectTransform.position.y) / 2f;
					GameObject indicator = Instantiate(
						dataGapIndicatorPrefab, new Vector3(spawnPos.x, spawnPos.y + yPos, spawnPos.z),
						visObjectsContainer.localRotation,
						visObjectsContainer
					);

					dataGapIndicators.Add(indicator);

				}

				BarVisObject barVis = (BarVisObject)VisObjects[DataPoint.FocusedIndex];
				focusedDataDisplay.Rotate(true, barVis.GetCurrentScale().y, barVis.GetCurrentScale().x);
				hoverDataDisplay.Rotate(true, barVis.GetCurrentScale().y, barVis.GetCurrentScale().x);
				VisObjects[DataPoint.FocusedIndex].OnHistoryToggle(true);

			}
			else {

				if (!HasHistoryEnabled) {
					return;
				}

				ClearHistoryVisObjects();
				BarVisObject barVis = (BarVisObject)VisObjects[DataPoint.FocusedIndex];
				focusedDataDisplay.Rotate(false, barVis.GetCurrentScale().y, barVis.GetCurrentScale().x);
				hoverDataDisplay.Rotate(false, barVis.GetCurrentScale().y, barVis.GetCurrentScale().x);
				VisObjects[DataPoint.FocusedIndex].OnHistoryToggle(false);
				hoverDataDisplay.Hide();

			}

			HasHistoryEnabled = isActive;

		}

		public void OnSwipeInteraction(PointerInteraction pointerInteraction) {

			switch (pointerInteraction.Direction.y) {
				case > 0.20f:
					SwipedUp?.Invoke();
					break;
				case < -0.20f:
					SwipedDown?.Invoke();
					break;
			}

		}

		public void OnMeasurementResultRangeUpdated() {

			ClearVisObjects();

			if (CurrentRange.Count < 1) {
				visObjectsContainer.localPosition = Vector3.zero;
				noResultsIndicator.SetActive(true);
				focusedDataDisplay.Hide();
				hoverDataDisplay.Hide();
				return;
			}

			noResultsIndicator.SetActive(false);
			PreviousIndex = DataPoint.FocusedIndex;

			VisObjects = new IVisObject[CurrentRange.Count];

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
				VisObjects[DataPoint.FocusedIndex], DataPoint.FocusedIndex, CurrentRange[DataPoint.FocusedIndex], true,
				IsSelected ? VisObjectState.Selected : VisObjectState.Deselected
			);

			OnTimeSeriesToggled(true);
			focusedDataDisplay.Show();

		}

		public void OnMeasurementResultsUpdated(int newIndex) {
			OnFocusedIndexChanged(newIndex);
		}

		public void ApplyStyle(VisualizationStyle style) {
			// Intentional empty body
		}

		public void Despawn() {
			ClearVisObjects();
			DataPoint = null;
			Destroy(gameObject);
		}

		private void OnVisObjectHovered(int index) {

			IVisObject visObject = VisObjects[index];

			if (index == DataPoint.FocusedIndex) {

				hoverDataDisplay.Hide();

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
				hoverDataDisplay.Show();
				hoverDataDisplay.SetDisplayData(VisObjects[index].CurrentData);
				hoverDataDisplay.MoveTo(VisObjects[index].VisObjectTransform.position);
				hoverDataDisplay.Hover(false);
			}

			VisObjectHovered?.Invoke(index);

		}

		private void OnVisObjectSelected(int index) {

			IsSelected = true;
			hoverDataDisplay.Hide();

			if (index == DataPoint.FocusedIndex) {
				focusedDataDisplay.Select();
				focusedDataDisplay.SetDisplayData(VisObjects[index].CurrentData);
				VisObjects[index].ChangeState(VisObjectState.Selected);
			}

			VisObjectSelected?.Invoke(index);

		}

		private void OnVisObjectDeselected(int index) {

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
			else {
				hoverDataDisplay.Hide();
			}

			VisObjectDeselected?.Invoke(index);

		}

		private IVisObject SpawnVisObject(int index, Vector3 pos, MeasurementResult result,
			bool focused, VisObjectState state) {

			GameObject newVis = Instantiate(
				visObjectPrefab, pos, visObjectsContainer.localRotation, visObjectsContainer
			);

			IVisObject visObject = newVis.GetComponent<IVisObject>();

			UpdateVisObject(visObject, index, result, focused, state);
			visObject.HasHovered += OnVisObjectHovered;
			visObject.HasSelected += OnVisObjectSelected;
			visObject.HasDeselected += OnVisObjectDeselected;
			visObject.VisCollider.enabled = true;

			return visObject;

		}

		private void UpdateVisObject(IVisObject target, int index, MeasurementResult result, bool focused,
			VisObjectState state) {

			if (target == null) {
				return;
			}

			target.Index = index;
			target.SetFocus(focused);

			VisObjectData data = new() {
				Result = result,
				Type = result.MeasurementDefinition.MeasurementType,
				Attribute = DataPoint.Attribute,
				AuthorSprite = result.Author != string.Empty
					? DataPoint.AuthorRepository.AuthorSprites[result.Author]
					: null
			};

			if (target.CurrentData.Result != result) {
				target.ApplyData(data);
			}

			target.ChangeState(state);

			if (target.IsFocused) {

				focusedDataDisplay.SetDisplayData(data);

				if (IsSelected) {
					focusedDataDisplay.Select();
				}
				else {
					focusedDataDisplay.Deselect(true);
				}

			}

		}

		private void ClearHistoryVisObjects() {

			if (!HasHistoryEnabled) {
				return;
			}

			if (historyMove != null) {
				StopCoroutine(historyMove);
			}

			for (int i = 0; i < VisObjects.Length; i++) {

				if (i == DataPoint.FocusedIndex) {
					continue;
				}

				if (VisObjects[i] == null) {
					continue;
				}

				VisObjects[i].HasHovered -= OnVisObjectHovered;
				VisObjects[i].HasSelected -= OnVisObjectSelected;
				VisObjects[i].HasDeselected -= OnVisObjectDeselected;
				VisObjects[i].Delete();
				VisObjects[i] = null;
				dataGapIndicators.ForEach(Destroy);
				dataGapIndicators.Clear();

			}

		}

		private void ClearVisObjects() {

			if (historyMove != null) {
				StopCoroutine(historyMove);
			}

			for (int i = 0; i < VisObjects.Length; i++) {

				if (VisObjects[i] == null) {
					continue;
				}

				VisObjects[i].HasHovered -= OnVisObjectHovered;
				VisObjects[i].HasSelected -= OnVisObjectSelected;
				VisObjects[i].HasDeselected -= OnVisObjectDeselected;
				VisObjects[i].Delete();
				VisObjects[i] = null;
				dataGapIndicators.ForEach(Destroy);
				dataGapIndicators.Clear();

			}

		}

		private IEnumerator MoveHistory(Vector3 direction, int multiplier = 1) {

			Vector3 startPosition = visObjectsContainer.position;
			moveTarget = visObjectsContainer.position + direction * (visHistoryConfig.elementDistance * multiplier);
			float moveDuration = visHistoryConfig.animationDuration;

			focusedDataDisplay.Hide();
			hoverDataDisplay.Hide();

			float t = 0;

			while (t < moveDuration) {
				visObjectsContainer.position = Vector3.Lerp(startPosition, moveTarget, t / moveDuration);
				t += Time.deltaTime;
				yield return null;
			}

			focusedDataDisplay.Show();
			visObjectsContainer.position = moveTarget;
			historyMove = null;

		}

	}

}