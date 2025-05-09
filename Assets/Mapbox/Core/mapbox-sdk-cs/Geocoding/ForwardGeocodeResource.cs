//-----------------------------------------------------------------------
// <copyright file="ForwardGeocodeResource.cs" company="Mapbox">
//     Copyright (c) 2016 Mapbox. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Mapbox.Geocoding {

	using System;
	using System.Collections.Generic;
	using Utils;
	using UnityEngine;

	/// <summary> A forward geocode request. </summary>
	public sealed class ForwardGeocodeResource : GeocodeResource<string> {

		/// <summary>
		///     ISO 3166-1 alpha-2 country codes.
		///     See <see href="https://en.wikipedia.org/wiki/ISO_3166-1_alpha-2">for all options</see>.
		/// </summary>
		private static readonly List<string> CountryCodes = new() {
			"ad",
			"ae",
			"af",
			"ag",
			"ai",
			"al",
			"am",
			"ao",
			"aq",
			"ar",
			"as",
			"at",
			"au",
			"aw",
			"ax",
			"az",
			"ba",
			"bb",
			"bd",
			"be",
			"bf",
			"bg",
			"bh",
			"bi",
			"bj",
			"bl",
			"bm",
			"bn",
			"bo",
			"bq",
			"br",
			"bs",
			"bt",
			"bv",
			"bw",
			"by",
			"bz",
			"ca",
			"cc",
			"cd",
			"cf",
			"cg",
			"ch",
			"ci",
			"ck",
			"cl",
			"cm",
			"cn",
			"co",
			"cr",
			"cu",
			"cv",
			"cw",
			"cx",
			"cy",
			"cz",
			"de",
			"dj",
			"dk",
			"dm",
			"do",
			"dz",
			"ec",
			"ee",
			"eg",
			"eh",
			"er",
			"es",
			"et",
			"fi",
			"fj",
			"fk",
			"fm",
			"fo",
			"fr",
			"ga",
			"gb",
			"gd",
			"ge",
			"gf",
			"gg",
			"gh",
			"gi",
			"gl",
			"gm",
			"gn",
			"gp",
			"gq",
			"gr",
			"gs",
			"gt",
			"gu",
			"gw",
			"gy",
			"hk",
			"hm",
			"hn",
			"hr",
			"ht",
			"hu",
			"id",
			"ie",
			"il",
			"im",
			"in",
			"io",
			"iq",
			"ir",
			"is",
			"it",
			"je",
			"jm",
			"jo",
			"jp",
			"ke",
			"kg",
			"kh",
			"ki",
			"km",
			"kn",
			"kp",
			"kr",
			"kw",
			"ky",
			"kz",
			"la",
			"lb",
			"lc",
			"li",
			"lk",
			"lr",
			"ls",
			"lt",
			"lu",
			"lv",
			"ly",
			"ma",
			"mc",
			"md",
			"me",
			"mf",
			"mg",
			"mh",
			"mk",
			"ml",
			"mm",
			"mn",
			"mo",
			"mp",
			"mq",
			"mr",
			"ms",
			"mt",
			"mu",
			"mv",
			"mw",
			"mx",
			"my",
			"mz",
			"na",
			"nc",
			"ne",
			"nf",
			"ng",
			"ni",
			"nl",
			"no",
			"np",
			"nr",
			"nu",
			"nz",
			"om",
			"pa",
			"pe",
			"pf",
			"pg",
			"ph",
			"pk",
			"pl",
			"pm",
			"pn",
			"pr",
			"ps",
			"pt",
			"pw",
			"py",
			"qa",
			"re",
			"ro",
			"rs",
			"ru",
			"rw",
			"sa",
			"sb",
			"sc",
			"sd",
			"se",
			"sg",
			"sh",
			"si",
			"sj",
			"sk",
			"sl",
			"sm",
			"sn",
			"so",
			"sr",
			"ss",
			"st",
			"sv",
			"sx",
			"sy",
			"sz",
			"tc",
			"td",
			"tf",
			"tg",
			"th",
			"tj",
			"tk",
			"tl",
			"tm",
			"tn",
			"to",
			"tr",
			"tt",
			"tv",
			"tw",
			"tz",
			"ua",
			"ug",
			"um",
			"us",
			"uy",
			"uz",
			"va",
			"vc",
			"ve",
			"vg",
			"vi",
			"vn",
			"vu",
			"wf",
			"ws",
			"ye",
			"yt",
			"za",
			"zm",
			"zw"
		};

		// Required
		private string query;

		// Optional
		private bool? autocomplete;

		// Optional
		private string[] country;

		// Optional
		private Vector2d? proximity;

		// Optional
		private Vector2dBounds? bbox;

		/// <summary> Initializes a new instance of the <see cref="ForwardGeocodeResource" /> class.</summary>
		/// <param name="query"> Place name for forward geocoding. </param>
		public ForwardGeocodeResource(string query) {
			Query = query;
		}

		/// <summary> Gets or sets the place name for forward geocoding. </summary>
		public override string Query
		{
			get => query;

			set => query = value;
		}

		/// <summary> Gets or sets the autocomplete option. </summary>
		public bool? Autocomplete
		{
			get => autocomplete;

			set => autocomplete = value;
		}

		/// <summary>
		///     Gets or sets the bounding box option. Bounding box is a rectangle within which to
		///     limit results, given as <see cref="Bbox"/>.
		/// </summary>
		public Vector2dBounds? Bbox
		{
			get => bbox;

			set => bbox = value;
		}

		/// <summary>
		///     Gets or sets the country option. Country is an Array of ISO 3166 alpha 2 country codes.
		///     For all possible values, <see cref="CountryCodes"/>.
		/// </summary>
		public string[] Country
		{
			get => country;

			set
			{
				if (value == null) {
					country = value;
					return;
				}

				for (int i = 0; i < value.Length; i++) {
					// Validate that provided countries exist
					if (!CountryCodes.Contains(value[i])) {
						throw new Exception(
							"Invalid country shortcode. See https://en.wikipedia.org/wiki/ISO_3166-1_alpha-2."
						);
					}
				}

				country = value;
			}
		}

		/// <summary>
		///     Gets or sets the proximity option, which is a location around which to bias results,
		///     given as <see cref="Vector2d"/>.
		/// </summary>
		public Vector2d? Proximity
		{
			get => proximity;

			set => proximity = value;
		}

		/// <summary> Builds a forward geocode URL string. </summary>
		/// <returns> A complete, valid forward geocode URL. </returns>
		public override string GetUrl() {
			Dictionary<string, string> opts = new();

			if (Autocomplete != null) {
				opts.Add("autocomplete", Autocomplete.ToString().ToLower());
			}

			if (Bbox != null) {
				Vector2dBounds nonNullableBbox = (Vector2dBounds)Bbox;
				opts.Add("bbox", nonNullableBbox.ToString());
			}

			if (Country != null) {
				opts.Add("country", GetUrlQueryFromArray<string>(Country));
			}

			if (Proximity != null) {
				Vector2d nonNullableProx = (Vector2d)Proximity;
				opts.Add("proximity", nonNullableProx.ToString());
			}

			if (Types != null) {
				opts.Add("types", GetUrlQueryFromArray(Types));
			}

			// !!!!!!!!!! HACK !!!!!!!
			// we are seeing super weird behaviour on some iOS devices:
			// crashes with properly escaped whitespaces %20 and commas %2C - and other special characters
			// 'NSAllowsArbitraryLoads' and 'NSURLConnection finished with error - code - 1002'
			// Use 'CFNETWORK_DIAGNOSTICS=1' in XCode to get more details https://stackoverflow.com/a/46748461

			// trying to get rid of at least the most common characters - other will still crash
#if UNITY_IOS
			Query = Query
				.Replace(",", " ")
				.Replace(".", " ")
				.Replace("-", " ");
#endif

			return
				Constants.BaseAPI +
				ApiEndpoint +
				Mode +
#if UNITY_IOS
#if UNITY_2017_1_OR_NEWER
				UnityEngine.Networking.UnityWebRequest.EscapeURL(Query) +
#else
				WWW.EscapeURL(Query) +
#endif
#else
				Uri.EscapeDataString(Query) +
#endif
				".json" +
				EncodeQueryString(opts);
		}

	}

}