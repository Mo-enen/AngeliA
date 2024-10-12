namespace AngeliA;

public sealed class FailbackMapGenerator (int typeID) : MapGenerator(typeID) {
	public override bool IncludeInOpenWorld => false;
	public override MapGenerationResult GenerateMap (Int3 worldPos) {
		return MapGenerationResult.Skipped;
	}
}
