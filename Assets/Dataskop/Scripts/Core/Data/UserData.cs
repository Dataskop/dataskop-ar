#nullable enable

namespace Dataskop.Data {

	public sealed class UserData {

		public string? Token { get; set; }

		public static UserData Instance { get; } = new();

	}

}
