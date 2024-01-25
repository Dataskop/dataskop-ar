using System.Collections;
using System.Collections.Generic;
using DataskopAR.Interaction;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace DataskopAR.Tests {

	public class CalibratorTests {

		[Test]
		public void CalibratorPhaseIsNotNoneAfterInitialization() {

			GameObject testObject = new GameObject();
			Calibrator calibrator = GameObject.Instantiate(testObject).AddComponent<Calibrator>();
			calibrator.Initialize();

			Assert.AreNotEqual(calibrator.CurrentPhase, CalibratorPhase.None);

		}

		[Test]
		public void CalibratorPhaseIsNoneAfterCompletion() {

			GameObject testObject = new GameObject();
			Calibrator calibrator = GameObject.Instantiate(testObject).AddComponent<Calibrator>();
			calibrator.Initialize();

			while (calibrator.IsCalibrating) {
				calibrator.OnCalibratorContinued();
			}

			Assert.AreEqual(calibrator.CurrentPhase, CalibratorPhase.None);

		}

	}

}