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
		[SerializeField] private GameObject noResultsIndicator;

		[Header("Values")]
		[SerializeField] private Color mapSelectionColor;
		[SerializeField] private Color mapHoverColor;
		[SerializeField] private Color mapDefaultColor;

		private MeasurementResultRange currentMeasurementRange;

		public MeasurementDefinition MeasurementDefinition { get; set; }

		public MeasurementResultRange CurrentMeasurementRange {

			get => currentMeasurementRange;

			private set {
				currentMeasurementRange = value;
				MeasurementRangeChanged?.Invoke();
			}

		}

		public int FocusedIndex { get; private set; }
		
		public int MeasurementCount => 

		public MeasurementResult FocusedMeasurement { get; set; }

		public DataAttribute Attribute { get; set; }

		public IVisualization Vis { get; private set; }

		public Device Device { get; set; }

		public AuthorRepository AuthorRepository { get; set; }

		private void Awake() {
			VisualizationTypeChanged += OnVisChanged;
		}

		public event Action<int> FocusedIndexChanged;

		public event Action<MeasurementResult> FocusedMeasurementResultChanged;

		public event Action<int> FocusedIndexChangedByTap;

		public event Action<VisualizationType> VisualizationTypeChanged;

		public event Action MeasurementRangeChanged;

		/// <summary>
		///     Sets and replaces the current visualization form with another.
		/// </summary>
		/// <param name="visPrefab">The visualization to be used for this data point.</param>
		/// <param name="timeRange">The time range of the data that should be visualized.</param>
		public void Visualize(GameObject visPrefab, TimeRange? timeRange) {

			Vis = Instantiate(visPrefab, transform).GetComponent<IVisualization>();

			CurrentMeasurementRange = timeRange == null
				? MeasurementDefinition.GetLatestRange()
				: MeasurementDefinition.GetRange(timeRange.Value);

			Vis.Initialize(this);

			if (CurrentMeasurementRange.Count < 1) {
				return;
			}

			FocusedIndexChanged += Vis.OnFocusedIndexChanged;
			Vis.SwipedUp += DecreaseMeasurementIndex;
			Vis.SwipedDown += IncreaseMeasurementIndex;
			Vis.VisObjectSelected += OnVisObjectSelected;
			VisualizationTypeChanged?.Invoke(Vis.Type);
		}

		public void RemoveVisualization() {

			if (Vis == null) {
				return;
			}

			FocusedIndexChanged -= Vis.OnFocusedIndexChanged;
			Vis.SwipedUp -= DecreaseMeasurementIndex;
			Vis.SwipedDown -= IncreaseMeasurementIndex;
			Vis.VisObjectSelected -= OnVisObjectSelected;
			Vis.Despawn();

		}

		public void OnMeasurementResultsUpdated() {

			CurrentMeasurementRange = MeasurementDefinition.GetLatestRange();

			int indexOfPreviousResult = CurrentMeasurementRange.IndexOf(FocusedMeasurement);

			if (Vis.HasHistoryEnabled) {
				SetIndex(indexOfPreviousResult);
			}
			else {
				SetIndex(FocusedIndex != 0 ? indexOfPreviousResult : 0);
			}

		}

		public void UpdateWithTimeRange(TimeRange timeRange) {

			if (MeasurementDefinition.GetRange(timeRange) == null) {
				return;
			}

			CurrentMeasurementRange = MeasurementDefinition.GetRange(timeRange);
			SetIndexWithoutNotify(0);
			Vis.OnMeasurementResultRangeUpdated();

		}

		public void ToggleHistory(bool newState) {
			
			if (Vis != null && Vis.VisOption.Style.IsTimeSeries) {
				Vis.OnTimeSeriesToggled(newState);
			}
		}

		private void IncreaseMeasurementIndex() {

			if (MeasurementDefinition.MeasurementResults == null) {
				return;
			}

			if (CurrentMeasurementRange.Count < 1) {
				return;
			}

			//TODO: Temporary second condition because no loading of additional data is happening right now.
			if (FocusedIndex == CurrentMeasurementRange.Count - 1 || FocusedIndex == Vis.VisHistoryConfiguration.visibleHistoryCount - 1) {
				return;
			}

			SetIndex(FocusedIndex + 1);

		}

		private void DecreaseMeasurementIndex() {

			if (MeasurementDefinition.MeasurementResults == null) {
				return;
			}

			if (CurrentMeasurementRange.Count < 1) {
				return;
			}

			if (FocusedIndex == 0) {
				return;
			}

			SetIndex(FocusedIndex - 1);
		}

		private void OnVisObjectSelected(int index) {

			if (index == FocusedIndex) {
				return;
			}

			FocusedIndexChangedByTap?.Invoke(index);

		}

		public void SetIndex(int index) {

			if (CurrentMeasurementRange.Count < 1) {
				return;
			}

			FocusedIndex = index;
			FocusedMeasurement = CurrentMeasurementRange[index];
			FocusedIndexChanged?.Invoke(FocusedIndex);
			FocusedMeasurementResultChanged?.Invoke(FocusedMeasurement);
		}

		public void SetIndexWithoutNotify(int index) {

			if (CurrentMeasurementRange.Count < 1) {
				return;
			}

			FocusedIndex = index;
			FocusedMeasurement = CurrentMeasurementRange[index];
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

			if (mapIconBorder == null) {
				return;
			}

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