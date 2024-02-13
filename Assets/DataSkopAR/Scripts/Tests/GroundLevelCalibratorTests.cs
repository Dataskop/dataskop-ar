using DataskopAR.Interaction;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace DataskopAR.Tests {

	[TestFixture]
	public class GroundLevelCalibratorTests {

		[Test]
		public void GroundLevelIsLowerThanInitialYPositionWhenLowerPlaneIsFound() {

			GameObject testObject = new();
			GroundLevelCalibrator calibrator = Object.Instantiate(testObject).AddComponent<GroundLevelCalibrator>();
			calibrator.GroundLevelYPosition = 0;
			ARPlane foundPlane = Object.Instantiate(testObject).AddComponent<ARPlane>();
			foundPlane.transform.position = new Vector3(0, -1, 0);

			calibrator.SetLowestPlaneFound(foundPlane);

			Assert.Negative(calibrator.GroundLevelYPosition);

		}

		[Test]
		public void FoundPlaneGetsDiscardedWhenYPosLimitIsExceeded() {

			GameObject testObject = new();
			GroundLevelCalibrator calibrator = Object.Instantiate(testObject).AddComponent<GroundLevelCalibrator>();
			calibrator.GroundLevelYPosition = 0;
			ARPlane foundPlane = Object.Instantiate(testObject).AddComponent<ARPlane>();
			foundPlane.transform.position = new Vector3(0, -4, 0);

			calibrator.SetLowestPlaneFound(foundPlane);

			Assert.True(calibrator.GroundLevelYPosition == 0);

		}

	}

}