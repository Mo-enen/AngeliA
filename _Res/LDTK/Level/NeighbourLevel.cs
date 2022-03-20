using Newtonsoft.Json;

namespace LdtkToAngeliA
{
    public partial class NeighbourLevel
    {
        /// <summary>
        /// A single lowercase character tipping on the level location (`n`orth, `s`outh, `w`est,
        /// `e`ast).
        /// </summary>
        [JsonProperty("dir")]
        public string Dir { get; set; }

        [JsonProperty("levelUid")]
        public int LevelUid { get; set; }
    }
}