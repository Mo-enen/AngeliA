namespace AngeliA;

public sealed class FailbackMapGenerator (int typeID) : MapGenerator(typeID) {
	public override MapGenerationResult GenerateMap (IBlockSquad squad, Int3 worldPos) => MapGenerationResult.Skipped;
}
