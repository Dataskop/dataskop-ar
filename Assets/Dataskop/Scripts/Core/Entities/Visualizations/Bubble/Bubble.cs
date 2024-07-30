using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Dataskop.Data;
using Dataskop.Interaction;
using UnityEngine;

namespace Dataskop.Entities.Visualizations {

	public class Bubble : MonoBehaviour, IVisualization {

		[Header("References")]
		[SerializeField] private GameObject visObjectPrefab;
		[SerializeField] private Transform visObjectsContainer;
		[SerializeField] private BubbleVisObjectStyle visObjectStyle;
		[SerializeField] private Transform dropShadow;
		[SerializeField] private LineRenderer groundLine;
		[SerializeField] private LineRenderer labelLine;

		[Header("Vis Values")]
		[SerializeField] private Vector3 offset;
		[SerializeField] private float scaleFactor;
		[SerializeField] private VisHistoryConfiguration visHistoryConfig;
		[SerializeField] private Color deselectColor;
		[SerializeField] private Color hoverColor;
		[SerializeField] private Color selectColor;
		[SerializeField] private Color historyColor;
		private Coroutine groundLineRoutine;
		private Coroutine historyMove;
		private Coroutine labelLineRoutineLower;
		private Coroutine labelLineRoutineUpper;
		private Coroutine labelRoutine;

		private Vector3 moveTarget = Vector3.zero;
		private Vector3 prevScale;

		private int FocusIndex => DataPoint.FocusedIndex;

		public event Action SwipedDown;

		public event Action SwipedUp;

		public event Action<int> VisObjectHovered;

		public event Action<int> VisObjectSelected;

		public event Action<int> VisObjectDeselected;

		public event Action<IVisObject> FocusedVisObjectChanged;

		public DataPoint DataPoint { get; set; }

		public IVisObject[] VisObjects { get; set; }

		public IVisObject FocusedVisObject => VisObjects[FocusIndex];

		public VisualizationOption VisOption { get; set; }

		public VisHistoryConfiguration VisHistoryConfiguration { get; set; }

		public bool IsSelected { get; set; }

		public bool IsInitialized { get; set; }

		public bool HasHistoryEnabled { get; set; }

		public Transform VisOrigin { get; set; }

		public MeasurementType[] AllowedMeasurementTypes { get; set; } = {
			MeasurementType.Float,
			MeasurementType.Bool
		};

		public Vector3 Offset { get; set; }

		public float Scale { get; set; }

		public VisualizationType Type { get; set; }

		public int PreviousIndex { get; set; }

		public IVisObjectStyle VisObjectStyle { get; set; }

		public void Initialize(DataPoint dp) {

			DataPoint = dp;
			VisOrigin = transform;
			Scale = scaleFactor;
			Offset = offset;
			VisHistoryConfiguration = visHistoryConfig;
			VisObjectStyle = visObjectStyle;
			Type = VisualizationType.Bubble;

			VisOrigin.localScale *= Scale;
			VisOrigin.root.localPosition = Offset;

			VisObjects = DataPoint.MeasurementDefinition.MeasurementResults.Count < VisHistoryConfiguration.visibleHistoryCount
				? new IVisObject[dp.MeasurementDefinition.MeasurementResults.Count]
				: new IVisObject[VisHistoryConfiguration.visibleHistoryCount];

			GameObject visObject = Instantiate(visObjectPrefab, transform.position, Quaternion.identity, visObjectsContainer);
			VisObjects[FocusIndex] = visObject.GetComponent<IVisObject>();
			VisObjects[FocusIndex].HasHovered += OnVisObjectHovered;
			VisObjects[FocusIndex].HasSelected += OnVisObjectSelected;
			VisObjects[FocusIndex].HasDeselected += OnVisObjectDeselected;
			VisObjects[FocusIndex].VisCollider.enabled = true;

			dropShadow.transform.localScale *= Scale;
			dropShadow.transform.localPosition -= Offset;

			groundLine.startWidth = 0.0075f;
			groundLine.endWidth = 0.0075f;
			groundLine.SetPosition(1, dropShadow.localPosition);

			labelLine.startWidth = 0.0075f;
			labelLine.endWidth = 0.0075f;

			PreviousIndex = FocusIndex;
			OnFocusedIndexChanged(DataPoint.MeasurementDefinition, FocusIndex);
			IsInitialized = true;
		}

		public void OnFocusedIndexChanged(MeasurementDefinition def, int index) {

			if (!AllowedMeasurementTypes.Contains(DataPoint.MeasurementDefinition.MeasurementType)) {
				NotificationHandler.Add(new Notification {
					Category = NotificationCategory.Error,
					Text = "Value Type not supported by this visualization.",
					DisplayDuration = 5f
				});
				return;
			}

			if (!HasHistoryEnabled) {

				MeasurementResult focusedResult = def.MeasurementResults[index];

				if (FocusIndex != PreviousIndex) {
					VisObjects[FocusIndex] = VisObjects[PreviousIndex];
					VisObjects[PreviousIndex] = null;
					PreviousIndex = FocusIndex;
				}

				UpdateVisObject(VisObjects[FocusIndex], FocusIndex, focusedResult, true, true,
					IsSelected ? VisObjectStyle.Styles[0].selectionMaterial : VisObjectStyle.Styles[0].defaultMaterial);

			}
			else {

				int objectCountDistance = Mathf.Abs(PreviousIndex - FocusIndex);

				if (historyMove != null) {
					StopCoroutine(historyMove);
					visObjectsContainer.transform.position = moveTarget;
				}

				historyMove = StartCoroutine(MoveHistory(PreviousIndex < FocusIndex ? Vector3.down : Vector3.up, objectCountDistance));

				// VisObjects above current result
				for (int i = 1; i < VisObjects.Length - FocusIndex; i++) {
					int targetIndex = FocusIndex + i;
					MeasurementResult newResultToAssign = def.MeasurementResults[targetIndex];
					IVisObject targetObject = VisObjects[targetIndex];
					UpdateVisObject(targetObject, targetIndex, newResultToAssign, false, false, VisObjectStyle.Styles[0].timeMaterial);
				}

				// VisObjects below current result
				for (int i = 1; i <= FocusIndex; i++) {
					int targetIndex = FocusIndex - i;
					MeasurementResult newResultToAssign = def.MeasurementResults[targetIndex];
					IVisObject targetObject = VisObjects[targetIndex];
					UpdateVisObject(targetObject, targetIndex, newResultToAssign, false, false, VisObjectStyle.Styles[0].timeMaterial);
				}

				MeasurementResult focusedResult = def.MeasurementResults[FocusIndex];
				UpdateVisObject(VisObjects[FocusIndex], FocusIndex, focusedResult, true, true,
					IsSelected ? VisObjectStyle.Styles[0].selectionMaterial : VisObjectStyle.Styles[0].defaultMaterial);
				PreviousIndex = FocusIndex;

			}

			FocusedVisObjectChanged?.Invoke(FocusedVisObject);
		}

		public void OnTimeSeriesToggled(bool isActive) {

			if (isActive) {

				IReadOnlyList<MeasurementResult> currentResults = DataPoint.MeasurementDefinition.MeasurementResults.ToList();
				float distance = visHistoryConfig.elementDistance;

				// VisObjects above current result
				for (int i = 1; i < VisObjects.Length - FocusIndex; i++) {

					if (currentResults[FocusIndex + i] == null) {
						continue;
					}

					Vector3 spawnPos = new(VisOrigin.position.x, VisOrigin.position.y + distance * i, VisOrigin.position.z);
					VisObjects[FocusIndex + i] = SpawnVisObject(FocusIndex + i, spawnPos, currentResults[FocusIndex + i], false, false,
						VisObjectStyle.Styles[0].timeMaterial);
				}

				// VisObjects below current result
				for (int i = 1; i <= FocusIndex; i++) {

					if (currentResults[FocusIndex - i] == null) {
						continue;
					}

					Vector3 spawnPos = new(VisOrigin.position.x, VisOrigin.position.y - distance * i, VisOrigin.position.z);
					VisObjects[FocusIndex - i] = SpawnVisObject(FocusIndex - i, spawnPos, currentResults[FocusIndex - i], false, false,
						VisObjectStyle.Styles[0].timeMaterial);
				}

				groundLine.enabled = false;
				HasHistoryEnabled = true;

			}
			else {

				if (!HasHistoryEnabled) {
					return;
				}

				ClearHistoryVisObjects();
				groundLine.enabled = true;
				HasHistoryEnabled = false;
			}

		}

		public void OnVisObjectHovered(int index) {

			if (index == FocusIndex) {
				if (!IsSelected) {

					if (VisObjects[index] == null) {
						return;
					}

					VisObjects[index].SetMaterials(VisObjectStyle.Styles[0].hoverMaterial);
				}
			}
			else {
				VisObjects[index].ShowDisplay();
			}

			VisObjectHovered?.Invoke(index);

		}

		public void OnVisObjectSelected(int index) {

			if (index == FocusIndex) {
				VisObjects[index].SetMaterials(VisObjectStyle.Styles[0].selectionMaterial);
			}

			IsSelected = true;
			VisObjectSelected?.Invoke(index);

		}

		public void OnVisObjectDeselected(int index) {

			if (index == FocusIndex) {

				if (VisObjects[index] == null) {
					return;
				}

				VisObjects[index].SetMaterials(VisObjectStyle.Styles[0].defaultMaterial);

				if (IsSelected) {
					IsSelected = false;
				}
			}
			else {
				VisObjects[index].HideDisplay();
			}

			VisObjectDeselected?.Invoke(index);

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

		public void ApplyStyle(VisualizationStyle style) {
			dropShadow.gameObject.SetActive(style.HasDropShadow);
			groundLine.gameObject.SetActive(style.HasGroundLine);
		}

		public void Despawn() {
			ClearVisObjects();
			DataPoint = null;
			Destroy(gameObject);
		}

		private IVisObject SpawnVisObject(int index, Vector3 pos, MeasurementResult result, bool visibleDisplay,
			bool focused, params Material[] materials) {

			GameObject newVis = Instantiate(visObjectPrefab, pos, visObjectsContainer.localRotation, visObjectsContainer);
			IVisObject visObject = newVis.GetComponent<IVisObject>();

			UpdateVisObject(visObject, index, result, visibleDisplay, focused, materials);
			visObject.HasHovered += OnVisObjectHovered;
			visObject.HasSelected += OnVisObjectSelected;
			visObject.HasDeselected += OnVisObjectDeselected;
			visObject.VisCollider.enabled = true;

			return visObject;

		}

		private void UpdateVisObject(IVisObject target, int index, MeasurementResult result,
			bool visibleDisplay, bool focused, params Material[] materials) {

			if (target == null) {
				return;
			}

			target.SetDisplayData(new VisualizationResultDisplayData {
				Result = result,
				Type = result.MeasurementDefinition.MeasurementType,
				Attribute = DataPoint.Attribute,
				AuthorSprite = result.Author != string.Empty
					? DataPoint.AuthorRepository.AuthorSprites[result.Author]
					: null
			});

			target.Index = index;
			target.IsFocused = focused;

			if (visibleDisplay) {
				target.ShowDisplay();
			}
			else {
				target.HideDisplay();
			}

			target.SetMaterials(materials);
		}

		private void ClearHistoryVisObjects() {

			if (!HasHistoryEnabled) {
				return;
			}

			for (int i = 0; i < VisObjects.Length; i++) {

				if (i == FocusIndex) {
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

			}

		}

		private void ClearVisObjects() {

			for (int i = 0; i < VisObjects.Length - 1; i++) {

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

		private IEnumerator MoveHistory(Vector3 direction, int multiplier = 1) {

			Vector3 startPosition = visObjectsContainer.transform.position;
			moveTarget = visObjectsContainer.transform.position +
			             direction * (visHistoryConfig.elementDistance * multiplier);
			float moveDuration = visHistoryConfig.animationDuration;

			float t = 0;
			while (t < moveDuration) {

				visObjectsContainer.transform.position = Vector3.Lerp(startPosition, moveTarget, t / moveDuration);

				t += Time.deltaTime;
				yield return null;

			}

			visObjectsContainer.transform.position = moveTarget;
			historyMove = null;

		}

		private IEnumerator MoveLinePointTo(LineRenderer line, int index, Vector3 target, float duration) {

			float current = 0f;

			while (current <= duration) {

				current += Time.deltaTime;
				float currentPercentage = Mathf.Clamp01(current / duration);

				line.SetPosition(index, Vector3.LerpUnclamped(line.GetPosition(index), target, currentPercentage));

				yield return null;

			}

		}

	}

}