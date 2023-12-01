using System.Collections;
using UnityEngine;

namespace DataSkopAR.Entities {

	public class Compass : MonoBehaviour {

#region Properties

		private float Heading { get; set; }

#endregion

#region Fields

		[Header("References")]
		[SerializeField] private Transform compassSymbolTransform;

		[Header("Values")]
		[SerializeField] private float rotationTime = 1f;

		private Coroutine rotationCoroutine;

#endregion

#region Methods

		private void Start() {
			rotationCoroutine = StartCoroutine(RotateToNorth(rotationTime));
		}

		public void StartRotating() {
			StopCoroutine(rotationCoroutine);
			rotationCoroutine = StartCoroutine(RotateToNorth(rotationTime));
		}

		public void ToggleCompass() {
			if (gameObject.activeSelf) {
				StopCoroutine(rotationCoroutine);
				gameObject.SetActive(false);
			}
			else {
				gameObject.SetActive(true);
				rotationCoroutine = StartCoroutine(RotateToNorth(rotationTime));
			}
		}

		private IEnumerator RotateToNorth(float rotatingTime) {

			while (Input.compass.enabled) {

				Heading = Input.compass.trueHeading;
				Vector3 eulerRotation = new Vector3(0, 0, Heading);
				Quaternion newQuaternion = Quaternion.Euler(eulerRotation);
				float timeElapsed = 0f;

				while (timeElapsed < rotationTime) {
					Quaternion spin = Quaternion.Slerp(compassSymbolTransform.rotation, newQuaternion, timeElapsed / rotatingTime);
					compassSymbolTransform.SetPositionAndRotation(compassSymbolTransform.position, spin);
					timeElapsed += Time.deltaTime;
					yield return new WaitForEndOfFrame();
				}

				compassSymbolTransform.SetPositionAndRotation(compassSymbolTransform.position, newQuaternion);

			}

			yield return null;

		}

#endregion

	}

}