using System.Collections;
using System.Collections.Generic;


namespace AngeliaFramework {
	public class CollapseMapGenerator : RoomBasedMapGenerator {


		// Data
		protected readonly Dictionary<int, Room> RoomPool = new();
		protected RoomNode RootNode = null;


		// MSG
		protected override void BeforeMapGenerate () {
			base.BeforeMapGenerate();
			RoomPool.Clear();
			RootNode = ForAllConnectedRooms(
				SampleReader, X.ToUnit(), Y.ToUnit(), Stage.ViewZ - 1,
				(_room) => RoomPool.TryAdd(_room.ID, _room)
			);
			Game.Log(RootNode.PrintTree());
		}


		protected override void OnMapGenerate () {




		}


		protected override void AfterMapGenerate () {
			RoomPool.Clear();
			RootNode = null;
			base.AfterMapGenerate();
		}


	}
}
