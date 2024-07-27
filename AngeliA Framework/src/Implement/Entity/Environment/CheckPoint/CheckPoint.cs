using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;


namespace AngeliA;
[EntityAttribute.Capacity(16)]
[EntityAttribute.MapEditorGroup("CheckPoint")]
public abstract class CheckPoint : EnvironmentEntity {




	#region --- VAR ---


	// Api
	public delegate void TouchedHandler (CheckPoint checkPoint, Character target);
	public static event TouchedHandler OnCheckPointTouched;
	public static Int3? LastTriggeredCheckPointUnitPosition { get; private set; } = null;
	public static int LastTriggeredCheckPointID { get; private set; } = 0;
	public static bool UnlockedPoolReady { get; private set; } = false;

	// Short
	private static string UnlockFolderPath => Util.CombinePaths(Universe.BuiltIn.SavingMetaRoot, "Unlocked CP");

	// Data
	private static readonly HashSet<int> UnlockedCheckPoint = new();
	private readonly int LinkedAltarID = 0;


	#endregion




	#region --- MSG ---


	[OnGameInitializeLater]
	public static void OnGameInitializeLater () {
		// Load Unlock from File
		UnlockedCheckPoint.Clear();
		foreach (var path in Util.EnumerateFiles(UnlockFolderPath, true, "*")) {
			if (int.TryParse(Util.GetNameWithoutExtension(path), out int id)) {
				UnlockedCheckPoint.TryAdd(id);
			}
		}
		UnlockedPoolReady = true;
	}


	[OnGameRestart]
	public static void OnGameRestart () {
		LastTriggeredCheckPointUnitPosition = null;
		LastTriggeredCheckPointID = 0;
	}


	public CheckPoint () => CheckAltar<CheckPoint>.TryGetLinkedID(TypeID, out LinkedAltarID);


	public override void FirstUpdate () {
		base.FirstUpdate();
		if (IsUnlocked(TypeID)) {
			// Unlocked
			Physics.FillEntity(PhysicsLayer.ENVIRONMENT, this, true);
		}
		var border = Renderer.TryGetSprite(TypeID, out var sprite) ? sprite.GlobalBorder : Int4.zero;
		Physics.FillBlock(
			PhysicsLayer.ENVIRONMENT, TypeID, Rect.Shrink(border), true, Tag.OnewayUp
		);
	}


	public override void Update () {

		base.Update();

		if (!IsUnlocked(TypeID)) return;
		var player = Player.Selecting;

		if (player == null || !player.Active) return;
		var unitPos = new Int3(X.ToUnit(), Y.ToUnit(), Stage.ViewZ);
		bool highlighting = Player.RespawnCpUnitPosition.HasValue && Player.RespawnCpUnitPosition.Value == unitPos;

		// Player Touch Check
		if (!highlighting && player.Rect.Overlaps(Rect)) {
			highlighting = true;

			LastTriggeredCheckPointUnitPosition = new Int3(X.ToUnit(), Y.ToUnit(), Stage.ViewZ);
			LastTriggeredCheckPointID = TypeID;

			// Clear Portal
			if (
				Stage.GetSpawnedEntityCount(CheckPointPortal.TYPE_ID) != 0 &&
				Stage.TryGetEntity(CheckPointPortal.TYPE_ID, out var portal)
			) {
				portal.Active = false;
			}

			// Player Respawn
			Player.RespawnCpUnitPosition = unitPos;

			// Particle
			OnCheckPointTouched?.Invoke(this, Player.Selecting);

		}

		// Spawn Portal
		if (
			highlighting &&
			Stage.GetSpawnedEntityCount(CheckPointPortal.TYPE_ID) == 0 &&
			CheckAltar<CheckPoint>.TryGetAltarPosition(TypeID, out var altarUnitPos) &&
			Stage.GetOrAddEntity(CheckPointPortal.TYPE_ID, X, Y + Const.CEL * 4) is CheckPointPortal cpPortal
		) {
			cpPortal.SetCheckPoint(LinkedAltarID, altarUnitPos);
		}

	}


	public override void LateUpdate () {
		base.LateUpdate();
		if (!IsUnlocked(TypeID)) {
			// Locked
			var cell = Renderer.Draw(TypeID, Rect);
			cell.Shift = Renderer.TryGetSprite(TypeID, out var sprite) ? sprite.GlobalBorder : Int4.zero;
		} else {
			// Unlocked
			Renderer.Draw(TypeID, Rect);
			var unitPos = new Int3(X.ToUnit(), Y.ToUnit(), Stage.ViewZ);
			if (Player.RespawnCpUnitPosition == unitPos) {
				DrawActivatedHighlight(Rect);
			}
		}
	}


	#endregion




	#region --- API ---


	public static bool IsUnlocked (int id) => UnlockedCheckPoint.Contains(id);


	public static void Unlock (int checkPointID) {
		if (UnlockedCheckPoint.Add(checkPointID)) {
			Util.BytesToFile(new byte[0], Util.CombinePaths(UnlockFolderPath, checkPointID.ToString()));
		}
	}


	public static void DrawActivatedHighlight (IRect targetRect) {
		const int LINE_COUNT = 4;
		const int DURATION = 22;
		int localFrame = Game.GlobalFrame % DURATION;
		var rect = targetRect;
		var tint = new Color32(128, 255, 128, 255);
		Renderer.SetLayerToAdditive();
		for (int i = 0; i < LINE_COUNT; i++) {
			tint.a = (byte)(i == LINE_COUNT - 1 ? Util.RemapUnclamped(0, DURATION, 64, 0, localFrame) : 64);
			rect.y = targetRect.y;
			rect.height = i * targetRect.height / LINE_COUNT;
			rect.height += Util.RemapUnclamped(0, DURATION, 0, targetRect.height / LINE_COUNT, localFrame);
			Renderer.Draw(BuiltInSprite.SOFT_LINE_H, rect, tint);
		}
		Renderer.SetLayerToDefault();
	}


	#endregion




}