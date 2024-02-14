using DataskopAR.Interaction;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace DataskopAR.Tests {

	[TestFixture][Category("Dataskop")]
	public class GroundLevelCalibratorTests {

		[Test]
		public void Ground_Level_Is_Lower_Than_Initial_Y_Position_When_Lower_Plane_Is_Found() {

			GameObject testObject = new();
			GroundLevelCalibrator calibrator = Object.Instantiate(testObject).AddComponent<GroundLevelCalibrator>();
			calibrator.GroundLevelYPosition = 0;
			ARPlane foundPlane = Object.Instantiate(testObject).AddComponent<ARPlane>();
			foundPlane.transform.position = new Vector3(0, -1, 0);

			calibrator.SetLowestPlaneFound(foundPlane);

			Assert.Negative(calibrator.GroundLevelYPosition);

		}

		[Test]
		public void Found_Plane_Gets_Discarded_When_Y_Pos_Limit_Is_Exceeded() {

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