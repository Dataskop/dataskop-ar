using System;
using System.Collections;
using System.Collections.Generic;
using DataskopAR.Utils;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

namespace DataskopAR.Interaction {

	/// <summary>
	///     Responsible for aligning the AR Worlds forward axis to North of earth.
	/// </summary>
	public class NorthAlignmentCalibrator : MonoBehaviour, ICalibration {

#region Events

		public event Action CalibrationCompleted;

		[Header("Events")]
		public UnityEvent<int, int> rotationSampleTaken;

#endregion

#region Fields

		[Header("References")]
		[SerializeField] private Transform mapTransform;
		[SerializeField] private Transform arCameraTransform;

		[Header("Values")]
		[SerializeField] private int rotationSamples = 10;

		private WaitForSeconds timeBetweenSteps;

#endregion

#region Properties

		public bool IsEnabled { get; set; }

#endregion

#region Methods

		public ICalibration Enable() {

			StartCoroutine(Rotate());
			IsEnabled = true;
			return this;

		}

		private IEnumerator Rotate() {

			List<double> calculatedAngles = new();

			for (int i = 0; i < rotationSamples; i++) {

				if (Input.compass.headingAccuracy < 0) {
					ErrorHandler.ThrowError(300, this);
				}

				calculatedAngles.Add(CalculateRotationAngle());
				rotationSampleTaken?.Invoke(i, rotationSamples);
				timeBetweenSteps = new WaitForSeconds(Random.Range(0.005f, 0.225f));
				yield return timeBetweenSteps;
			}

			float finalAngle = (float)MathExtensions.MeanAngle(calculatedAngles.ToArray());
			mapTransform.Rotate(Vector3.up, finalAngle);

			CalibrationCompleted?.Invoke();

			yield return new WaitForEndOfFrame();

		}

		private double CalculateRotationAngle() {
			float mapToCamAngle = MathExtensions.GetSignedAngleOnAxis(arCameraTransform, mapTransform, Vector3.up);
			float calcAngle = 360f - (Input.compass.trueHeading + mapToCamAngle);
			return calcAngle;
		}

		public void Disable() {
			IsEnabled = false;
		}

#endregion

	}

}