﻿using System;
using System.Collections.Generic;
using Mapbox.Unity.MeshGeneration.Modifiers;

namespace Mapbox.Unity.Map {

	public class SubLayerBehaviorModifiers : ISubLayerBehaviorModifiers {

		// TODO: Remove if not required.
		private VectorSubLayerProperties _subLayerProperties;

		public SubLayerBehaviorModifiers(VectorSubLayerProperties subLayerProperties) {
			_subLayerProperties = subLayerProperties;
		}

		/// <summary>
		/// Certain layers ("Mapbox Streets with Building Ids") contains unique identifiers
		/// to help mesh generation and feature management. This settings should be
		/// set to "true" while using these tilesets.
		/// </summary>
		/// <param name="isUniqueIds">Is layer using unique building ids</param>
		public virtual void IsBuildingIdsUnique(bool isUniqueIds) {
			if (_subLayerProperties.buildingsWithUniqueIds != isUniqueIds) {
				_subLayerProperties.buildingsWithUniqueIds = isUniqueIds;
				_subLayerProperties.HasChanged = true;
			}
		}

		/// <summary>
		/// Set the strategy for pivot placement for features.
		/// </summary>
		/// <param name="positionTargetType">Strategy for feature pivot point</param>
		public virtual void SetFeaturePivotStrategy(PositionTargetType positionTargetType) {
			if (_subLayerProperties.moveFeaturePositionTo != positionTargetType) {
				_subLayerProperties.moveFeaturePositionTo = positionTargetType;
				_subLayerProperties.HasChanged = true;
			}
		}

		/// <summary>
		/// Add game object modifier to the modifiers list.
		/// </summary>
		/// <param name="modifier">Game object modifier to add to style</param>
		public virtual void AddGameObjectModifier(GameObjectModifier modifier) {
			if (_subLayerProperties.GoModifiers == null) {
				_subLayerProperties.GoModifiers = new List<GameObjectModifier>();
			}

			_subLayerProperties.GoModifiers.Add(modifier);
			_subLayerProperties.HasChanged = true;
		}

		/// <summary>
		/// List of game object modifiers to the modifiers list.
		/// </summary>
		/// <param name="modifiers">List of game object modifiers to add to style</param>
		public virtual void AddGameObjectModifier(List<GameObjectModifier> modifiers) {
			if (_subLayerProperties.GoModifiers == null) {
				_subLayerProperties.GoModifiers = new List<GameObjectModifier>();
			}

			_subLayerProperties.GoModifiers.AddRange(modifiers);
			_subLayerProperties.HasChanged = true;
		}

		/// <summary>
		/// Return game object modifiers from the modifiers list by query
		/// </summary>
		/// <param name="function">Query function to test mesh modifiers</param>
		public virtual List<GameObjectModifier> GetGameObjectModifier(Func<GameObjectModifier, bool> function) {
			List<GameObjectModifier> finalList = new();

			if (_subLayerProperties.GoModifiers != null) {
				foreach (GameObjectModifier goModifier in _subLayerProperties.GoModifiers) {
					if (function(goModifier)) {
						finalList.Add(goModifier);
					}
				}
			}

			return finalList;
		}

		/// <summary>
		/// Remove game object modifier from the modifiers list
		/// </summary>
		/// <param name="modifier">Game object modifier to be removed from style</param>
		public virtual void RemoveGameObjectModifier(GameObjectModifier modifier) {
			if (_subLayerProperties.GoModifiers != null) {
				_subLayerProperties.GoModifiers.Remove(modifier);
				_subLayerProperties.HasChanged = true;
			}
		}

		/// <summary>
		/// Add mesh modifier to the modifiers list.
		/// </summary>
		/// <param name="modifier">Mesh modifier to add to style</param>
		public virtual void AddMeshModifier(MeshModifier modifier) {
			if (_subLayerProperties.MeshModifiers == null) {
				_subLayerProperties.MeshModifiers = new List<MeshModifier>();
			}

			_subLayerProperties.MeshModifiers.Add(modifier);
			_subLayerProperties.HasChanged = true;
		}

		/// <summary>
		/// List of mesh modifiers to the modifiers list.
		/// </summary>
		/// <param name="modifiers">List of mesh modifiers to add to style</param>
		public virtual void AddMeshModifier(List<MeshModifier> modifiers) {
			if (_subLayerProperties.MeshModifiers == null) {
				_subLayerProperties.MeshModifiers = new List<MeshModifier>();
			}

			_subLayerProperties.MeshModifiers.AddRange(modifiers);
			_subLayerProperties.HasChanged = true;
		}

		/// <summary>
		/// Return mesh modifiers from the modifiers list by query
		/// </summary>
		/// <param name="function">Query function to test mesh modifiers</param>
		public virtual List<MeshModifier> GetMeshModifier(Func<MeshModifier, bool> function) {
			List<MeshModifier> finalList = new();

			if (_subLayerProperties.MeshModifiers != null) {
				foreach (MeshModifier meshModifier in _subLayerProperties.MeshModifiers) {
					if (function(meshModifier)) {
						finalList.Add(meshModifier);
					}
				}
			}

			return finalList;
		}

		/// <summary>
		/// Remove mesh modifier from the modifiers list
		/// </summary>
		/// <param name="modifier">Mesh modifier to be removed from style</param>
		public virtual void RemoveMeshModifier(MeshModifier modifier) {
			if (_subLayerProperties.MeshModifiers != null) {
				_subLayerProperties.MeshModifiers.Remove(modifier);
				_subLayerProperties.HasChanged = true;
			}
		}

	}

}