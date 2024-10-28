using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Dataskop.Data;
using Dataskop.Entities;
using Dataskop.Entities.Visualizations;
using Dataskop.Interaction;
using UnityEngine;

namespace Dataskop {

	public class RadialBar : MonoBehaviour, IVisualization {

		[Header("References")]
		[SerializeField] private GameObject visObjectPrefab;
		[SerializeField] private Transform visObjectsContainer;
		[SerializeField] private GameObject noResultsIndicator;
		[SerializeField] private RadialBarDataDisplay focusedDataDisplay;
		[SerializeField] private RadialBarDataDisplay hoverDataDisplay;

		[Header("Vis Values")]
		[SerializeField] private Vector3 offset;
		[SerializeField] private float scaleFactor;

		public event Action SwipedDown;

		public event Action SwipedUp;

		public event Action<int> VisObjectHovered;

		public event Action<int> VisObjectSelected;

		public event Action<int> VisObjectDeselected;

		public event Action<IVisObject> FocusedVisObjectChanged;

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

		private float Scale { get; set; }

		private MeasurementResultRange[] CurrentRanges { get; set; }

		public void Initialize(DataPoint dp) {

			DataPoint = dp;
			VisOrigin = transform;
			Scale = scaleFactor;
			Offset = offset;
			VisHistoryConfiguration = new VisHistoryConfiguration(0, 0, false);
			Type = VisualizationType.Dot;
			VisOrigin.localScale *= Scale;
			VisOrigin.root.localPosition = Offset;
			HasHistoryEnabled = false;

			CurrentRanges = DataPoint.Device.MeasurementDefinitions
				.SelectMany(md => md.MeasurementResults)
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

			// Only display the latest value, thus only one VisObject is needed.
			VisObjects = new IVisObject[1];

			GameObject visObject = Instantiate(visObjectPrefab, transform.position, Quaternion.identity, visObjectsContainer);
			VisObjects[DataPoint.FocusedIndex] = visObject.GetComponent<IVisObject>();
			VisObjects[DataPoint.FocusedIndex].HasHovered += OnVisObjectHovered;
			VisObjects[DataPoint.FocusedIndex].HasSelected += OnVisObjectSelected;
			VisObjects[DataPoint.FocusedIndex].HasDeselected += OnVisObjectDeselected;
			VisObjects[DataPoint.FocusedIndex].VisCollider.enabled = true;

			/*
			SetLinePosition(groundLine,
				new Vector3(VisOrigin.localPosition.x,
					VisOrigin.localPosition.y - VisObjects[DataPoint.FocusedIndex].VisRenderer.sprite.bounds.size.y * 0.75f,
					VisOrigin.localPosition.z),
				dropShadow.localPosition);
			*/

			OnFocusedIndexChanged(DataPoint.FocusedIndex);
		}

		public void OnTimeSeriesToggled(bool isActive) {
			return;
		}

		public void OnFocusedIndexChanged(int index) {
			return;
		}

		public void OnSwipeInteraction(PointerInteraction pointerInteraction) {
			return;
		}

		public void OnMeasurementResultRangeUpdated() {
			throw new NotImplementedException();
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

			GameObject newVis = Instantiate(visObjectPrefab, pos, visObjectsContainer.localRotation, visObjectsContainer);
			IVisObject visObject = newVis.GetComponent<IVisObject>();

			UpdateVisObject(visObject, index, result, focused, state);
			visObject.HasHovered += OnVisObjectHovered;
			visObject.HasSelected += OnVisObjectSelected;
			visObject.HasDeselected += OnVisObjectDeselected;
			visObject.VisCollider.enabled = true;

			return visObject;

		}

		private void UpdateVisObject(IVisObject target, int index, MeasurementResult result, bool focused, VisObjectState state) {

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

			}

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