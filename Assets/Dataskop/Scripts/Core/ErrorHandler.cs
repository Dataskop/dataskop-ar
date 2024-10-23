using System;
using System.Collections.Generic;
using System.Text;

namespace Dataskop {

	/// <summary>
	/// Class for receiving Error Codes, keeping track of received errors and sending out an event when
	/// receiving Errors.
	/// </summary>
	public static class ErrorHandler {

#region ErrorType enum

		public enum ErrorType {

			Tip,
			Warning,
			Error

		}

#endregion

		private static readonly List<Error> ErrorList = new() {
			new Error {
				ErrorCode = 100,
				ErrorMessage = "No data points in view - move your camera to see data around you.",
				Type = ErrorType.Tip
			},
			new Error {
				ErrorCode = 101,
				ErrorMessage = "Select a data point to pin it to the information card.",
				Type = ErrorType.Tip
			},
			new Error {
				ErrorCode = 200,
				ErrorMessage = "Data from this data point was not updated in the past 32 hours",
				Type = ErrorType.Warning
			},
			new Error {
				ErrorCode = 201,
				ErrorMessage = "Initial GPS data inaccurate! Move around to get better position data.",
				Type = ErrorType.Warning
			},
			new Error {
				ErrorCode = 300,
				ErrorMessage = "Compass data is unreliable! Please move around and re-calibrate.",
				Type = ErrorType.Error
			},
			new Error {
				ErrorCode = 301,
				ErrorMessage = "World alignment is off! Please move around and re-calibrate.",
				Type = ErrorType.Error
			}
		};

		private static readonly Error InvalidError = new() {
			ErrorCode = -1,
			ErrorMessage = "Error Not Found! Possible Error Codes: " + GetAvailableErrorCodes(),
			Type = ErrorType.Error
		};

		/// <summary>
		/// Collected Errors
		/// </summary>
		public static Queue<Error> ErrorQueue { get; } = new();

		public static event EventHandler<ErrorReceivedEventArgs> OnErrorReceived;

		/// <summary>
		/// Throws an error with the given error code.
		/// </summary>
		/// <param name="errorCode">The error code for the error to be thrown.</param>
		/// <param name="sender">The object of the cause of the error.</param>
		public static void ThrowError(int errorCode, object sender) {
			Error thrownError = GetError(errorCode);
			thrownError.TimeStamp = DateTime.Now;
			ErrorQueue.Enqueue(thrownError);
			OnErrorReceived?.Invoke(sender, new ErrorReceivedEventArgs {
				Error = thrownError
			});
		}

		/// <summary>
		/// Throws an error with the given error code and an additional value.
		/// </summary>
		/// <param name="errorCode">The error code for the error to be thrown.</param>
		/// <param name="value">An additional value as float.</param>
		/// <param name="sender">The object of the cause of the error.</param>
		public static void ThrowError(int errorCode, float value, object sender) {
			Error thrownError = GetError(errorCode);
			thrownError.TimeStamp = DateTime.Now;
			thrownError.Value = value;
			ErrorQueue.Enqueue(thrownError);
			OnErrorReceived?.Invoke(sender, new ErrorReceivedEventArgs {
				Error = thrownError
			});
		}

		/// <summary>
		/// Gets the error for the given error code. Returns an "Error Not Found"-Error when wrong error
		/// code was entered.
		/// </summary>
		/// <param name="errorCode">The error code used to retrieve the corresponding error.</param>
		/// <returns>The <see cref="Error" /> with the given error code.</returns>
		private static Error GetError(int errorCode) {
			return !ErrorList.Exists(error => error.ErrorCode == errorCode)
				? InvalidError
				: ErrorList.Find(error => error.ErrorCode == errorCode);
		}

		private static string GetAvailableErrorCodes() {
			StringBuilder codeString = new();

			foreach (Error error in ErrorList)
				codeString.Append($" ({error.ErrorCode}) ");

			return codeString.ToString();
		}

#region Nested type: Error

		public struct Error {

			public int ErrorCode { get; set; }

			public string ErrorMessage { get; set; }

			public ErrorType Type { get; set; }

			public DateTime TimeStamp { get; set; }

			public float? Value { get; set; }

			public new string ToString() {
				return $"[{TimeStamp.Hour}:{TimeStamp.Minute}:{TimeStamp.Second}] - {ErrorCode}: {ErrorMessage}";
			}

		}

#endregion

#region Nested type: ErrorReceivedEventArgs

		public class ErrorReceivedEventArgs : EventArgs {

			public Error Error { get; set; }

		}

#endregion

	}

}