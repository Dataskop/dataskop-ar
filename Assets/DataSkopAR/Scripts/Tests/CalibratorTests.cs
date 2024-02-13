using DataskopAR.Interaction;
using NUnit.Framework;
using UnityEngine;

namespace DataskopAR.Tests {

	[TestFixture]
	public class CalibratorTests {

		[Test]
		public void CalibratorPhaseIsNotNoneAfterInitialization() {

			GameObject testObject = new();
			Calibrator calibrator = Object.Instantiate(testObject).AddComponent<Calibrator>();

			calibrator.Initialize();

			Assert.AreNotEqual(calibrator.CurrentPhase, CalibratorPhase.None);

		}

		[Test]
		public void CalibratorPhaseIsNoneAfterCompletion() {

			// Arrange
			GameObject testObject = new();
			Calibrator calibrator = Object.Instantiate(testObject).AddComponent<Calibrator>();

			// Act
			calibrator.Initialize();

			while (calibrator.IsCalibrating) {
				calibrator.OnCalibratorContinued();
			}

			// Assert
			Assert.AreEqual(calibrator.CurrentPhase, CalibratorPhase.None);

		}

	}

}