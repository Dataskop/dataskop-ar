using System.Collections;
using System.Collections.Generic;
using DataSkopAR.Utils;
using UnityEngine;

namespace DataSkopAR.Entities {

	/// <summary>
	///     Responsible for aligning the AR Worlds forward axis to North of earth.
	/// </summary>
	public class NorthAlignmentCalibrator : MonoBehaviour {

#region Fields

		[Header("References")]
		[SerializeField] private Transform mapTransform;
		[SerializeField] private Transform arCameraTransform;

		[Header("Values")]
		[SerializeField] private int rotationSamples = 10;

		private readonly WaitForSeconds timeBetweenSteps = new(0.025f);

#endregion

#region Methods

		public void Calibrate() {
			StartCoroutine(Rotate());
		}

		private IEnumerator Rotate() {

			List<double> calculatedAngles = new();

			if (Input.compass.enabled) {

				for (int i = 0; i < rotationSamples; i++) {

					if (Input.compass.headingAccuracy < 0)
						ErrorHandler.ThrowError(300, this);

					calculatedAngles.Add(CalculateRotationAngle());
					yield return timeBetweenSteps;
				}

				float finalAngle = (float)MathExtensions.MeanAngle(calculatedAngles.ToArray());
				mapTransform.Rotate(Vector3.up, finalAngle);
			}
			else {
				yield break;
			}

			yield return new WaitForEndOfFrame();

		}

		private double CalculateRotationAngle() {
			float mapToCamAngle = MathExtensions.GetSignedAngleOnAxis(arCameraTransform, mapTransform, Vector3.up);
			float calcAngle = 360f - (Input.compass.trueHeading + mapToCamAngle);
			return calcAngle;
		}

#endregion

	}

}