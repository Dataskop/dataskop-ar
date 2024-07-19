using System;
using Dataskop.Data;
using Dataskop.Entities.Visualizations;
using Dataskop.Interaction;
using UnityEngine;

namespace Dataskop.Entities {

	public class DataPoint : MonoBehaviour {

		[Header("References")]
		[SerializeField] private SpriteRenderer mapIconBorder;
		[SerializeField] private SpriteRenderer visIcon;
		[SerializeField] private Sprite[] visIcons;

		[Header("Values")]
		[SerializeField] private Color mapSelectionColor;
		[SerializeField] private Color mapHoverColor;
		[SerializeField] private Color mapDefaultColor;

		public MeasurementDefinition MeasurementDefinition { get; set; }

		public int FocusedIndex { get; set; }

		//TODO: Check for Usages of this if necessary to refactor to event subscription
		public MeasurementResult FocusedMeasurement => MeasurementDefinition.GetMeasurementResult(FocusedIndex);

		public DataAttribute Attribute { get; set; }

		public IVisualization Vis { get; private set; }

		public Device Device { get; set; }

		public AuthorRepository AuthorRepository { get; set; }

		private void Awake() {
			VisualizationTypeChanged += OnVisChanged;
		}

		public event Action<MeasurementDefinition, int> FocusedIndexChanged;

		public event Action<int> FocusedIndexChangedByTap;

		public event Action<VisualizationType> VisualizationTypeChanged;

		/// <summary>
		///     Sets and replaces the current visualization form with another.
		/// </summary>
		/// <param name="visPrefab">The visualization to be used for this data point.</param>
		public void SetVis(GameObject visPrefab) {
			Vis = Instantiate(visPrefab, transform).GetComponent<IVisualization>();
			Vis.Initialize(this);
			FocusedIndexChanged += Vis.OnFocusedIndexChanged;
			Vis.SwipedUp += DecreaseMeasurementIndex;
			Vis.SwipedDown += IncreaseMeasurementIndex;
			Vis.VisObjectSelected += OnVisObjectSelected;
			VisualizationTypeChanged?.Invoke(Vis.Type);
		}

		public void RemoveVis() {

			if (Vis == null) {
				return;
			}

			FocusedIndexChanged -= Vis.OnFocusedIndexChanged;
			Vis.SwipedUp -= DecreaseMeasurementIndex;
			Vis.SwipedDown -= IncreaseMeasurementIndex;
			Vis.Despawn();

		}

		public void OnMeasurementResultsUpdated() {

			//TODO: What happens when Measurement Results get updated (either through request or auto refetch)...

		}

		private void IncreaseMeasurementIndex() {

			if (MeasurementDefinition.MeasurementResults == null) {
				return;
			}

			//TODO: Temporary second condition because no loading of additional data is happening right now.
			if (FocusedIndex == MeasurementDefinition.MeasurementResults.Count - 1 ||
			    FocusedIndex == Vis.VisHistoryConfiguration.visibleHistoryCount - 1) {
				return;
			}

			FocusedIndex++;
			FocusedIndexChanged?.Invoke(MeasurementDefinition, FocusedIndex);

		}

		private void DecreaseMeasurementIndex() {

			if (MeasurementDefinition.MeasurementResults == null) {
				return;
			}

			if (FocusedIndex == 0) {
				return;
			}

			FocusedIndex--;
			FocusedIndexChanged?.Invoke(MeasurementDefinition, FocusedIndex);
		}

		private void OnVisObjectSelected(int index) {

			if (index == FocusedIndex) {
				return;
			}

			FocusedIndexChangedByTap?.Invoke(index);

		}

		public void SetIndex(int index) {

			if (index == FocusedIndex) {
				return;
			}

			FocusedIndex = index;
			FocusedIndexChanged?.Invoke(MeasurementDefinition, FocusedIndex);

		}

		/// <summary>
		///     Sets the selection status of the data point.
		/// </summary>
		public void SetSelectionStatus(SelectionState state) {

			switch (state) {

				case SelectionState.Deselected:
					SetMapIconColor(mapDefaultColor);
					break;
				case SelectionState.Hovered:
					SetMapIconColor(mapHoverColor);
					break;
				case SelectionState.Selected:
					SetMapIconColor(mapSelectionColor);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(state), state, null);
			}

		}

		private void SetMapIconColor(Color color) {
			mapIconBorder.color = color;
		}

		private void OnVisChanged(VisualizationType visType) {

			visIcon.sprite = visType switch {
				VisualizationType.Dot => visIcons[0],
				VisualizationType.Bubble => visIcons[1],
				VisualizationType.Bar => visIcons[2],
				_ => throw new ArgumentOutOfRangeException(nameof(visType), visType, null)
			};

		}

	}

}