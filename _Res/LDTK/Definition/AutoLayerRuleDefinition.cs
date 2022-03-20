using Newtonsoft.Json;

namespace LdtkToAngeliA
{
    public partial class AutoLayerRuleDefinition
    {
        /// <summary>
        /// If FALSE, the rule effect isn't applied, and no tiles are generated.
        /// </summary>
        [JsonProperty("active")]
        public bool Active { get; set; }

        /// <summary>
        /// When TRUE, the rule will prevent other rules to be applied in the same cell if it matches
        /// (TRUE by default).
        /// </summary>
        [JsonProperty("breakOnMatch")]
        public bool BreakOnMatch { get; set; }

        /// <summary>
        /// Chances for this rule to be applied (0 to 1)
        /// </summary>
        [JsonProperty("chance")]
        public float Chance { get; set; }

        /// <summary>
        /// Checker mode Possible values: `None`, `Horizontal`, `Vertical`
        /// </summary>
        [JsonProperty("checker")]
        public string Checker { get; set; }

        /// <summary>
        /// If TRUE, allow rule to be matched by flipping its pattern horizontally
        /// </summary>
        [JsonProperty("flipX")]
        public bool FlipX { get; set; }

        /// <summary>
        /// If TRUE, allow rule to be matched by flipping its pattern vertically
        /// </summary>
        [JsonProperty("flipY")]
        public bool FlipY { get; set; }

        /// <summary>
        /// Default IntGrid value when checking cells outside of level bounds
        /// </summary>
        [JsonProperty("outOfBoundsValue")]
        public int? OutOfBoundsValue { get; set; }

        /// <summary>
        /// Rule pattern (size x size)
        /// </summary>
        [JsonProperty("pattern")]
        public int[] Pattern { get; set; }

        /// <summary>
        /// If TRUE, enable Perlin filtering to only apply rule on specific random area
        /// </summary>
        [JsonProperty("perlinActive")]
        public bool PerlinActive { get; set; }

        [JsonProperty("perlinOctaves")]
        public float PerlinOctaves { get; set; }

        [JsonProperty("perlinScale")]
        public float PerlinScale { get; set; }

        [JsonProperty("perlinSeed")]
        public float PerlinSeed { get; set; }

        /// <summary>
        /// X pivot of a tile stamp (0-1)
        /// </summary>
        [JsonProperty("pivotX")]
        public float PivotX { get; set; }

        /// <summary>
        /// Y pivot of a tile stamp (0-1)
        /// </summary>
        [JsonProperty("pivotY")]
        public float PivotY { get; set; }

        /// <summary>
        /// Pattern width & height. Should only be 1,3,5 or 7.
        /// </summary>
        [JsonProperty("size")]
        public int Size { get; set; }

        /// <summary>
        /// Array of all the tile IDs. They are used randomly or as stamps, based on `tileMode` value.
        /// </summary>
        [JsonProperty("tileIds")]
        public int[] TileIds { get; set; }

        /// <summary>
        /// Defines how tileIds array is used Possible values: `Single`, `Stamp`
        /// </summary>
        [JsonProperty("tileMode")]
        public string TileMode { get; set; }

        /// <summary>
        /// Unique Int identifier
        /// </summary>
        [JsonProperty("uid")]
        public int Uid { get; set; }

        /// <summary>
        /// X cell coord modulo
        /// </summary>
        [JsonProperty("xModulo")]
        public int XModulo { get; set; }

        /// <summary>
        /// Y cell coord modulo
        /// </summary>
        [JsonProperty("yModulo")]
        public int YModulo { get; set; }
    }
}