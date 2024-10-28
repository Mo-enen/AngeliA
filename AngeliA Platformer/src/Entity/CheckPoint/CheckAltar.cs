using System.Collections;
using System.Collections.Generic;


using AngeliA;
namespace AngeliA.Platformer;
[EntityAttribute.MapEditorGroup("CheckPoint")]
[EntityAttribute.Capacity(1, 1)]
[EntityAttribute.Layer(EntityLayer.ENVIRONMENT)]
public abstract class CheckAltar<CP> : Entity, IBlockEntity where CP : CheckPoint {




	#region --- VAR ---


	// Api
	public static bool LinkPoolReady { get; private set; } = false;
	public static int CurrentAltarID { get; private set; } = 0;
	public static Int3 CurrentAltarUnitPos { get; private set; }

	// Data
	private static readonly Dictionary<int, int> LinkPool = [];
	private readonly int LinkedCheckPointID = 0;


	#endregion




	#region --- MSG ---


	public CheckAltar () => LinkedCheckPointID = typeof(CP).AngeHash();


	[OnGameInitialize(-64)]
	internal static void InitializeLinkPool () {
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
		LinkPoolReady = true;
	}


	[OnMapEditorEditModeChanged]
	internal static void OnMapEditorEditModeChanged () {
		CheckAltar<CheckPoint>.CurrentAltarID = 0;
		CheckAltar<CheckPoint>.CurrentAltarUnitPos = default;
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

		var player = PlayerSystem.Selecting;

		if (player == null || !player.Active) return;
		var unitPos = new Int3(X.ToUnit(), Y.ToUnit(), Stage.ViewZ);
		bool highlighting = PlayerSystem.RespawnCpUnitPosition.HasValue && PlayerSystem.RespawnCpUnitPosition.Value == unitPos;
		bool trySpawnPortal = highlighting && CheckPoint.LastTriggeredCheckPointID == LinkedCheckPointID;

		// Player Touch Check
		if (!highlighting && player.Rect.Overlaps(Rect)) {
			highlighting = true;
			PlayerSystem.RespawnCpUnitPosition = unitPos;

			// Clear Portal
			if (
				CheckPoint.LastTriggeredCheckPointID == LinkedCheckPointID &&
				Stage.GetSpawnedEntityCount(CheckPointPortal.TYPE_ID) != 0 &&
				Stage.TryGetEntity(CheckPointPortal.TYPE_ID, out var cpPortal)
			) {
				cpPortal.Active = false;
			}

			// Update Last Checked Pos
			CheckAltar<CheckPoint>.CurrentAltarID = TypeID;
			CheckAltar<CheckPoint>.CurrentAltarUnitPos = new Int3(X.ToUnit(), Y.ToUnit(), Stage.ViewZ);
		}

		// Spawn Portal
		if (
			highlighting && trySpawnPortal &&
			CheckPoint.LastTriggeredCheckPointUnitPosition.HasValue &&
			Stage.GetSpawnedEntityCount(CheckPointPortal.TYPE_ID) == 0 &&
			Stage.GetOrSpawnEntity(CheckPointPortal.TYPE_ID, X, Y + Const.CEL * 4) is CheckPointPortal portal
		) {
			portal.SetCheckPoint(CheckPoint.LastTriggeredCheckPointID, CheckPoint.LastTriggeredCheckPointUnitPosition.Value);
		}

	}


	public override void LateUpdate () {
		base.LateUpdate();
		Renderer.Draw(TypeID, Rect);
		var unitPos = new Int3(X.ToUnit(), Y.ToUnit(), Stage.ViewZ);
		if (PlayerSystem.RespawnCpUnitPosition == unitPos) {
			CheckPoint.DrawActivatedHighlight(Rect);
		}
	}


	#endregion




	#region --- API ---


	public static bool TryGetLinkedID (int id, out int linkedID) => LinkPool.TryGetValue(id, out linkedID);


	#endregion




}