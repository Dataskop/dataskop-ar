namespace Mapbox.Platform.Cache {

	using Map;
	using System;

	public interface ICache {

		/// <summary>
		/// Maximum number of tiles to store
		/// </summary>
		uint MaxCacheSize { get; }

		/// <summary>
		/// Add tile data to the cache
		/// </summary>
		/// <param name="tilesetId">Tile set name</param>
		/// <param name="tileId</param>
		/// <param name="item">Item to cache</param>
		/// <param name="replaceIfExists">Force insert even if item already exists.</param>
		void Add(string tilesetId, CanonicalTileId tileId, CacheItem item, bool replaceIfExists);

		/// <summary>
		/// Get tile
		/// </summary>
		/// <param name="tilesetId"></param>
		/// <param name="tileId"></param>
		/// <returns>byte[] with tile data. Null if requested tile is not in cache</returns>
		CacheItem Get(string tilesetId, CanonicalTileId tileId);

		/// <summary>Clear cache for all tile sets</summary>
		void Clear();

		/// <summary>
		/// Clear cache for one tile set
		/// </summary>
		/// <param name="tilesetId"></param>
		void Clear(string tilesetId);

		/// <summary>
		/// Reinitialize cache. Might be needed after 'Clear', eg for SQLiteCache
		/// </summary>
		void ReInit();

	}

}