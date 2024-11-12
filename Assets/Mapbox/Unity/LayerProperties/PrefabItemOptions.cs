namespace Mapbox.Unity.Map {

	using UnityEngine;
	using System.Collections;
	using System;
	using System.Collections.Generic;
	using MeshGeneration.Modifiers;
	using Utilities;
	using MeshGeneration.Filters;

	[Serializable]
	public class PrefabItemOptions : VectorSubLayerProperties {

		#region Fixed Properties

		//Fixed primitiveType
		public readonly VectorPrimitiveType primitiveType = VectorPrimitiveType.Point;

		//Group features turned off
		public readonly bool combineMeshes = false;

		//No extrusion
		public readonly ExtrusionType extrusionType = ExtrusionType.None;

		//Dictionary containing the layer names for each location prefab find by type
		public readonly Dictionary<LocationPrefabFindBy, string> layerNameFromFindByTypeDictionary = new() {
			{
				LocationPrefabFindBy.AddressOrLatLon, ""
			}, {
				LocationPrefabFindBy.MapboxCategory, "poi_label"
			}, {
				LocationPrefabFindBy.POIName, "poi_label"
			}
		};

		//Dictionary containing the property names in the layer for each location prefab find by type
		public readonly Dictionary<LocationPrefabFindBy, string> categoryPropertyFromFindByTypeDictionary = new() {
			{
				LocationPrefabFindBy.AddressOrLatLon, ""
			}, {
				LocationPrefabFindBy.MapboxCategory, "maki"
			}, {
				LocationPrefabFindBy.POIName, "name"
			}
		};

		//Dictionary containing the density names in the layer for each location prefab find by type
		public readonly Dictionary<LocationPrefabFindBy, string> densityPropertyFromFindByTypeDictionary = new() {
			{
				LocationPrefabFindBy.AddressOrLatLon, ""
			}, {
				LocationPrefabFindBy.MapboxCategory, "localrank"
			}, {
				LocationPrefabFindBy.POIName, "localrank"
			}
		};

		//Dictionary containing the density names in the layer for each location prefab find by type
		public readonly Dictionary<LocationPrefabFindBy, string> namePropertyFromFindByTypeDictionary = new() {
			{
				LocationPrefabFindBy.AddressOrLatLon, ""
			}, {
				LocationPrefabFindBy.MapboxCategory, ""
			}, {
				LocationPrefabFindBy.POIName, "name"
			}
		};

		//Force Move prefab feature position to the first vertex
		public readonly PositionTargetType _movePrefabFeaturePositionTo = PositionTargetType.FirstVertex;

		public readonly LayerFilterCombinerOperationType _combinerType = LayerFilterCombinerOperationType.All;

		#endregion

		#region User Choice Properties

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="T:Mapbox.Unity.Map.PrefabItemOptions"/> item is active.
		/// </summary>
		/// <value><c>true</c> if is active; otherwise, <c>false</c>.</value>
		public bool isActive
		{
			get => coreOptions.isActive;
			set => coreOptions.isActive = value;
		}

		public bool snapToTerrain
		{
			get => coreOptions.snapToTerrain;
			set => coreOptions.snapToTerrain = value;
		}

		public string prefabItemName
		{
			get => coreOptions.sublayerName;
			set => coreOptions.sublayerName = value;
		}

		/// <summary>
		/// The prefab to be spawned on the map
		/// </summary>
		public SpawnPrefabOptions spawnPrefabOptions;

		/// <summary>
		/// Find points-of-interest to spawn prefabs using this enum
		/// </summary>
		public LocationPrefabFindBy findByType = LocationPrefabFindBy.MapboxCategory; //default to Mapbox Category

		/// <summary>
		/// Spawn at any location in the categories selected
		/// </summary>
		public LocationPrefabCategories categories;

		/// <summary>
		/// Spawn at any location containing this name string
		/// </summary>
		public string nameString = "Name";

		/// <summary>
		/// Spawn at specific coordinates
		/// </summary>
		[Geocode]
		public string[] coordinates;

		[Range(1, 30)]
		public int density = 15;

		public Action<List<GameObject>> OnAllPrefabsInstantiated
		{
			get => spawnPrefabOptions.AllPrefabsInstatiated;
			set => spawnPrefabOptions.AllPrefabsInstatiated = value;
		}

		#endregion

		public override bool HasChanged
		{
			set
			{
				if (value == true) {
					OnPropertyHasChanged(
						new VectorLayerUpdateArgs {
							property = this
						}
					);
				}
			}
		}

	}

}