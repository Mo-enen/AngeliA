namespace AngeliA;

public sealed class FailbackMapGenerator : MapGenerator {
	public override bool IncludeInOpenWorld => false;
	public override MapGenerationResult GenerateMap (IBlockSquad squad, Int3 startPoint, Direction8? startDirection) {
		return MapGenerationResult.Skipped;
	}
}
