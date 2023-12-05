using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {
	[EntityAttribute.Capacity(16)]
	[EntityAttribute.MapEditorGroup("CheckPoint")]
	public abstract class CheckPoint : EnvironmentEntity {




		#region --- VAR ---


		// Api
		public delegate void TouchedHandler (CheckPoint checkPoint, Character target);
		public static event TouchedHandler OnCheckPointTouched;
		public static Vector3Int? LastTriggeredCheckPointUnitPosition { get; private set; } = null;
		public static int LastTriggeredCheckPointID { get; private set; } = 0;

		// Short
		private static string UnlockFolderPath => Util.CombinePaths(AngePath.PlayerDataRoot, "Unlocked CP");

		// Data
		private static readonly HashSet<int> UnlockedCheckPoint = new();
		private readonly int LinkedAltarID = 0;


		#endregion




		#region --- MSG ---


		[OnGameInitialize(-128)]
		[OnSlotChanged]
		public static void OnGameInitialize () => LoadUnlockFromFile();


		[OnGameRestart]
		public static void OnGameRestart () {
			LastTriggeredCheckPointUnitPosition = null;
			LastTriggeredCheckPointID = 0;
		}


		public CheckPoint () => CheckAltar<CheckPoint>.TryGetLinkedID(TypeID, out LinkedAltarID);


		public override void FillPhysics () {
			base.FillPhysics();
			if (IsUnlocked(TypeID)) {
				// Unlocked
				CellPhysics.FillEntity(PhysicsLayer.ENVIRONMENT, this, true);
			}
			var border = CellRenderer.TryGetSprite(TypeID, out var sprite) ? sprite.GlobalBorder : Vector4Int.zero;
			CellPhysics.FillBlock(
				PhysicsLayer.ENVIRONMENT, TypeID, Rect.Shrink(border), true, Const.ONEWAY_UP_TAG
			);
		}


		public override void PhysicsUpdate () {

			base.PhysicsUpdate();

			if (!IsUnlocked(TypeID)) return;
			var player = Player.Selecting;

			if (player == null || !player.Active) return;
			var unitPos = new Vector3Int(X.ToUnit(), Y.ToUnit(), Stage.ViewZ);
			bool highlighting = Player.RespawnCpUnitPosition.HasValue && Player.RespawnCpUnitPosition.Value == unitPos;

			// Player Touch Check
			if (!highlighting && player.Rect.Overlaps(Rect)) {
				highlighting = true;

				LastTriggeredCheckPointUnitPosition = new Vector3Int(X.ToUnit(), Y.ToUnit(), Stage.ViewZ);
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
				IGlobalPosition.TryGetPosition(LinkedAltarID, out var altarUnitPos) &&
				Stage.GetSpawnedEntityCount(CheckPointPortal.TYPE_ID) == 0 &&
				Stage.GetOrAddEntity(CheckPointPortal.TYPE_ID, X, Y + Const.CEL * 4) is CheckPointPortal cpPortal
			) {
				cpPortal.SetCheckPoint(LinkedAltarID, altarUnitPos);
			}

		}


		public override void FrameUpdate () {
			base.FrameUpdate();
			if (!IsUnlocked(TypeID)) {
				// Locked
				var cell = CellRenderer.Draw(TypeID, Rect);
				cell.Shift = CellRenderer.TryGetSprite(TypeID, out var sprite) ? sprite.GlobalBorder : Vector4Int.zero;
			} else {
				// Unlocked
				CellRenderer.Draw(TypeID, Rect);
				var unitPos = new Vector3Int(X.ToUnit(), Y.ToUnit(), Stage.ViewZ);
				if (Player.RespawnCpUnitPosition == unitPos) {
					DrawActivatedHighlight(Rect);
				}
			}
		}


		#endregion




		#region --- API ---


		public static bool IsUnlocked (int id) => UnlockedCheckPoint.Contains(id);


		public static void Unlock (int checkPointID) {
			if (UnlockedCheckPoint.Contains(checkPointID)) return;
			UnlockedCheckPoint.Add(checkPointID);
			Util.ByteToFile(new byte[0], Util.CombinePaths(UnlockFolderPath, checkPointID.ToString()));
		}


		public static void DrawActivatedHighlight (RectInt targetRect) {
			const int LINE_COUNT = 4;
			const int DURATION = 22;
			int localFrame = Game.GlobalFrame % DURATION;
			var rect = targetRect;
			var tint = new Color32(128, 255, 128, 255);
			CellRenderer.SetLayerToAdditive();
			for (int i = 0; i < LINE_COUNT; i++) {
				tint.a = (byte)(i == LINE_COUNT - 1 ? Util.RemapUnclamped(0, DURATION, 64, 0, localFrame) : 64);
				rect.y = targetRect.y;
				rect.height = i * targetRect.height / LINE_COUNT;
				rect.height += Util.RemapUnclamped(0, DURATION, 0, targetRect.height / LINE_COUNT, localFrame);
				CellRenderer.Draw("Soft Line H".AngeHash(), rect, tint);
			}
			CellRenderer.SetLayerToDefault();
		}


		#endregion




		#region --- LGC ---


		private static void LoadUnlockFromFile () {
			UnlockedCheckPoint.Clear();
			foreach (var path in Util.EnumerateFiles(UnlockFolderPath, true, "*")) {
				if (int.TryParse(Util.GetNameWithoutExtension(path), out int id)) {
					UnlockedCheckPoint.TryAdd(id);
				}
			}
		}


		#endregion




	}
}