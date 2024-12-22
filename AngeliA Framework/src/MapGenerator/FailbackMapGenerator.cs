namespace AngeliA;

public sealed class FailbackMapGenerator (int typeID) : MapGenerator(typeID) {
	public override MapGenerationResult GenerateMap (WorldStream stream, Int3 worldPos) => MapGenerationResult.Skipped;
}
