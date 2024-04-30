﻿#nullable enable

namespace DataskopAR.Data {

	public sealed class UserData {

		private UserData() { }

		public string? Token { get; set; }

		public static UserData Instance { get; } = new();

	}

}