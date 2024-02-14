using DataskopAR.Interaction;
using NUnit.Framework;
using UnityEngine;

namespace DataskopAR.Tests {

	[TestFixture]
	public class CalibratorTests {

		[Test]
		public void Calibrator_Phase_Is_Not_None_After_Initialization() {

			Calibrator calibrator = CreateCalibrator();

			calibrator.Initialize();

			Assert.AreNotEqual(calibrator.CurrentPhase, CalibratorPhase.None);

		}

		[Test]
		public void Calibrator_Phase_Is_None_After_Completion() {

			// Arrange
			Calibrator calibrator = CreateCalibrator();

			// Act
			calibrator.Initialize();

			while (calibrator.IsCalibrating) {
				calibrator.OnCalibratorContinued();
			}

			// Assert
			Assert.AreEqual(calibrator.CurrentPhase, CalibratorPhase.None);

		}

		// Test Fixtures
		private static Calibrator CreateCalibrator() {
			return Object.Instantiate(new GameObject()).AddComponent<Calibrator>();
		}

	}

}