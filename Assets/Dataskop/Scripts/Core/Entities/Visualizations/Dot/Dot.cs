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
		[SerializeField] private TimeSeriesConfig timeSeriesConfiguration;
		[SerializeField] private Color deselectColor;
		[SerializeField] private Color hoverColor;
		[SerializeField] private Color selectColor;
		[SerializeField] private Color historyColor;

		private DataPoint dataPoint;

		private DotOptions Options { get; set; }

		private int CurrentFocusIndex => DataPoint.FocusedMeasurementIndex;

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

		public TimeSeriesConfig TimeSeriesConfig { get; set; }

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
			TimeSeriesConfig = timeSeriesConfiguration;

			Type = VisualizationType.Dot;
			Options = Instantiate(options);

			//TODO: Handle MeasurementResults being less than configured time series configuration visible history count.
			VisObjects = new IVisObject[timeSeriesConfiguration.visibleHistoryCount];
			GameObject visObject = Instantiate(visPrefab, transform.position, Quaternion.identity, visObjectsContainer);
			VisObjects[CurrentFocusIndex] = visObject.GetComponent<IVisObject>();
			VisObjects[CurrentFocusIndex].HasHovered += OnVisObjectHovered;
			VisObjects[CurrentFocusIndex].HasSelected += OnVisObjectSelected;
			VisObjects[CurrentFocusIndex].HasDeselected += OnVisObjectDeselected;

			VisOrigin.localScale *= Scale;
			VisOrigin.root.localPosition = Offset;

			dropShadow.transform.localScale *= Scale;
			dropShadow.transform.localPosition -= Offset;

			/*
			SetLinePosition(groundLine,
				new Vector3(VisTransform.localPosition.x, VisTransform.localPosition.y - visImageRenderer.sprite.bounds.size.y * 0.75f,
					VisTransform.localPosition.z),
				dropShadow.localPosition);

			groundLine.startWidth = 0.0075f;
			groundLine.endWidth = 0.0075f;
			*/

			OnFocusedMeasurementIndexChanged(DataPoint.MeasurementDefinition, CurrentFocusIndex);
			IsInitialized = true;

		}

		public void OnFocusedMeasurementIndexChanged(MeasurementDefinition def, int index) {

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

				if (CurrentFocusIndex != PreviousIndex) {
					VisObjects[CurrentFocusIndex] = VisObjects[PreviousIndex];
					VisObjects[PreviousIndex] = null;
					PreviousIndex = CurrentFocusIndex;
				}

				VisObjects[CurrentFocusIndex].SetDisplayData(new VisualizationResultDisplayData {
					Result = focusedResult,
					Type = focusedResult.MeasurementDefinition.MeasurementType,
					Attribute = DataPoint.Attribute,
					AuthorSprite = focusedResult.Author != string.Empty ? DataPoint.AuthorRepository.AuthorSprites[focusedResult.Author]
						: null
				});

				VisObjects[CurrentFocusIndex].Index = CurrentFocusIndex;
				VisObjects[CurrentFocusIndex].IsFocused = true;

			}
			else {

				StartCoroutine(MoveHistory(PreviousIndex < CurrentFocusIndex ? Vector3.down : Vector3.up));
				PreviousIndex = CurrentFocusIndex;

				// VisObjects above current result
				for (int i = 1; i < VisObjects.Length - CurrentFocusIndex; i++) {

					int targetIndex = CurrentFocusIndex + i;
					MeasurementResult newResultToAssign = def.MeasurementResults[targetIndex];
					IVisObject targetObject = VisObjects[targetIndex];

					targetObject.SetDisplayData(new VisualizationResultDisplayData {
						Result = newResultToAssign,
						Type = newResultToAssign.MeasurementDefinition.MeasurementType,
						Attribute = DataPoint.Attribute,
						AuthorSprite = newResultToAssign.Author != string.Empty
							? DataPoint.AuthorRepository.AuthorSprites[newResultToAssign.Author]
							: null
					});

					targetObject.Index = targetIndex;
					targetObject.SetMaterial(Options.styles[0].timeMaterial);
					targetObject.HideDisplay();

				}

				// VisObjects below current result
				for (int i = 1; i <= CurrentFocusIndex; i++) {

					int targetIndex = CurrentFocusIndex - i;
					MeasurementResult newResultToAssign = def.MeasurementResults[targetIndex];
					IVisObject targetObject = VisObjects[targetIndex];

					targetObject.SetDisplayData(new VisualizationResultDisplayData {
						Result = newResultToAssign,
						Type = newResultToAssign.MeasurementDefinition.MeasurementType,
						Attribute = DataPoint.Attribute,
						AuthorSprite = newResultToAssign.Author != string.Empty
							? DataPoint.AuthorRepository.AuthorSprites[newResultToAssign.Author]
							: null
					});

					targetObject.Index = targetIndex;
					targetObject.SetMaterial(Options.styles[0].timeMaterial);
					targetObject.HideDisplay();

				}

				MeasurementResult focusedResult = def.MeasurementResults[CurrentFocusIndex];
				VisObjects[CurrentFocusIndex].SetDisplayData(new VisualizationResultDisplayData {
					Result = focusedResult,
					Type = focusedResult.MeasurementDefinition.MeasurementType,
					Attribute = DataPoint.Attribute,
					AuthorSprite = focusedResult.Author != string.Empty
						? DataPoint.AuthorRepository.AuthorSprites[focusedResult.Author]
						: null
				});

				VisObjects[CurrentFocusIndex].Index = CurrentFocusIndex;
				VisObjects[CurrentFocusIndex].SetMaterial(Options.styles[0].defaultMaterial);
				VisObjects[CurrentFocusIndex].ShowDisplay();

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

		public void OnMeasurementResultsUpdated() {
			OnFocusedMeasurementIndexChanged(DataPoint.MeasurementDefinition, CurrentFocusIndex);
		}

		public void OnTimeSeriesToggled(bool isActive) {

			if (isActive) {
				//groundLine.enabled = false;
				IReadOnlyList<MeasurementResult> currentResults = DataPoint.MeasurementDefinition.MeasurementResults.ToList();
				float distance = timeSeriesConfiguration.elementDistance;

				// VisObjects above current result
				for (int i = 1; i < VisObjects.Length - CurrentFocusIndex; i++) {
					Vector3 spawnPos = new(VisOrigin.position.x, VisOrigin.position.y + distance * (i), VisOrigin.position.z);
					VisObjects[CurrentFocusIndex + i] =
						SpawnVisObject(CurrentFocusIndex + i, spawnPos, currentResults[CurrentFocusIndex + i]);
				}

				// VisObjects below current result
				for (int i = 1; i <= CurrentFocusIndex; i++) {
					Vector3 spawnPos = new(VisOrigin.position.x, VisOrigin.position.y - distance * (i), VisOrigin.position.z);
					VisObjects[CurrentFocusIndex - i] =
						SpawnVisObject(CurrentFocusIndex - i, spawnPos, currentResults[CurrentFocusIndex - i]);
				}

				HasHistoryEnabled = true;

			}
			else {

				if (!HasHistoryEnabled) {
					return;
				}

				ClearHistoryVisObjects();
				HasHistoryEnabled = false;
				//groundLine.enabled = true;
			}

		}

		public void OnVisObjectHovered(int index) {

			if (index == CurrentFocusIndex) {
				VisObjects[index].SetMaterial(Options.styles[0].hoverMaterial);
			}
			else {
				VisObjects[index].ShowDisplay();
			}

			VisObjectHovered?.Invoke(index);

		}

		public void OnVisObjectSelected(int index) {

			if (index == CurrentFocusIndex) {
				VisObjects[index].SetMaterial(Options.styles[0].selectionMaterial);
			}

			//TODO: Setting new Focus Index to the tapped VisObject and do animation work
			IsSelected = true;
			VisObjectSelected?.Invoke(index);

		}

		public void OnVisObjectDeselected(int index) {
/*
			if (IsSelected) {
				if (animationCoroutine != null) {
					CancelAnimation();
				}
			}
			//animationTarget = visTransform.localScale / selectionScale;

			/*
			animationCoroutine = StartCoroutine(Lerper.TransformLerpOnCurve(
				visTransform,
				TransformValue.Scale,
				VisTransform.localScale,
				animationTarget,
				animationTimeOnDeselect,
				animationCurveDeselect,
				OnScaleChanged
			));
		}

		*/
			if (index == CurrentFocusIndex) {
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

			visObject.SetDisplayData(new VisualizationResultDisplayData {
				Result = result,
				Type = result.MeasurementDefinition.MeasurementType,
				Attribute = DataPoint.Attribute,
				AuthorSprite = result.Author != string.Empty
					? DataPoint.AuthorRepository.AuthorSprites[result.Author] : null
			});

			visObject.Index = index;
			visObject.SetMaterial(Options.styles[0].timeMaterial);
			visObject.HideDisplay();
			visObject.HasHovered += OnVisObjectHovered;
			visObject.HasSelected += OnVisObjectSelected;
			visObject.HasDeselected += OnVisObjectDeselected;

			return visObject;

		}

		private void ClearHistoryVisObjects() {

			if (!HasHistoryEnabled) {
				return;
			}

			for (int i = 0; i < VisObjects.Length; i++) {

				if (i == CurrentFocusIndex) {
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

		public void Despawn() {
			ClearVisObjects();
			DataPoint = null;
			Destroy(gameObject);
		}

		private IEnumerator MoveHistory(Vector3 direction) {

			Vector3 startPosition = visObjectsContainer.transform.position;
			Vector3 targetPosition = visObjectsContainer.transform.position + timeSeriesConfiguration.elementDistance * direction;
			float moveDuration = timeSeriesConfiguration.animationDuration;

			float t = 0;
			while (t < moveDuration) {

				visObjectsContainer.transform.position = Vector3.Lerp(startPosition, targetPosition, t / moveDuration);

				t += Time.deltaTime;
				yield return null;

			}

			visObjectsContainer.transform.position = targetPosition;

		}

		private void CancelAnimation() {
			/*
			StopCoroutine(animationCoroutine);
			VisTransform.localScale = animationTarget;
			*/
		}

		private void SetLinePosition(LineRenderer lr, Vector3 startPoint, Vector3 endPoint) {
			lr.SetPosition(0, startPoint);
			lr.SetPosition(1, endPoint);
		}

		private void OnScaleChanged() {
			/*
			moveLineCoroutine = StartCoroutine(MoveLinePointTo(0,
				new Vector3(VisTransform.localPosition.x, VisTransform.localPosition.y - visImageRenderer.sprite.bounds.size.y * 0.75f,
					VisTransform.localPosition.z),
				0.1f));
				*/
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

	}

}