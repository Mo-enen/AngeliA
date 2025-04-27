using System.Collections;
using System.Collections.Generic;


using AngeliA;

namespace AngeliA.Platformer;


/// <summary>
/// Entity that function as a check point. Can also use to unlock a type of check point.
/// </summary>
public abstract class CheckAltar<CP> : CheckAltar where CP : CheckPoint {
	public CheckAltar () => LinkedCheckPointID = typeof(CP).AngeHash();
}


/// <summary>
/// Entity that function as a check point. Can also use to unlock a type of check point.
/// </summary>
[EntityAttribute.MapEditorGroup("CheckPoint")]
[EntityAttribute.Layer(EntityLayer.ENVIRONMENT)]
public abstract class CheckAltar : Entity, IBlockEntity {




	#region --- VAR ---


	// Api
	/// <summary>
	/// True if the internal cp link pool is ready to use
	/// </summary>
	public static bool LinkPoolReady { get; private set; } = false;
	/// <summary>
	/// Current activating altar type ID
	/// </summary>
	public static int CurrentAltarID { get; private set; } = 0;
	/// <summary>
	/// Current activating altar position in unit space
	/// </summary>
	public static Int3 CurrentAltarUnitPos { get; private set; }

	// Data
	private static readonly Dictionary<int, int> LinkPool = [];
	protected int LinkedCheckPointID = 0;


	#endregion




	#region --- MSG ---


	[CircuitOperate_Int3UnitPos_IntStamp_Direction5From]
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
		var border = Renderer.TryGetSprite(TypeID, out var sprite, false) ? sprite.GlobalBorder : Int4.Zero;
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


	#endregion




	#region --- API ---


	/// <summary>
	/// Use this function to control logic that handles player touch
	/// </summary>
	public virtual void Touch () => TriggerCheckAltar(TypeID, new Int3(X.ToUnit(), Y.ToUnit(), Stage.ViewZ));


	public static bool TryGetLinkedID (int id, out int linkedID) => LinkPool.TryGetValue(id, out linkedID);


	#endregion




	#region --- LGC ---


	private static void TriggerCheckAltar (Int3 unitPos) {
		int id = WorldSquad.Front.GetBlockAt(unitPos.x, unitPos.y, unitPos.z, BlockType.Entity);
		TriggerCheckAltar(id, unitPos);
	}
	private static void TriggerCheckAltar (int id, Int3 unitPos) {

		// Update Last Checked Pos
		CurrentAltarID = id;
		CurrentAltarUnitPos = unitPos;
		PlayerSystem.RespawnCpUnitPosition = unitPos;

		// Clear Portal
		if (
			TryGetLinkedID(id, out int linkedCheckPointID) &&
			CheckPoint.LastTriggeredCheckPointID == linkedCheckPointID &&
			Stage.GetSpawnedEntityCount(CheckPointPortal.TYPE_ID) != 0 &&
			Stage.TryFindEntity(CheckPointPortal.TYPE_ID, out var cpPortal)
		) {
			cpPortal.Active = false;
		}

	}


	#endregion




}