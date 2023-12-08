using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {
	public class MapGenerator_SimpleMaze : ActionMapGenerator {


		protected override void GenerateMap () {

			//int z = Stage.ViewZ;
			//for (int i = 0; i < 256; i++) {
			//	ResultWriter.SetBlockAt(i, 0, z, BlockType.Level, typeof(Barrel).AngeHash());
			//}

			foreach (var room in MapGeneratorRoom.ForAllConnectedRooms(
				SampleReader, X.ToUnit(), Y.ToUnit(), Stage.ViewZ - 1
			)) {

				Debug.Log("roomid: " + room.ID);


			}

		}


	}
}