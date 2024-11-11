using System;
using Dataskop.Data;
using Dataskop.Utils;
using UnityEngine;

namespace Dataskop.Entities.Visualizations {

	public class BarVisObject : MonoBehaviour, IVisObject {

		[Header("References")]
		[SerializeField] private BoxCollider visCollider;
		[SerializeField] private MeshRenderer barFillMeshRenderer;
		[SerializeField] private MeshRenderer barFrameMeshRenderer;
		[SerializeField] private Transform barFill;
		[SerializeField] private Transform barFrame;
		[SerializeField] private Material defaultFrameMaterial;
		[SerializeField] private Material hoveredFrameMaterial;
		[SerializeField] private Material selectedFrameMaterial;
		[SerializeField] private Material focusFillMaterial;
		[SerializeField] private Material historicFillMaterial;

		private Vector3 animationTarget;
		private bool isSelected;

		public Transform VisObjectTransform => transform;

		public VisObjectData CurrentData { get; private set; }

		public int Index { get; set; }

		public bool IsFocused { get; set; }

		public Collider VisCollider => visCollider;

		public event Action<int> HasHovered;

		public event Action<int> HasSelected;

		public event Action<int> HasDeselected;

		public void OnHover() {
			HasHovered?.Invoke(Index);
		}

		public void OnSelect() {
			HasSelected?.Invoke(Index);
		}

		public void OnDeselect() {
			HasDeselected?.Invoke(Index);
		}

		public void OnHistoryToggle(bool active) {
			Rotate(active);
		}

		public void ChangeState(VisObjectState newState) {
			switch (newState) {

				case VisObjectState.Deselected:

					if (isSelected) {
						isSelected = false;
					}

					barFrameMeshRenderer.material = defaultFrameMaterial;

					break;
				case VisObjectState.Hovered:

					if (isSelected && IsFocused) {
						return;
					}

					if (IsFocused) {
						barFrameMeshRenderer.material = hoveredFrameMaterial;
					}

					break;
				case VisObjectState.Selected:
					isSelected = true;
					barFrameMeshRenderer.material = selectedFrameMaterial;
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
			}
		}

		public void ApplyData(params VisObjectData[] data) {

			CurrentData = data[0];

			switch (CurrentData.Type) {
				case MeasurementType.Float:
				{
					SetPillarHeight(
						CurrentData.Result.ReadAsFloat(), CurrentData.Attribute.Minimum, CurrentData.Attribute.Maximum,
						0.01f,
						barFrame.localScale.y
					);
					break;
				}
				case MeasurementType.Bool:
				{
					SetPillarHeight(
						CurrentData.Result.ReadAsBool() ? 1 : 0, CurrentData.Attribute.Minimum,
						CurrentData.Attribute.Maximum,
						0.01f,
						barFrame.localScale.y
					);
					break;
				}
			}

		}

		public void SetFocus(bool isFocused) {
			IsFocused = isFocused;
			barFillMeshRenderer.material = IsFocused ? focusFillMaterial : historicFillMaterial;

		}

		public void Delete() {
			Destroy(gameObject);
		}

		public Vector3 GetCurrentScale() {
			return barFrame.localScale;
		}

		private void Rotate(bool isRotated) {
			transform.localRotation = isRotated ?
				Quaternion.Euler(0, 0, -90) :
				Quaternion.Euler(0, 0, 0);
		}

		private void SetPillarHeight(float heightValue, float minValue, float maxValue, float minBarHeight,
			float maxBarHeight) {
			heightValue = Mathf.Clamp(heightValue, minValue, maxValue);
			float barHeight = MathExtensions.Map(heightValue, minValue, maxValue, minBarHeight, maxBarHeight);
			Vector3 localScale = barFill.localScale;
			Vector3 barFillScale = new(localScale.x, barHeight, localScale.z);
			localScale = barFillScale;
			barFill.localScale = localScale;
		}

	}

}