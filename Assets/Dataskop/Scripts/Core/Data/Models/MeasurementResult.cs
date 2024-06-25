using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Dataskop.Data {

	[UsedImplicitly]
	public class MeasurementResult {

		/// <summary>
		///     Creates and returns a MeasurementResult from a given DTO.
		/// </summary>
		public MeasurementResult(string value, int valueType, string timeStamp, Position location, MeasurementDefinition md,
			string additionalProperties) {
			Value = value;
			ValueType = valueType;
			Timestamp = DateTime.Parse(timeStamp);
			Position = location;
			MeasurementDefinition = md;

			try {

				//string fixedProperties = additionalProperties.Substring(1, additionalProperties.Length - 2);

				AdditionalMeasurementResultsProperties properties =
					JsonConvert.DeserializeObject<AdditionalMeasurementResultsProperties>(additionalProperties);

				Author = properties.Author;

			}
			catch {

				Author = string.Empty;

			}

		}

		public class AdditionalMeasurementResultsProperties {

			public string Author { get; }

			public AdditionalMeasurementResultsProperties(string author) {
				Author = author;
			}

		}

		public string Value { get; }

		public int ValueType { get; }

		public DateTime Timestamp { get; }

		public Position Position { get; }

		private MeasurementDefinition MeasurementDefinition { get; }

		public string Author { get; }

		/// <summary>
		///     Returns the measurement result's value as a double (if suitable).
		/// </summary>
		public double ReadAsDouble() {

			if (MeasurementDefinition != null && MeasurementDefinition.MeasurementType != MeasurementType.Float)
				throw new InvalidOperationException("Value type not suitable.");

			return double.Parse(Value, CultureInfo.InvariantCulture);

		}

		/// <summary>
		///     Returns the measurement result's value as a float (if suitable).
		/// </summary>
		public float ReadAsFloat() {

			if (MeasurementDefinition != null && MeasurementDefinition.MeasurementType != MeasurementType.Float) {
				throw new InvalidOperationException("Value type not suitable.");
			}

			if (float.TryParse(Value, NumberStyles.Float, CultureInfo.InvariantCulture, out float value)) {
				return value;
			}

			return float.Epsilon;

		}

		/// <summary>
		///     Returns the measurement result's value as a string (if suitable).
		/// </summary>
		public string ReadAsString() {

			if (MeasurementDefinition != null && MeasurementDefinition.MeasurementType != MeasurementType.String)
				throw new InvalidOperationException("Value type not suitable.");

			return Value;

		}

		/// <summary>
		///     Returns the measurement result's value as a bool (if suitable).
		/// </summary>
		public bool ReadAsBool() {

			if (MeasurementDefinition != null && MeasurementDefinition.MeasurementType != MeasurementType.Bool)
				throw new InvalidOperationException("Value type not suitable.");

			return bool.Parse(Value);

		}

		public string GetTime() {
			return $"{GetDate()}";
		}

		public string GetDate() {
			return $"{Timestamp.ToString(new CultureInfo("de-DE"))}";
		}

		public string GetClockTime() {
			return $"{Timestamp.ToLongTimeString()}";
		}

		/// <summary>
		///     Returns an interpolated value for a given timestamp.
		/// </summary>
		public static float GetValueAtTime(List<MeasurementResult> mResults, DateTime time, string strategy) {
			List<string> validStrategies = new() {
				"steps",
				"average",
				"linear_interpolation"
			};

			if (!validStrategies.Contains(strategy))
				throw new ArgumentOutOfRangeException(nameof(strategy), "Invalid strategy");

			if (mResults.Count == 0)
				throw new ArgumentOutOfRangeException(nameof(mResults), "No measurement results provided");

			List<MeasurementResult> sortedMResults = mResults.OrderBy(item => item.Timestamp).ToList();
			MeasurementResult leftItem = null;
			MeasurementResult rightItem = null;

			// Find nearest item on the left
			for (int i = 0; i < sortedMResults.Count; i++) {
				MeasurementResult current = sortedMResults[i];

				if (current.Timestamp == time) return current.ReadAsFloat();

				if (current.Timestamp > time) break;

				leftItem = current;
			}

			// Find nearest item on the right
			for (int i = sortedMResults.Count - 1; i >= 0; i--) {
				MeasurementResult current = sortedMResults[i];

				if (current.Timestamp < time) break;

				rightItem = current;
			}

			// Return first value if there is no left.
			if (leftItem == null) return rightItem.ReadAsFloat();

			// Return last item if there is no right.
			if (rightItem == null) return leftItem.ReadAsFloat();

			if (strategy == "average") return (leftItem.ReadAsFloat() + rightItem.ReadAsFloat()) / 2;

			if (strategy == "linear_interpolation") {
				float valueDistanceLeftRight = rightItem.ReadAsFloat() - leftItem.ReadAsFloat();
				double factor = (time - leftItem.Timestamp) / (rightItem.Timestamp - leftItem.Timestamp);
				float factorFloat = (float)factor;
				return leftItem.ReadAsFloat() + valueDistanceLeftRight * factorFloat;
			}

			// Steps result (is also the fallback)
			return leftItem.ReadAsFloat();
		}

	}

}