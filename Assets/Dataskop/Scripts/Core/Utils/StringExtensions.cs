namespace Dataskop {

	public static class StringExtensions {

		public static string FirstCharToUpper(this string input) {

			if (string.IsNullOrEmpty(input)) {
				return string.Empty;
			}

			char[] a = input.ToCharArray();
			a[0] = char.ToUpper(a[0]);
			return new string(a);
		}

	}

}