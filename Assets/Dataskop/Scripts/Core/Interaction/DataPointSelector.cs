#nullable enable

using System.Collections;
using Dataskop.Entities;
using Dataskop.Entities.Visualizations;
using UnityEngine;
using UnityEngine.Events;

namespace Dataskop.Interaction {

	public class DataPointSelector : MonoBehaviour {

		private const int TargetLayerMask = 1 << 7;

		[Header("References")]
		[SerializeField] private Camera cam = null!;
		[SerializeField] private Vector3 screenRayPosition = Vector3.zero;
		[SerializeField] private InputHandler inputHandler = null!;

		[Header("Events")]
		public UnityEvent<DataPoint?>? onDataPointSelected;
		public UnityEvent<DataPoint?>? onDataPointSoftSelected;
		public UnityEvent<bool>? onVisChangeWithSelection;
		private DataPoint? hoveredDataPoint;

		private DataPoint? selectedDataPoint;

		/// <summary>
		///     The DataPoint which got selected with a tap.
		/// </summary>
		private DataPoint? SelectedDataPoint {
			get => selectedDataPoint;
			set {
				selectedDataPoint = value;
				onDataPointSelected?.Invoke(SelectedDataPoint);
			}
		}

		private DataPoint? PreviouslySelectedDataPoint { get; set; }

		/// <summary>
		///     The DataPoint which got selected with the reticule.
		/// </summary>
		private DataPoint? HoveredDataPoint {
			get => hoveredDataPoint;
			set {
				hoveredDataPoint = value;

				if (SelectedDataPoint == null) {
					onDataPointSoftSelected?.Invoke(HoveredDataPoint);
				}
			}
		}

		private IVisObject? HoveredVisObject { get; set; }

		private IVisObject? SelectedVisObject { get; set; }

		private Ray ReticuleToWorldRay => cam.ViewportPointToRay(screenRayPosition);

		private void Awake() {
			inputHandler.WorldPointerUpped += OnWorldPointerUpReceived;
		}

		private void FixedUpdate() {
			SetHoveredDataPoint(ReticuleToWorldRay);
		}

		private void OnWorldPointerUpReceived(PointerInteraction i) {

			if (i.isSwipe) {
				return;
			}

			SetSelectedDataPoint(i.startingGameObject);

		}

		private void SetHoveredDataPoint(Ray ray) {

			if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, TargetLayerMask)) {

				GameObject hitGameObject = hit.collider.gameObject;

				if (!hitGameObject.CompareTag("VisObject")) {
					return;
				}

				DataPoint dataPoint = hitGameObject.GetComponentInParent<IVisualization>().DataPoint;
				IVisObject visObject = hitGameObject.GetComponent<IVisObject>();

				if (visObject == HoveredVisObject) {
					return;
				}

				if (dataPoint.Vis.IsSelected) {

					if (dataPoint.Vis.FocusedVisObject != visObject) {

						if (HoveredVisObject == null) {
							HoveredVisObject = visObject;
							HoveredVisObject.OnHover();
						}
						else {
							HoveredVisObject.OnDeselect();
							HoveredVisObject = visObject;
							HoveredVisObject.OnHover();
						}

					}

				}
				else {

					HoveredDataPoint = dataPoint;
					HoveredDataPoint.SetSelectionStatus(SelectionState.Hovered);

					if (HoveredVisObject != null) {
						HoveredVisObject.OnDeselect();
					}

					HoveredVisObject = visObject;
					HoveredVisObject.OnHover();
				}

			}
			else {

				if (HoveredDataPoint == null) {

					if (HoveredVisObject != null) {
						HoveredVisObject.OnDeselect();
						HoveredVisObject = null;
					}

				}
				else {

					if (HoveredVisObject != null) {
						HoveredVisObject.OnDeselect();
						HoveredVisObject = null;
					}

					HoveredDataPoint?.SetSelectionStatus(SelectionState.Deselected);
					HoveredDataPoint = null;

				}

			}

		}

		private void SetSelectedDataPoint(GameObject? pointedGameObject) {

			if (pointedGameObject != null && !pointedGameObject.CompareTag("VisObject")) {
				return;
			}

			if (pointedGameObject == null) {
				SelectedVisObject?.OnDeselect();
				SelectedVisObject = null;
				SelectedDataPoint?.SetSelectionStatus(SelectionState.Deselected);
				SelectedDataPoint = null;
				return;
			}

			DataPoint? tappedDataPoint = pointedGameObject?.GetComponentInParent<DataPoint>();
			IVisObject? tappedVisObject = pointedGameObject?.GetComponent<IVisObject>();

			if (tappedVisObject == SelectedVisObject && tappedDataPoint == SelectedDataPoint) {
				SelectedVisObject?.OnDeselect();
				SelectedVisObject = null;
				SelectedDataPoint?.SetSelectionStatus(SelectionState.Deselected);
				SelectedDataPoint = null;
				return;
			}

			if (tappedVisObject == SelectedVisObject) {
				return;
			}

			SelectedVisObject?.OnDeselect();
			SelectedVisObject = tappedVisObject;
			SelectedVisObject?.OnSelect();

			if (tappedDataPoint == SelectedDataPoint) {
				return;
			}

			SelectedDataPoint?.SetSelectionStatus(SelectionState.Deselected);
			SelectedDataPoint = tappedDataPoint;
			SelectedDataPoint?.SetSelectionStatus(SelectionState.Selected);
			HoveredDataPoint = null;

			if (HoveredDataPoint == SelectedDataPoint) {
				HoveredVisObject = null;
			}

		}

		public void SelectDataPointOnVisualizationChange() {
			PreviouslySelectedDataPoint = SelectedDataPoint;

			if (PreviouslySelectedDataPoint == null) {
				return;
			}

			onVisChangeWithSelection?.Invoke(true);
			StartCoroutine(SelectPreviouslySelectedDataPoint());
		}

		private IEnumerator SelectPreviouslySelectedDataPoint() {
			yield return new WaitForEndOfFrame();
			SelectedDataPoint = PreviouslySelectedDataPoint;
			SelectedDataPoint?.SetSelectionStatus(SelectionState.Selected);
			onVisChangeWithSelection?.Invoke(false);
		}

	}

}