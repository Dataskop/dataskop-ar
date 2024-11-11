namespace Mapbox.Unity.Telemetry {

	public class TelemetryDummy : ITelemetryLibrary {

		private static ITelemetryLibrary _instance = new TelemetryDummy();

		public static ITelemetryLibrary Instance => _instance;

		public void Initialize(string accessToken) {
			// empty.
		}

		public void SendTurnstile() {
			// empty.
		}

		public void SetLocationCollectionState(bool enable) {
			// empty.
		}

	}

}