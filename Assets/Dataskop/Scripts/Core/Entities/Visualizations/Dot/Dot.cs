using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Dataskop.Data;
using Dataskop.Interaction;
using UnityEngine;

namespace Dataskop.Entities.Visualizations {

	public class Dot : MonoBehaviour, IVisualization {

		[Header("References")]
		[SerializeField] private GameObject visPrefab;
		[SerializeField] private Transform visObjectsContainer;
		[SerializeField] private DotOptions options;
		[SerializeField] private Transform dropShadow;
		[SerializeField] private LineRenderer groundLine;
		[SerializeField] private Sprite[] boolIcons;

		[Header("Vis Values")]
		[SerializeField] private Vector3 offset;
		[SerializeField] private float scaleFactor;
		[SerializeField] private VisHistoryConfiguration timeSeriesConfiguration;
		[SerializeField] private Color deselectColor;
		[SerializeField] private Color hoverColor;
		[SerializeField] private Color selectColor;
		[SerializeField] private Color historyColor;

		private DataPoint dataPoint;

		private DotOptions Options { get; set; }

		private int FocusIndex => DataPoint.FocusedIndex;

		public event Action SwipedDown;

		public event Action SwipedUp;

		public event Action<int> VisObjectHovered;

		public event Action<int> VisObjectSelected;

		public event Action<int> VisObjectDeselected;

		public IVisObject[] VisObjects { get; set; }

		public DataPoint DataPoint {
			get => dataPoint;
			set {
				dataPoint = value;
				if (value != null) {
					OnDataPointChanged();
				}
			}
		}

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

		public int PreviousIndex { get; set; } = 0;

		public void OnDataPointChanged() {

			VisOrigin = transform;
			Scale = scaleFactor;
			Offset = offset;
			VisHistoryConfiguration = timeSeriesConfiguration;

			Type = VisualizationType.Dot;
			Options = Instantiate(options);

			//TODO: Handle MeasurementResults being less than configured time series configuration visible history count.
			VisObjects = new IVisObject[timeSeriesConfiguration.visibleHistoryCount];
			GameObject visObject = Instantiate(visPrefab, transform.position, Quaternion.identity, visObjectsContainer);
			VisObjects[FocusIndex] = visObject.GetComponent<IVisObject>();
			VisObjects[FocusIndex].HasHovered += OnVisObjectHovered;
			VisObjects[FocusIndex].HasSelected += OnVisObjectSelected;
			VisObjects[FocusIndex].HasDeselected += OnVisObjectDeselected;

			VisOrigin.localScale *= Scale;
			VisOrigin.root.localPosition = Offset;

			dropShadow.transform.localScale *= Scale;
			dropShadow.transform.localPosition -= Offset;

			SetLinePosition(groundLine,
				new Vector3(VisOrigin.localPosition.x,
					VisOrigin.localPosition.y - VisObjects[FocusIndex].VisRenderer.sprite.bounds.size.y * 0.75f,
					VisOrigin.localPosition.z),
				dropShadow.localPosition);

			groundLine.startWidth = 0.0075f;
			groundLine.endWidth = 0.0075f;

			OnFocusedIndexChanged(DataPoint.MeasurementDefinition, FocusIndex);
			IsInitialized = true;

		}

		public void OnFocusedIndexChanged(MeasurementDefinition def, int index) {

			if (!AllowedMeasurementTypes.Contains(def.MeasurementResults[index].MeasurementDefinition.MeasurementType)) {
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

				UpdateVisObject(VisObjects[FocusIndex], FocusIndex, focusedResult,
					IsSelected ? Options.styles[0].selectionMaterial : Options.styles[0].defaultMaterial, true, true);

			}
			else {

				int objectCountDistance = Mathf.Abs(PreviousIndex - FocusIndex);
				StartCoroutine(MoveHistory(PreviousIndex < FocusIndex ? Vector3.down : Vector3.up, objectCountDistance));

				// VisObjects above current result
				for (int i = 1; i < VisObjects.Length - FocusIndex; i++) {
					int targetIndex = FocusIndex + i;
					MeasurementResult newResultToAssign = def.MeasurementResults[targetIndex];
					IVisObject targetObject = VisObjects[targetIndex];
					UpdateVisObject(targetObject, targetIndex, newResultToAssign, Options.styles[0].timeMaterial);
				}

				// VisObjects below current result
				for (int i = 1; i <= FocusIndex; i++) {
					int targetIndex = FocusIndex - i;
					MeasurementResult newResultToAssign = def.MeasurementResults[targetIndex];
					IVisObject targetObject = VisObjects[targetIndex];
					UpdateVisObject(targetObject, targetIndex, newResultToAssign, Options.styles[0].timeMaterial);
				}

				MeasurementResult focusedResult = def.MeasurementResults[FocusIndex];
				UpdateVisObject(VisObjects[FocusIndex], FocusIndex, focusedResult,
					IsSelected ? Options.styles[0].selectionMaterial : Options.styles[0].defaultMaterial, true, true);
				PreviousIndex = FocusIndex;

			}

		}

		public void OnTimeSeriesToggled(bool isActive) {

			if (isActive) {
				
				IReadOnlyList<MeasurementResult> currentResults = DataPoint.MeasurementDefinition.MeasurementResults.ToList();
				float distance = timeSeriesConfiguration.elementDistance;

				// VisObjects above current result
				for (int i = 1; i < VisObjects.Length - FocusIndex; i++) {
					Vector3 spawnPos = new(VisOrigin.position.x, VisOrigin.position.y + distance * (i), VisOrigin.position.z);
					VisObjects[FocusIndex + i] =
						SpawnVisObject(FocusIndex + i, spawnPos, currentResults[FocusIndex + i]);
				}

				// VisObjects below current result
				for (int i = 1; i <= FocusIndex; i++) {
					Vector3 spawnPos = new(VisOrigin.position.x, VisOrigin.position.y - distance * (i), VisOrigin.position.z);
					VisObjects[FocusIndex - i] =
						SpawnVisObject(FocusIndex - i, spawnPos, currentResults[FocusIndex - i]);
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
					VisObjects[index].SetMaterial(Options.styles[0].hoverMaterial);
				}
			}
			else {
				VisObjects[index].ShowDisplay();
			}

			VisObjectHovered?.Invoke(index);

		}

		public void OnVisObjectSelected(int index) {

			if (index == FocusIndex) {
				VisObjects[index].SetMaterial(Options.styles[0].selectionMaterial);
			}

			IsSelected = true;
			VisObjectSelected?.Invoke(index);

		}

		public void OnVisObjectDeselected(int index) {

			if (index == FocusIndex) {
				VisObjects[index].SetMaterial(Options.styles[0].defaultMaterial);
			}
			else {
				VisObjects[index].HideDisplay();
			}

			IsSelected = false;
			VisObjectDeselected?.Invoke(index);

		}

		private IVisObject SpawnVisObject(int index, Vector3 pos, MeasurementResult result) {

			GameObject newVis = Instantiate(visPrefab, pos, visObjectsContainer.localRotation, visObjectsContainer);
			IVisObject visObject = newVis.GetComponent<IVisObject>();

			UpdateVisObject(visObject, index, result, Options.styles[0].timeMaterial);
			visObject.HasHovered += OnVisObjectHovered;
			visObject.HasSelected += OnVisObjectSelected;
			visObject.HasDeselected += OnVisObjectDeselected;

			return visObject;

		}

		private void UpdateVisObject(IVisObject target, int index, MeasurementResult result, Material material, bool visibleDisplay = false,
			bool focused = false) {

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
			target.SetMaterial(material);

			if (visibleDisplay) {
				target.ShowDisplay();
			}
			else {
				target.HideDisplay();
			}

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

		public void ApplyStyle(VisualizationStyle style) {
			dropShadow.gameObject.SetActive(style.HasDropShadow);
			groundLine.gameObject.SetActive(style.HasGroundLine);
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

		private IEnumerator MoveHistory(Vector3 direction, int multiplier = 1) {

			Vector3 startPosition = visObjectsContainer.transform.position;
			Vector3 targetPosition = visObjectsContainer.transform.position +
			                         direction * (timeSeriesConfiguration.elementDistance * multiplier);
			float moveDuration = timeSeriesConfiguration.animationDuration;

			float t = 0;
			while (t < moveDuration) {

				visObjectsContainer.transform.position = Vector3.Lerp(startPosition, targetPosition, t / moveDuration);

				t += Time.deltaTime;
				yield return null;

			}

			visObjectsContainer.transform.position = targetPosition;

		}

		private void SetLinePosition(LineRenderer lr, Vector3 startPoint, Vector3 endPoint) {
			lr.SetPosition(0, startPoint);
			lr.SetPosition(1, endPoint);
		}

		private IEnumerator MoveLinePointTo(int index, Vector3 target, float duration) {
			float current = 0f;

			while (current <= duration) {
				current += Time.deltaTime;
				float currentPercentage = Mathf.Clamp01(current / duration);

				groundLine.SetPosition(index, Vector3.LerpUnclamped(groundLine.GetPosition(index), target, currentPercentage));

				yield return null;
			}
		}

		public void Despawn() {
			ClearVisObjects();
			DataPoint = null;
			Destroy(gameObject);
		}

	}

}