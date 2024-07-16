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

		private DataPoint? selectedDataPoint;
		private DataPoint? hoveredDataPoint;

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

			if (i.isSwipe) return;

			SetSelectedDataPoint(i.startingGameObject);

		}

		private void SetHoveredDataPoint(Ray ray) {

			if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, TargetLayerMask)) {

				GameObject hitGameObject = hit.collider.gameObject;

				if (!hitGameObject.CompareTag("VisObject")) {
					return;
				}

				DataPoint hoveredDataPoint = hitGameObject.GetComponentInParent<Visualization>().DataPoint;
				IVisObject hoveredVisObject = hitGameObject.GetComponent<IVisObject>();

				if (hoveredVisObject != HoveredVisObject) {
					HoveredVisObject = hoveredVisObject;
					HoveredVisObject?.OnHover();
				}

				if (SelectedDataPoint == hoveredDataPoint) return;

				if (HoveredDataPoint == hoveredDataPoint) return;

				if (HoveredDataPoint != null) {
					HoveredDataPoint.SetSelectionStatus(false, false);
				}

				HoveredDataPoint = hoveredDataPoint;
				HoveredDataPoint.SetSelectionStatus(true, true);

			}
			else {

				if (HoveredDataPoint != null) {
					HoveredDataPoint.SetSelectionStatus(false, false);
					HoveredDataPoint = null;
				}

				HoveredVisObject?.OnDeselect();

			}

		}

		private void SetSelectedDataPoint(GameObject? pointedGameObject) {

			if (pointedGameObject != null && !pointedGameObject.CompareTag("VisObject")) {
				return;
			}

			if (pointedGameObject == null) {
				SelectedVisObject?.OnDeselect();
				SelectedVisObject = null;
				SelectedDataPoint = null;
				return;
			}

			DataPoint? tappedDataPoint = pointedGameObject?.GetComponentInParent<Visualization>().DataPoint;
			IVisObject? tappedVisObject = pointedGameObject?.GetComponent<IVisObject>();

			if (tappedVisObject == SelectedVisObject && tappedDataPoint == SelectedDataPoint) {
				SelectedVisObject?.OnDeselect();
				SelectedVisObject = null;
				SelectedDataPoint?.SetSelectionStatus(false, false);
				SelectedDataPoint = null;
				return;
			}

			if (tappedVisObject != SelectedVisObject) {

				SelectedVisObject = tappedVisObject;
				SelectedVisObject?.OnSelect();

				if (tappedDataPoint != SelectedDataPoint) {
					SelectedDataPoint?.SetSelectionStatus(false, false);
					SelectedDataPoint = tappedDataPoint;
					SelectedDataPoint?.SetSelectionStatus(true, false);

					if (HoveredDataPoint == SelectedDataPoint) {
						HoveredDataPoint = null;
					}
					
				}

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
			SelectedDataPoint?.SetSelectionStatus(true, false);
			onVisChangeWithSelection?.Invoke(false);
		}

	}

}