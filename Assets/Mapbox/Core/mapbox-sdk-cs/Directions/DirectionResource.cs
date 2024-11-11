//-----------------------------------------------------------------------
// <copyright file="DirectionResource.cs" company="Mapbox">
//     Copyright (c) 2016 Mapbox. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Mapbox.Directions {

	using System;
	using System.Collections.Generic;
	using Utils;
	using Platform;

	/// <summary> A directions request. </summary>
	public class DirectionResource : Resource {

		private string apiEndpoint = "directions/v5/";

		// Required
		private RoutingProfile profile;

		// Optional
		private Vector2d[] coordinates;

		// Optional
		private bool? alternatives;

		// Optional
		private BearingFilter[] bearings;

		// Optional
		private bool? continueStraight;

		// Optional
		private Overview overview;

		// Optional
		private double[] radiuses;

		// Optional
		private bool? steps;

		/// <summary> Initializes a new instance of the <see cref="DirectionResource" /> class.</summary>
		/// <param name="coordinates">
		///     Array of LatLng points along route, between 2 and 25 elements in length.
		/// </param>
		/// <param name="profile">
		///     A routing profile, <see cref="RoutingProfile"/> for all profile options.
		/// </param>
		public DirectionResource(Vector2d[] coordinates, RoutingProfile profile) {
			Coordinates = coordinates;
			RoutingProfile = profile;
		}

		/// <summary> Gets the API endpoint as a partial URL path. </summary>
		public override string ApiEndpoint => apiEndpoint;

		/// <summary>
		///     Gets or sets the coordinates. Array of LatLng points along route,
		///     between 2 and 25 elements in length.
		/// </summary>
		public Vector2d[] Coordinates
		{
			get => coordinates;

			set
			{
				if (value.Length < 2 || value.Length > 25) {
					throw new Exception("Must be between 2 and 25 elements in coordinates array.");
				}

				coordinates = value;
			}
		}

		/// <summary>
		///     Gets or sets the routing profile, <see cref="RoutingProfile"/> for all profile options.
		/// </summary>
		public RoutingProfile RoutingProfile
		{
			get => profile;

			set => profile = value;
		}

		/// <summary>
		///     Gets or sets the alternative option. Controls whether direction request should
		///     return alternative routes.
		/// </summary>
		public bool? Alternatives
		{
			get => alternatives;

			set => alternatives = value;
		}

		/// <summary>
		///     Gets or sets the bearing option. An array of bearing filters. Each filter is composed of
		///     a bearing as decimal degrees clockwise between 0 and 360, and a range of variation from
		///     the bearing as decimal degrees between 0 and 180.
		/// </summary>
		public BearingFilter[] Bearings
		{
			get => bearings;

			set
			{
				if (value != null && value.Length != coordinates.Length) {
					throw new Exception("There must be as many bearings as there are coordinates in the request.");
				}

				bearings = value;
			}
		}

		/// <summary>
		///     Gets or sets the continue_straight option. Controls whether to route will
		///     continue in same direction of travel or if route may continue in opposite
		///     direction of travel at intermediate waypoints.
		/// </summary>
		public bool? ContinueStraight
		{
			get => continueStraight;

			set => continueStraight = value;
		}

		/// <summary>
		///     Gets or sets the overview option. See <see cref="Overview"/> for all overview options.
		/// </summary>
		public Overview Overview
		{
			get => overview;

			set => overview = value;
		}

		/// <summary>
		///     Gets or sets the radiuses option. Controls maximum distance in meters that
		///     each coordinate is allowed to move when snapped to a nearby road segment.
		/// </summary>
		public double[] Radiuses
		{
			get => radiuses;

			set
			{
				if (value != null) {
					if (value.Length != coordinates.Length) {
						throw new Exception("There must be as many radiuses as there are coordinates in the request.");
					}

					for (int i = 0; i < value.Length; i++) {
						if (value[i] <= 0) {
							throw new Exception("Radius must be greater than 0");
						}
					}
				}

				radiuses = value;
			}
		}

		/// <summary> Gets or sets the steps option. Controls whether to return steps and turn-by-turn instructions.</summary>
		public bool? Steps
		{
			get => steps;

			set => steps = value;
		}

		/// <summary>
		/// Gets the URL string.
		/// </summary>
		/// <returns>The URL string.</returns>
		public override string GetUrl() {
			Dictionary<string, string> opts = new();

			if (Alternatives != null) {
				opts.Add("alternatives", Alternatives.ToString().ToLower());
			}

			if (Bearings != null) {
				opts.Add("bearings", GetUrlQueryFromArray(Bearings, ";"));
			}

			if (ContinueStraight != null) {
				opts.Add("continue_straight", ContinueStraight.ToString().ToLower());
			}

			if (Overview != null) {
				opts.Add("overview", Overview.ToString());
			}

			if (Radiuses != null) {
				opts.Add("radiuses", GetUrlQueryFromArray(Radiuses));
			}

			if (Steps != null) {
				opts.Add("steps", Steps.ToString().ToLower());
			}

			return Constants.BaseAPI +
			       ApiEndpoint +
			       RoutingProfile +
			       GetUrlQueryFromArray<Vector2d>(Coordinates, ";") +
			       ".json" +
			       EncodeQueryString(opts);
		}

	}

}