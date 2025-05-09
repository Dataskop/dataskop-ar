﻿using Mapbox.Unity;

namespace Mapbox.Tokens {

	using Platform;
	using System;
	using System.ComponentModel;
	using VectorTile.Geometry;

	public enum MapboxTokenStatus {

		/// <summary>The token is valid and active </summary>
		[Description("The token is valid and active")]
		TokenValid,
		/// <summary>the token can not be parsed </summary>
		[Description("the token can not be parsed")]
		TokenMalformed,
		/// <summary>the signature for the token does not validate </summary>
		[Description("the signature for the token does not validate")]
		TokenInvalid,
		/// <summary> the token was temporary and expired</summary>
		[Description("the token was temporary and expired")]
		TokenExpired,
		/// <summary>the token's authorization has been revoked </summary>
		[Description("the token's authorization has been revoked")]
		TokenRevoked,
		/// <summary>inital value </summary>
		StatusNotYetSet

	}

	/// <summary>
	/// Wrapper class to retrieve details about a token
	/// </summary>
	public class MapboxTokenApi {

		public MapboxTokenApi() { }

		// use internal FileSource without(!) passing access token from config into constructor
		// otherwise access token would be appended to url twice
		// https://www.mapbox.com/api-documentation/accounts/#retrieve-a-token
		// if we should ever implement other API methods: creating, deleting, updating ... tokens
		// we will need another FileSource with the token from the config
		private FileSource _fs;

		public void Retrieve(Func<string> skuToken, string accessToken, Action<MapboxToken> callback) {
			if (_fs == null) {
				_fs = new FileSource(skuToken);
			}

			_fs.Request(
				Utils.Constants.BaseAPI + "tokens/v2?access_token=" + accessToken,
				(Response response) =>
				{
					if (response.HasError) {
						callback(
							new MapboxToken() {
								HasError = true,
								ErrorMessage = response.ExceptionsAsString
							}
						);

						return;

					}

					callback(MapboxToken.FromResponseData(response.Data));
				}
			);
		}

	}

}