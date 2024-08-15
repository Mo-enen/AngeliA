using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

internal class TestMapGenerator : MapGenerator {

	public override MapGenerationResult GenerateMap (Int3 worldPosition, IBlockSquad squad, int seed) {


		System.Threading.Thread.Sleep(500);
		Debug.Log(worldPosition);
		System.Threading.Thread.Sleep(500);




		return MapGenerationResult.Success;
	}

}
