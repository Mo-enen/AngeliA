using Newtonsoft.Json;

namespace LdtkToAngeliA
{
    public partial class EntityInstanceTile
    {
        /// <summary>
        /// An array of 4 Int values that refers to the tile in the tileset image: `[ x, y, width,
        /// height ]`
        /// </summary>
        [JsonProperty("srcRect")]
        public int[] SrcRect { get; set; }

        /// <summary>
        /// Tileset ID
        /// </summary>
        [JsonProperty("tilesetUid")]
        public int TilesetUid { get; set; }
    }
}