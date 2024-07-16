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
		private DataPoint? softSelectedDataPoint;

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
		private DataPoint? SoftSelectedDataPoint {
			get => softSelectedDataPoint;
			set {
				softSelectedDataPoint = value;

				if (SelectedDataPoint == null) {
					onDataPointSoftSelected?.Invoke(SoftSelectedDataPoint);
				}
			}
		}

		private TimeElement? HoveredTimeElement { get; set; }

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

				if (hitGameObject.CompareTag("TimeElement")) {

					if (SoftSelectedDataPoint != null) {
						SoftSelectedDataPoint.SetSelectionStatus(false, false);
						SoftSelectedDataPoint = null;
					}

					TimeElement hoveredTimeElement = hitGameObject.GetComponent<TimeElement>();

					if (!hoveredTimeElement.Series.IsSpawned) return;

					if (HoveredTimeElement == hoveredTimeElement) return;

					if (HoveredTimeElement != null) {
						HoveredTimeElement.HideData();
					}

					HoveredTimeElement = hoveredTimeElement;
					HoveredTimeElement.DisplayData();
					return;

				}

				if (hitGameObject.CompareTag("VisObject")) {

					if (HoveredTimeElement != null) {
						HoveredTimeElement.HideData();
						HoveredTimeElement = null;
					}

					DataPoint hoveredDataPoint = hitGameObject.GetComponentInParent<Visualization>().DataPoint;
					IVisObject hoveredVisObject = hitGameObject.GetComponent<IVisObject>();

					if (!hoveredDataPoint.Vis.IsSpawned) return;

					if (SelectedDataPoint == hoveredDataPoint) return;

					if (SoftSelectedDataPoint == hoveredDataPoint) return;

					if (SoftSelectedDataPoint != null)
						SoftSelectedDataPoint.SetSelectionStatus(false, false);

					SoftSelectedDataPoint = hoveredDataPoint;
					SoftSelectedDataPoint.SetSelectionStatus(true, true);

				}

			}
			else {

				if (SoftSelectedDataPoint != null) {
					SoftSelectedDataPoint.SetSelectionStatus(false, false);
					SoftSelectedDataPoint = null;
				}

				if (HoveredTimeElement != null) {
					HoveredTimeElement.HideData();
					HoveredTimeElement = null;
				}

			}

		}

		private void SetSelectedDataPoint(GameObject? pointedGameObject) {

			if (pointedGameObject != null && !pointedGameObject.CompareTag("VisObject"))
				return;

			DataPoint? tappedDataPoint = pointedGameObject?.GetComponentInParent<Visualization>().DataPoint;

			if (tappedDataPoint == SelectedDataPoint) {
				RemoveSelection();
				return;
			}

			if (SelectedDataPoint != null)
				RemoveSelection();

			SelectedDataPoint = tappedDataPoint;

			if (SoftSelectedDataPoint == SelectedDataPoint)
				SoftSelectedDataPoint = null;

			SelectedDataPoint?.SetSelectionStatus(true, false);

		}

		public void SelectDataPointOnVisualizationChange() {
			PreviouslySelectedDataPoint = SelectedDataPoint;

			if (PreviouslySelectedDataPoint != null) {
				onVisChangeWithSelection?.Invoke(true);
				StartCoroutine(SelectPreviouslySelectedDataPoint());
			}
		}

		private void RemoveSelection() {
			SelectedDataPoint?.SetSelectionStatus(false, false);
			SelectedDataPoint = null;
		}

		private IEnumerator SelectPreviouslySelectedDataPoint() {
			yield return new WaitForEndOfFrame();
			SelectedDataPoint = PreviouslySelectedDataPoint;
			SelectedDataPoint?.SetSelectionStatus(true, false);
			onVisChangeWithSelection?.Invoke(false);
		}

	}

}