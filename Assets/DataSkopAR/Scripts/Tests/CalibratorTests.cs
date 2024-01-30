using DataskopAR.Interaction;
using NUnit.Framework;
using UnityEngine;

namespace DataskopAR.Tests {

	public class CalibratorTests {

		[Test]
		public void CalibratorPhaseIsNotNoneAfterInitialization() {

			// Arrange
			GameObject testObject = new GameObject();
			Calibrator calibrator = GameObject.Instantiate(testObject).AddComponent<Calibrator>();

			// Act
			calibrator.Initialize();

			// Assert
			Assert.AreNotEqual(calibrator.CurrentPhase, CalibratorPhase.None);

		}

		[Test]
		public void CalibratorPhaseIsNoneAfterCompletion() {

			// Arrange
			GameObject testObject = new GameObject();
			Calibrator calibrator = GameObject.Instantiate(testObject).AddComponent<Calibrator>();

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