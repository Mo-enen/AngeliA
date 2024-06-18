using System.Collections;
using System.Collections.Generic;


namespace AngeliA;
[EntityAttribute.MapEditorGroup("CheckPoint")]
[EntityAttribute.Bounds(0, 0, Const.CEL, Const.CEL * 2)]
[EntityAttribute.Capacity(1, 1)]
public abstract class CheckAltar<CP> : EnvironmentEntity, IUnique where CP : CheckPoint {




	#region --- VAR ---


	// Data
	private static readonly Dictionary<int, int> LinkPool = new();
	private readonly int LinkedCheckPointID = 0;


	#endregion




	#region --- MSG ---


	public CheckAltar () => LinkedCheckPointID = typeof(CP).AngeHash();


	[OnGameInitialize(-64)]
	public static void BeforeGameInitializeLater () {
		LinkPool.Clear();
		foreach (var type in typeof(CheckAltar<>).AllChildClass()) {
			var args = type.BaseType.GenericTypeArguments;
			if (args.Length >= 1) {
				int typeID = type.AngeHash();
				int argID = args[0].AngeHash();
				LinkPool.TryAdd(typeID, argID);
				LinkPool.TryAdd(argID, typeID);
			}
		}
	}


	public override void OnActivated () {
		base.OnActivated();
		Height = Const.CEL * 2;
	}


	public override void FirstUpdate () {
		base.FirstUpdate();
		var border = Renderer.TryGetSprite(TypeID, out var sprite) ? sprite.GlobalBorder : Int4.zero;
		Physics.FillBlock(
			PhysicsLayer.ENVIRONMENT, TypeID, Rect.Shrink(border), true, Tag.OnewayUp
		);
	}


	public override void Update () {

		base.Update();

		var player = Player.Selecting;

		if (player == null || !player.Active) return;
		var unitPos = new Int3(X.ToUnit(), Y.ToUnit(), Stage.ViewZ);
		bool highlighting = Player.RespawnCpUnitPosition.HasValue && Player.RespawnCpUnitPosition.Value == unitPos;
		bool trySpawnPortal = highlighting && CheckPoint.LastTriggeredCheckPointID == LinkedCheckPointID;

		// Player Touch Check
		if (!highlighting && player.Rect.Overlaps(Rect)) {
			highlighting = true;
			Player.RespawnCpUnitPosition = unitPos;
			CheckPoint.Unlock(LinkedCheckPointID);
			// Clear Portal
			if (
				CheckPoint.LastTriggeredCheckPointID == LinkedCheckPointID &&
				Stage.GetSpawnedEntityCount(CheckPointPortal.TYPE_ID) != 0 &&
				Stage.TryGetEntity(CheckPointPortal.TYPE_ID, out var cpPortal)
			) {
				cpPortal.Active = false;
			}
		}

		// Spawn Portal
		if (
			highlighting && trySpawnPortal &&
			CheckPoint.LastTriggeredCheckPointUnitPosition.HasValue &&
			Stage.GetSpawnedEntityCount(CheckPointPortal.TYPE_ID) == 0 &&
			Stage.GetOrAddEntity(CheckPointPortal.TYPE_ID, X, Y + Const.CEL * 4) is CheckPointPortal portal
		) {
			portal.SetCheckPoint(CheckPoint.LastTriggeredCheckPointID, CheckPoint.LastTriggeredCheckPointUnitPosition.Value);
		}

	}


	public override void LateUpdate () {
		base.LateUpdate();
		Renderer.Draw(TypeID, Rect);
		var unitPos = new Int3(X.ToUnit(), Y.ToUnit(), Stage.ViewZ);
		if (Player.RespawnCpUnitPosition == unitPos) {
			CheckPoint.DrawActivatedHighlight(Rect);
		}
	}


	#endregion




	#region --- API ---


	public static bool TryGetLinkedID (int id, out int linkedID) => LinkPool.TryGetValue(id, out linkedID);


	#endregion




}