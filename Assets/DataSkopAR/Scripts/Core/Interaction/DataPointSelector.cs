using System.Collections;
using DataskopAR.Data;
using DataskopAR.Entities.Visualizations;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
namespace DataskopAR.Interaction {

	public class DataPointSelector : MonoBehaviour {

#region Constants

		private const int TargetLayerMask = 1 << 7;

#endregion

#region Fields

		[Header("References")]
		[SerializeField] private Camera cam;
		[SerializeField] private Vector3 screenRayPosition = Vector3.zero;

		[Header("Events")]
		public UnityEvent<DataPoint> onDataPointSelected;
		public UnityEvent<DataPoint> onDataPointSoftSelected;
		public UnityEvent<bool> onVisChangeWithSelection;

		private DataPoint selectedDataPoint;
		private DataPoint softSelectedDataPoint;

#endregion

#region Properties

		/// <summary>
		///     The DataPoint which got selected with a tap.
		/// </summary>
		public DataPoint SelectedDataPoint {
			get => selectedDataPoint;

			private set {
				selectedDataPoint = value;
				onDataPointSelected?.Invoke(SelectedDataPoint);
			}
		}

		private DataPoint PreviouslySelectedDataPoint { get; set; }

		/// <summary>
		///     The DataPoint which got selected with the reticule.
		/// </summary>
		public DataPoint SoftSelectedDataPoint {
			get => softSelectedDataPoint;

			private set {
				softSelectedDataPoint = value;

				if (SelectedDataPoint == null) {
					onDataPointSoftSelected?.Invoke(SoftSelectedDataPoint);
				}
			}
		}

		public TimeElement HoveredTimeElement { get; set; }

		private Ray TapScreenToWorldRay { get; set; }

		private Ray ReticuleToWorldRay => cam.ViewportPointToRay(screenRayPosition);

		private Vector2 TapPosition { get; set; }

#endregion

#region Methods

		private void FixedUpdate() {

			if (Physics.Raycast(ReticuleToWorldRay, out RaycastHit hit, Mathf.Infinity, TargetLayerMask)) {

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

				if (hitGameObject.CompareTag("Vis")) {

					if (HoveredTimeElement != null) {
						HoveredTimeElement.HideData();
						HoveredTimeElement = null;
					}

					DataPoint hoveredDataPoint = hitGameObject.GetComponentInParent<Visualization>().DataPoint;

					if (!hoveredDataPoint.Vis.IsSpawned) return;

					//Debug.DrawRay(ReticuleToWorldRay.origin, ReticuleToWorldRay.direction * 50f, Color.green, 20f);

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

		public void TapPositionInput(InputAction.CallbackContext ctx) {
			TapPosition = ctx.ReadValue<Vector2>();
		}

		public void TapInput(InputAction.CallbackContext ctx) {

			if (ctx.canceled) {

				if (UIInteractionDetection.IsPointerOverUi) {
					UIInteractionDetection.IsPointerOverUi = false;
					return;
				}

				TapScreenToWorldRay = cam.ScreenPointToRay(new Vector3(TapPosition.x, TapPosition.y, -5));
				SetSelectedDataPoint(TapScreenToWorldRay);
			}

		}

		private void SetSelectedDataPoint(Ray ray) {

			if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, TargetLayerMask)) {

				if (!hit.collider.gameObject.CompareTag("Vis"))
					return;

				DataPoint tappedDataPoint = hit.collider.gameObject.GetComponentInParent<Visualization>().DataPoint;

				if (tappedDataPoint == SelectedDataPoint) {
					RemoveSelection();
					return;
				}

				if (SelectedDataPoint != null)
					RemoveSelection();

				SelectedDataPoint = tappedDataPoint;

				if (SoftSelectedDataPoint == SelectedDataPoint)
					SoftSelectedDataPoint = null;

				SelectedDataPoint.SetSelectionStatus(true, false);
				return;

			}

			if (SelectedDataPoint != null)
				RemoveSelection();
		}

		public void SelectDataPointOnVisualizationChange() {
			PreviouslySelectedDataPoint = SelectedDataPoint;

			if (PreviouslySelectedDataPoint != null) {
				onVisChangeWithSelection?.Invoke(true);
				StartCoroutine(SelectPreviouslySelectedDataPoint());
			}
		}

		private void RemoveSelection() {
			SelectedDataPoint.SetSelectionStatus(false, false);
			SelectedDataPoint = null;
		}

		private IEnumerator SelectPreviouslySelectedDataPoint() {
			yield return new WaitForEndOfFrame();
			SelectedDataPoint = PreviouslySelectedDataPoint;
			SelectedDataPoint.SetSelectionStatus(true, false);
			onVisChangeWithSelection?.Invoke(false);
		}

#endregion

	}

}