using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliA.Platformer;


[EntityAttribute.MapEditorGroup("Contraption")]
public sealed class Track : Entity, IBlockEntity {




	#region --- VAR ---


	// Const
	public static readonly int TYPE_ID = typeof(Track).AngeHash();
	private static readonly SpriteCode BODY_SP = "Track.Body";
	private static readonly SpriteCode BODY_TILT = "Track.Tilt";
	private static readonly SpriteCode BODY_CENTER = "TrackCenter";

	// Data
	private readonly bool[] HasTrackArr = [false, false, false, false, false, false, false, false,];


	#endregion




	#region --- MSG ---


	public override void OnActivated () {
		base.OnActivated();
		OnEntityRefresh();
	}


	public void OnEntityRefresh () {

		var squad = WorldSquad.Front;
		int unitX = (X + 1).ToUnit();
		int unitY = (Y + 1).ToUnit();

		// 7 0 1
		// 6   2
		// 5 4 3
		HasTrackArr[0] = squad.GetBlockAt(unitX, unitY + 1, BlockType.Entity) == TYPE_ID;
		HasTrackArr[1] = squad.GetBlockAt(unitX + 1, unitY + 1, BlockType.Entity) == TYPE_ID;
		HasTrackArr[2] = squad.GetBlockAt(unitX + 1, unitY, BlockType.Entity) == TYPE_ID;
		HasTrackArr[3] = squad.GetBlockAt(unitX + 1, unitY - 1, BlockType.Entity) == TYPE_ID;
		HasTrackArr[4] = squad.GetBlockAt(unitX, unitY - 1, BlockType.Entity) == TYPE_ID;
		HasTrackArr[5] = squad.GetBlockAt(unitX - 1, unitY - 1, BlockType.Entity) == TYPE_ID;
		HasTrackArr[6] = squad.GetBlockAt(unitX - 1, unitY, BlockType.Entity) == TYPE_ID;
		HasTrackArr[7] = squad.GetBlockAt(unitX - 1, unitY + 1, BlockType.Entity) == TYPE_ID;

	}


	public override void FirstUpdate () {
		base.FirstUpdate();
		Physics.FillEntity(PhysicsLayer.ENVIRONMENT, this, true);
	}


	public override void BeforeUpdate () {
		base.BeforeUpdate();

	}


	public override void LateUpdate () {
		base.LateUpdate();

		if (!Renderer.TryGetSprite(BODY_SP, out var bodySP)) return;
		if (!Renderer.TryGetSprite(BODY_TILT, out var bodyTilt)) return;

		int centerX = X + Width / 2;
		int centerY = Y + Height / 2;

		// Line
		const int SIZE = Const.HALF + 2;
		const int SIZE_TILT = Const.HALF * 141422 / 100000 + 2;
		for (int i = 0; i < 8; i++) {
			if (!HasTrackArr[i]) continue;
			var dir = (Direction8)i;
			bool positive = dir.IsPositive();
			int rot = dir.GetRotation();
			bool tilt = dir.IsTilted();
			int size = tilt ? SIZE_TILT : SIZE;
			Renderer.Draw(
				tilt ? bodyTilt : bodySP,
				centerX, centerY,
				500, positive ? 0 : 1000,
				positive ? rot : rot + 180,
				size, size
			);
		}

		// Center
		Renderer.Draw(BODY_CENTER, centerX, centerY, 500, 500, 0, Const.HALF, Const.HALF);

	}


	#endregion




	#region --- API ---



	#endregion




	#region --- LGC ---



	#endregion




}
