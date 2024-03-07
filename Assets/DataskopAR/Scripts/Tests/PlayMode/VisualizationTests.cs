using System.Collections;
using DataskopAR.Entities.Visualizations;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace DataskopAR.PlayMode.Tests {

	public class VisualizationTests {

		// A Test behaves as an ordinary method
		[Test]
		public void VisualizationTestsSimplePasses() {
			// Use the Assert class to test conditions
		}

		// A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
		// `yield return null;` to skip a frame.
		[UnityTest]
		public IEnumerator Visualization_Is_Selected_When_Tapping_On_It() {

			yield return null;
		}

	}

}