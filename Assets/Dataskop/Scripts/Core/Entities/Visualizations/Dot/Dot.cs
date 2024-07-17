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

		public void OnDataPointChanged() {

			VisOrigin = transform;
			Scale = scaleFactor;
			Offset = offset;
			TimeSeriesConfig = timeSeriesConfiguration;

			DataPoint.MeasurementResultChanged += OnMeasurementResultChanged;

			Type = VisualizationType.Dot;
			Options = Instantiate(options);

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

			OnMeasurementResultChanged(DataPoint.FocusedMeasurement);
			IsInitialized = true;

		}

		public void OnMeasurementResultChanged(MeasurementResult mr) {

			if (!AllowedMeasurementTypes.Contains(mr.MeasurementDefinition.MeasurementType)) {
				NotificationHandler.Add(new Notification {
					Category = NotificationCategory.Error,
					Text = "Value Type not supported by this visualization.",
					DisplayDuration = 5f
				});
				return;
			}

			VisObjects[CurrentFocusIndex].SetDisplayData(new VisualizationResultDisplayData {
				Result = mr,
				Type = mr.MeasurementDefinition.MeasurementType,
				Attribute = DataPoint.Attribute,
				AuthorSprite = mr.Author != string.Empty ? DataPoint.AuthorRepository.AuthorSprites[mr.Author] : null
			});

			VisObjects[CurrentFocusIndex].ShowDisplay();
			VisObjects[CurrentFocusIndex].Index = CurrentFocusIndex;

		}

		public void ApplyStyle(VisualizationStyle style) {
			dropShadow.gameObject.SetActive(style.HasDropShadow);
			groundLine.gameObject.SetActive(style.HasGroundLine);
		}

		public void Swiped(PointerInteraction pointerInteraction) {

			switch (pointerInteraction.Direction.y) {
				case > 0.20f:
					SwipedUp?.Invoke();
					break;
				case < -0.20f:
					SwipedDown?.Invoke();
					break;
			}

		}

		public void Despawn() {
			DataPoint.MeasurementResultChanged -= OnMeasurementResultChanged;
			DataPoint = null;
			Destroy(gameObject);
		}

		public void OnMeasurementResultsUpdated() {

			OnMeasurementResultChanged(DataPoint.MeasurementDefinition.GetLatestMeasurementResult());
			/*
			if (TimeSeries.IsSpawned) {
				TimeSeries.OnMeasurementResultsUpdated(DataPoint.MeasurementDefinition.MeasurementResults.ToArray());
			}
			*/

		}

		public void OnTimeSeriesToggled(bool isActive) {

			if (isActive) {
				groundLine.enabled = false;
				IReadOnlyList<MeasurementResult> currentResults = DataPoint.MeasurementDefinition.MeasurementResults.ToList();
				float distance = timeSeriesConfiguration.elementDistance;

				// VisObjects above current result
				for (int i = CurrentFocusIndex + 1; i < VisObjects.Length; i++) {
					Vector3 spawnPos = new(VisOrigin.position.x, VisOrigin.position.y + distance * i, VisOrigin.position.z);
					VisObjects[i] = SpawnVisObject(i, spawnPos, currentResults[i]);
				}

				// VisObjects below current result
				for (int i = CurrentFocusIndex - 1; i > 0; i--) {
					Vector3 spawnPos = new(VisOrigin.position.x, VisOrigin.position.y - distance * i, VisOrigin.position.z);
					VisObjects[i] = SpawnVisObject(i, spawnPos, currentResults[i]);
				}

				HasHistoryEnabled = true;

			}
			else {

				if (!HasHistoryEnabled) {
					return;
				}

				ClearVisObjects();
				groundLine.enabled = true;
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

		visImageRenderer.material = Options.styles[0].defaultMaterial;
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
			visObject.IsFocused = false;
			visObject.SetMaterial(Options.styles[0].timeMaterial);
			visObject.HideDisplay();
			visObject.HasHovered += OnVisObjectHovered;
			visObject.HasSelected += OnVisObjectSelected;
			visObject.HasDeselected += OnVisObjectDeselected;

			return visObject;

		}

		private void ClearVisObjects() {

			for (int i = 0; i < VisObjects.Length; i++) {
				if (i == CurrentFocusIndex) {
					continue;
				}

				VisObjects[i].HasHovered -= OnVisObjectHovered;
				VisObjects[i].HasSelected -= OnVisObjectSelected;
				VisObjects[i].HasDeselected -= OnVisObjectDeselected;
				VisObjects[i].Delete();
				VisObjects[i] = null;

			}

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