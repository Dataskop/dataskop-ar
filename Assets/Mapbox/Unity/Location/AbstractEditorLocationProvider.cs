﻿namespace Mapbox.Unity.Location {

	using System.Collections;
	using UnityEngine;

	public abstract class AbstractEditorLocationProvider : AbstractLocationProvider {

		[SerializeField]
		protected int _accuracy;

		[SerializeField] private bool _autoFireEvent;

		[SerializeField] private float _updateInterval;

		[SerializeField] private bool _sendEvent;

		private WaitForSeconds _wait = new(0);

#if UNITY_EDITOR
		protected virtual void Awake() {
			_wait = new WaitForSeconds(_updateInterval);
			StartCoroutine(QueryLocation());
		}
#endif

		private IEnumerator QueryLocation() {
			// HACK: Let others register before we send our first event. 
			// Often this happens in Start.
			yield return new WaitForSeconds(.1f);

			while (true) {
				SetLocation();

				if (_autoFireEvent) {
					SendLocation(_currentLocation);
				}

				yield return _wait;
			}
		}


		// Added to support TouchCamera script. 
		public void SendLocationEvent() {
			SetLocation();
			SendLocation(_currentLocation);
		}


		protected virtual void OnValidate() {
			if (_sendEvent) {
				_sendEvent = false;
				SendLocation(_currentLocation);
			}
		}

		protected abstract void SetLocation();

	}

}