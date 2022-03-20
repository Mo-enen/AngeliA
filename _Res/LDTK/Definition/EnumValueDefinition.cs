using Newtonsoft.Json;

namespace LdtkToAngeliA
{
    public partial class EnumValueDefinition
    {
        /// <summary>
        /// An array of 4 Int values that refers to the tile in the tileset image: `[ x, y, width,
        /// height ]`
        /// </summary>
        [JsonProperty("__tileSrcRect")]
        public int[] TileSrcRect { get; set; }

        /// <summary>
        /// Optional color
        /// </summary>
        [JsonProperty("color")]
        public int Color { get; set; }

        /// <summary>
        /// Enum value
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// The optional ID of the tile
        /// </summary>
        [JsonProperty("tileId")]
        public int? TileId { get; set; }
    }
}