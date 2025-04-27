using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AngeliA;

namespace AngeliA.Platformer;

/// <summary>
/// Entity that save player position when player touchs. When game restart, player spawns to the saved position.
/// </summary>
[EntityAttribute.MapEditorGroup("CheckPoint")]
[EntityAttribute.Layer(EntityLayer.ENVIRONMENT)]
public abstract class CheckPoint : Entity, IBlockEntity {




	#region --- VAR ---


	// Api
	[OnCheckPointTouched_CheckPoint_Character] internal static System.Action<CheckPoint, Character> OnCheckPointTouched;
	/// <summary>
	/// Position in unit space for last check point get triggered
	/// </summary>
	public static Int3? LastTriggeredCheckPointUnitPosition { get; private set; } = null;
	/// <summary>
	/// Type ID for last check point get triggered
	/// </summary>
	public static int LastTriggeredCheckPointID { get; private set; } = 0;
	/// <summary>
	/// True if only work when linked altar is unlocked
	/// </summary>
	protected virtual bool RequireAltarUnlock => false;
	protected int LastTriggerFrame = int.MinValue;

	// Short
	private bool AltarAvailable => !RequireAltarUnlock || (LinkedAltarID != 0 && CheckAltar.CurrentAltarID == LinkedAltarID);

	// Data
	private readonly int LinkedAltarID = 0;


	#endregion




	#region --- MSG ---


	[CircuitOperate_Int3UnitPos_IntStamp_Direction5From]
	internal static void CircuitOperator (Int3 unitPos, int _, Direction5 __) => TriggerCheckPoint(unitPos);


	[OnGameRestart]
	internal static void OnGameRestart () {
		LastTriggeredCheckPointUnitPosition = null;
		LastTriggeredCheckPointID = 0;
	}


	public CheckPoint () => CheckAltar.TryGetLinkedID(TypeID, out LinkedAltarID);


	public override void OnActivated () {
		base.OnActivated();
		LastTriggerFrame = int.MinValue;
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
		bool available = AltarAvailable;

		// Player Touch Check
		if (!highlighting && available && player.Rect.Overlaps(Rect)) {
			highlighting = true;
			Touch();
		}

		// Spawn Portal
		if (
			available && highlighting &&
			Stage.GetSpawnedEntityCount(CheckPointPortal.TYPE_ID) == 0 &&
			LinkedAltarID != 0 &&
			CheckAltar.CurrentAltarID == LinkedAltarID &&
			Stage.GetOrSpawnEntity(CheckPointPortal.TYPE_ID, X, Y + Const.CEL * 4) is CheckPointPortal cpPortal
		) {
			cpPortal.SetCheckPoint(LinkedAltarID, CheckAltar.CurrentAltarUnitPos);
		}

	}


	public override void LateUpdate () {
		base.LateUpdate();

		bool available = AltarAvailable;

		// Body
		if (Renderer.TryGetSprite(TypeID, out var bodySp)) {
			var cell = Renderer.Draw(bodySp, Rect);
			if (!available) {
				cell.Shift = bodySp.GlobalBorder;
			}
		}

		// Highlight
		if (available) {
			var unitPos = new Int3(X.ToUnit(), Y.ToUnit(), Stage.ViewZ);
			if (PlayerSystem.RespawnCpUnitPosition == unitPos) {
				var tint = new Color32(128, 255, 128, 255);
				FrameworkUtil.DrawLoopingActivatedHighlight(Rect, tint);
			}
		}
	}


	#endregion




	#region --- API ---


	/// <summary>
	/// Use this function to control logic when player touch
	/// </summary>
	public virtual void Touch () {
		TriggerCheckPoint(TypeID, new Int3(X.ToUnit(), Y.ToUnit(), Stage.ViewZ));
		OnCheckPointTouched?.Invoke(this, PlayerSystem.Selecting);
		LastTriggerFrame = Game.GlobalFrame;
	}


	#endregion




	#region --- LGC ---


	private static void TriggerCheckPoint (Int3 unitPos) {
		int id = WorldSquad.Front.GetBlockAt(unitPos.x, unitPos.y, unitPos.z, BlockType.Entity);
		TriggerCheckPoint(id, unitPos);
	}
	private static void TriggerCheckPoint (int id, Int3 unitPos) {

		LastTriggeredCheckPointUnitPosition = unitPos;
		LastTriggeredCheckPointID = id;
		PlayerSystem.RespawnCpUnitPosition = unitPos;

		// Clear Portal
		if (
			Stage.GetSpawnedEntityCount(CheckPointPortal.TYPE_ID) != 0 &&
			Stage.TryFindEntity(CheckPointPortal.TYPE_ID, out var portal)
		) {
			portal.Active = false;
		}

	}


	#endregion




}