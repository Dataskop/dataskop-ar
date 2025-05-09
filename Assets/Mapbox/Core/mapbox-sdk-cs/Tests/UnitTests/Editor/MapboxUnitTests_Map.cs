//-----------------------------------------------------------------------
// <copyright file="MapTest.cs" company="Mapbox">
//     Copyright (c) 2016 Mapbox. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

// TODO: figure out how run tests outside of Unity with .NET framework, something like '#if !UNITY'

#if UNITY_5_6_OR_NEWER

namespace Mapbox.MapboxSdkCs.UnitTest {

	using Map;
	using Platform;
	using Mapbox.Utils;
	using NUnit.Framework;
#if UNITY_5_6_OR_NEWER
	using System.Collections;
	using UnityEngine.TestTools;
#endif

	[TestFixture]
	internal class MapTest {

		private FileSource _fs;

		[SetUp]
		public void SetUp() {
#if UNITY_5_6_OR_NEWER
			_fs = new FileSource(
				Unity.MapboxAccess.Instance.Configuration.GetMapsSkuToken,
				Unity.MapboxAccess.Instance.Configuration.AccessToken
			);
#else
			// when run outside of Unity FileSource gets the access token from environment variable 'MAPBOX_ACCESS_TOKEN'
			_fs = new FileSource();
#endif
		}

#if UNITY_5_6_OR_NEWER
		[UnityTest]
		public IEnumerator World()
#else
		[Test]
		public void World()
#endif
		{
			Map<VectorTile> map = new(_fs);

			map.Vector2dBounds = Vector2dBounds.World();
			map.Zoom = 3;

			Utils.VectorMapObserver mapObserver = new();
			map.Subscribe(mapObserver);
			map.Update();

#if UNITY_5_6_OR_NEWER
			IEnumerator enumerator = _fs.WaitForAllRequests();
			while (enumerator.MoveNext()) yield return null;
#else
			_fs.WaitForAllRequests();
#endif

			Assert.AreEqual(64, mapObserver.Tiles.Count);

			map.Unsubscribe(mapObserver);
		}

#if UNITY_5_6_OR_NEWER
		[UnityTest]
		public IEnumerator RasterHelsinki()
#else
		[Test]
		public void RasterHelsinki()
#endif
		{
			Map<RasterTile> map = new(_fs);

			map.Center = new Vector2d(60.163200, 24.937700);
			map.Zoom = 13;

			Utils.RasterMapObserver mapObserver = new();
			map.Subscribe(mapObserver);
			map.Update();

#if UNITY_5_6_OR_NEWER
			IEnumerator enumerator = _fs.WaitForAllRequests();
			while (enumerator.MoveNext()) yield return null;
#else
			_fs.WaitForAllRequests();
#endif

			Assert.AreEqual(1, mapObserver.Tiles.Count);
			Assert.IsNotNull(mapObserver.Tiles[0]);

			map.Unsubscribe(mapObserver);
		}

#if UNITY_5_6_OR_NEWER
		[UnityTest]
		public IEnumerator ChangeTilesetId()
#else
		[Test]
		public void ChangeTilesetId()
#endif
		{
			Map<ClassicRasterTile> map = new(_fs);

			Utils.ClassicRasterMapObserver mapObserver = new();
			map.Subscribe(mapObserver);

			map.Center = new Vector2d(60.163200, 24.937700);
			map.Zoom = 13;
			map.TilesetId = "invalid";
			map.Update();

#if UNITY_5_6_OR_NEWER
			IEnumerator enumerator = _fs.WaitForAllRequests();
			while (enumerator.MoveNext()) yield return null;
#else
			_fs.WaitForAllRequests();
#endif

			Assert.AreEqual(0, mapObserver.Tiles.Count);

			map.TilesetId = "mapbox.terrain-rgb";
			map.Update();

#if UNITY_5_6_OR_NEWER
			enumerator = _fs.WaitForAllRequests();
			while (enumerator.MoveNext()) yield return null;
#else
			_fs.WaitForAllRequests();
#endif

			Assert.AreEqual(1, mapObserver.Tiles.Count);

			map.TilesetId = null; // Use default tileset ID.
			map.Update();

#if UNITY_5_6_OR_NEWER
			enumerator = _fs.WaitForAllRequests();
			while (enumerator.MoveNext()) yield return null;
#else
			_fs.WaitForAllRequests();
#endif

			Assert.AreEqual(2, mapObserver.Tiles.Count);

			// Should have fetched tiles from different tileset IDs.
			Assert.AreNotEqual(mapObserver.Tiles[0], mapObserver.Tiles[1]);

			map.Unsubscribe(mapObserver);
		}

		[Test]
		public void SetVector2dBoundsZoom() {
			Map<RasterTile> map1 = new(_fs);
			Map<RasterTile> map2 = new(_fs);

			map1.Zoom = 3;
			map1.Vector2dBounds = Vector2dBounds.World();

			map2.SetVector2dBoundsZoom(Vector2dBounds.World(), 3);

			Assert.AreEqual(map1.Tiles.Count, map2.Tiles.Count);
		}

		[Test]
		public void TileMax() {
			Map<RasterTile> map = new(_fs);

			map.SetVector2dBoundsZoom(Vector2dBounds.World(), 2);
			map.Update();
			Assert.Less(map.Tiles.Count, Map<RasterTile>.TileMax);

			// Should stay the same, ignore requests.
			map.SetVector2dBoundsZoom(Vector2dBounds.World(), 5);
			map.Update();
			Assert.AreEqual(16, map.Tiles.Count);
		}

		[Test]
		public void Zoom() {
			Map<RasterTile> map = new(_fs);

			map.Zoom = 50;
			Assert.AreEqual(20, map.Zoom);

			map.Zoom = -50;
			Assert.AreEqual(0, map.Zoom);
		}

	}

}

#endif