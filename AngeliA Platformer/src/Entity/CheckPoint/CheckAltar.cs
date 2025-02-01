using System.Collections;
using System.Collections.Generic;


using AngeliA;
namespace AngeliA.Platformer;


public abstract class CheckAltar<CP> : CheckAltar where CP : CheckPoint {
	public CheckAltar () => LinkedCheckPointID = typeof(CP).AngeHash();
}


[EntityAttribute.MapEditorGroup("CheckPoint")]
[EntityAttribute.Capacity(1, 1)]
[EntityAttribute.Layer(EntityLayer.ENVIRONMENT)]
public abstract class CheckAltar : Entity, ICircuitOperator, IBlockEntity {




	#region --- VAR ---


	// Api
	public static bool LinkPoolReady { get; private set; } = false;
	public static int CurrentAltarID { get; private set; } = 0;
	public static Int3 CurrentAltarUnitPos { get; private set; }

	// Data
	private static readonly Dictionary<int, int> LinkPool = [];
	protected int LinkedCheckPointID = 0;


	#endregion




	#region --- MSG ---


	[CircuitOperator_Int3UnitPos_IntStamp_Direction5From]
	internal static void CircuitOperator (Int3 unitPos, int _, Direction5 __) => TriggerCheckAltar(unitPos);


	[OnGameInitialize(-64)]
	internal static void InitializeLinkPool () {
		LinkPool.Clear();
		foreach (var type in typeof(CheckAltar).AllChildClass()) {
			var args = type.BaseType.GenericTypeArguments;
			if (args.Length >= 1) {
				int typeID = type.AngeHash();
				int argID = args[0].AngeHash();
				LinkPool.TryAdd(typeID, argID);
				LinkPool.TryAdd(argID, typeID);
			}
		}
		LinkPool.TrimExcess();
		LinkPoolReady = true;
	}


	[OnMapEditorModeChange_Mode]
	internal static void OnMapEditorEditModeChanged (OnMapEditorModeChange_ModeAttribute.Mode mode) {
		if (mode == OnMapEditorModeChange_ModeAttribute.Mode.ExitEditMode) {
			CurrentAltarID = 0;
			CurrentAltarUnitPos = default;
		}
	}


	public override void OnActivated () {
		base.OnActivated();
		Height = Const.CEL * 2;
	}


	public override void FirstUpdate () {
		base.FirstUpdate();
		Physics.FillEntity(PhysicsLayer.ENVIRONMENT, this, true);
		var border = Renderer.TryGetSprite(TypeID, out var sprite, false) ? sprite.GlobalBorder : Int4.zero;
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
			Touch();
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
		// Body
		Draw();
		// Highlight
		var unitPos = new Int3(X.ToUnit(), Y.ToUnit(), Stage.ViewZ);
		if (PlayerSystem.RespawnCpUnitPosition == unitPos) {
			var tint = new Color32(128, 255, 128, 255);
			FrameworkUtil.DrawLoopingActivatedHighlight(Rect, tint);
		}
	}


	void ICircuitOperator.OnTriggeredByCircuit () => Touch();


	#endregion




	#region --- API ---


	public virtual void Touch () => TriggerCheckAltar(new Int3(X.ToUnit(), Y.ToUnit(), Stage.ViewZ));


	public static bool TryGetLinkedID (int id, out int linkedID) => LinkPool.TryGetValue(id, out linkedID);


	public static void TriggerCheckAltar (Int3 unitPos) {

		int id = WorldSquad.Front.GetBlockAt(unitPos.x, unitPos.y, unitPos.z, BlockType.Entity);
		if (!TryGetLinkedID(id, out int linkedCheckPointID)) return;

		PlayerSystem.RespawnCpUnitPosition = unitPos;

		// Clear Portal
		if (
			CheckPoint.LastTriggeredCheckPointID == linkedCheckPointID &&
			Stage.GetSpawnedEntityCount(CheckPointPortal.TYPE_ID) != 0 &&
			Stage.TryFindEntity(CheckPointPortal.TYPE_ID, out var cpPortal)
		) {
			cpPortal.Active = false;
		}

		// Update Last Checked Pos
		CurrentAltarID = id;
		CurrentAltarUnitPos = unitPos;

	}


	#endregion




}