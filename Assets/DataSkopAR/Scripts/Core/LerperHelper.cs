using System;
using System.Collections;
using JetBrains.Annotations;
using UnityEngine;

namespace DataSkopAR {

	public static class LerperHelper {

		public static IEnumerator TransformLerp(Transform t, TransformValue tValue, Vector3 origin, Vector3 target, float duration,
			[CanBeNull] Action callback) {

			float current = 0f;

			while (current <= duration) {
				current += Time.deltaTime;
				float currentPercentage = Mathf.Clamp01(current / duration);

				switch (tValue) {

					case TransformValue.Position:
						t.localPosition = Vector3.LerpUnclamped(origin, target, currentPercentage);
						break;
					case TransformValue.Scale:
						t.localScale = Vector3.LerpUnclamped(origin, target, currentPercentage);
						break;
					case TransformValue.Rotation:
						t.localRotation = Quaternion.LerpUnclamped(Quaternion.Euler(origin), Quaternion.Euler(target), currentPercentage);
						break;
					default:
						throw new ArgumentOutOfRangeException(nameof(tValue), tValue, null);
				}

				yield return null;

			}

			callback?.Invoke();
		}

		public static IEnumerator TransformLerpOnCurve(Transform t, TransformValue tValue, Vector3 origin, Vector3 target, float duration, AnimationCurve curve,
			[CanBeNull] Action callback) {

			float current = 0f;

			while (current <= duration) {

				current += Time.deltaTime;
				float currentPercentage = Mathf.Clamp01(current / duration);
				float curvePercentage = curve.Evaluate(currentPercentage);

				switch (tValue) {

					case TransformValue.Position:
						t.localPosition = Vector3.LerpUnclamped(origin, target, curvePercentage);
						break;
					case TransformValue.Scale:
						t.localScale = Vector3.LerpUnclamped(origin, target, curvePercentage);
						break;
					case TransformValue.Rotation:
						t.localRotation = Quaternion.LerpUnclamped(Quaternion.Euler(origin), Quaternion.Euler(target), curvePercentage);
						break;
					default:
						throw new ArgumentOutOfRangeException(nameof(tValue), tValue, null);
				}

				yield return null;
			}

			callback?.Invoke();

		}

	}

	public enum TransformValue {

		Position,
		Scale,
		Rotation

	}

}