using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {
	[EntityAttribute.Capacity(16)]
	[EntityAttribute.MapEditorGroup("CheckPoint")]
	public abstract class CheckPoint : Entity {




		#region --- VAR ---


		// Api
		public delegate void TouchedHandler (CheckPoint checkPoint, Character target);
		public static event TouchedHandler OnCheckPointTouched;
		public static int BackPortalEntityID { get; set; } = 0;
		public static Vector3Int? TurnBackUnitPosition { get; private set; } = null;
		protected virtual bool OnlySpawnWhenUnlocked => true;

		// Short
		private static string UnlockFolderPath => Util.CombinePaths(AngePath.PlayerDataRoot, "Unlocked CP");
		private Vector4Int Border => CellRenderer.TryGetSprite(TypeID, out var sprite) ? sprite.GlobalBorder : Vector4Int.zero;

		// Data
		private static readonly HashSet<int> UnlockedCheckPoint = new();
		private static readonly Dictionary<int, int> LinkedId = new();


		#endregion




		#region --- MSG ---


		[OnGameInitialize(-128)]
		[OnSlotChanged]
		public static void OnGameInitialize () => LoadUnlockFromFile();


		public override void FillPhysics () {
			base.FillPhysics();
			if (!OnlySpawnWhenUnlocked || IsUnlocked(TypeID)) {
				// Unlocked
				CellPhysics.FillEntity(Const.LAYER_ENVIRONMENT, this, true);
			}
			CellPhysics.FillBlock(
				Const.LAYER_ENVIRONMENT, TypeID, Rect.Shrink(Border), true, Const.ONEWAY_UP_TAG
			);
		}


		public override void PhysicsUpdate () {

			base.PhysicsUpdate();

			if (OnlySpawnWhenUnlocked && !IsUnlocked(TypeID)) return;
			var player = Player.Selecting;

			if (player == null || !player.Active) return;
			var unitPos = new Vector3Int(X.ToUnit(), Y.ToUnit(), Stage.ViewZ);
			bool highlighting = Player.RespawnCpUnitPosition.HasValue && Player.RespawnCpUnitPosition.Value == unitPos;

			// Player Touch Check
			if (player.Rect.Overlaps(Rect) && !highlighting) {
				highlighting = true;
				OnPlayerTouched(unitPos);
				if (TryGetTurnBackUnitPosition(out var turnBackUnitPos)) TurnBackUnitPosition = turnBackUnitPos;
				Player.RespawnCpUnitPosition = unitPos;
			}

			// Spawn Portal
			if (
				highlighting &&
				TurnBackUnitPosition.HasValue &&
				Stage.GetSpawnedEntityCount(BackPortalEntityID) == 0
			) {
				Stage.GetOrAddEntity(BackPortalEntityID, X, Y + Const.CEL * 4);
			}

		}


		public override void FrameUpdate () {
			base.FrameUpdate();
			if (OnlySpawnWhenUnlocked && !IsUnlocked(TypeID)) {
				// Locked
				var cell = CellRenderer.Draw(TypeID, Rect);
				cell.Shift = Border;
			} else {
				// Unlocked
				CellRenderer.Draw(TypeID, Rect);
				var unitPos = new Vector3Int(X.ToUnit(), Y.ToUnit(), Stage.ViewZ);
				if (Player.RespawnCpUnitPosition == unitPos) {
					DrawActivatedHighlight();
				}
			}
		}


		protected virtual void OnPlayerTouched (Vector3Int unitPos) {
			// Clear Portal
			if (
				Stage.GetSpawnedEntityCount(BackPortalEntityID) != 0 &&
				Stage.TryGetEntity(BackPortalEntityID, out var portal)
			) {
				portal.Active = false;
			}
			// Particle
			OnCheckPointTouched?.Invoke(this, Player.Selecting);
		}


		protected virtual bool TryGetTurnBackUnitPosition (out Vector3Int unitPos) {
			unitPos = default;
			return
				LinkedId.TryGetValue(TypeID, out int linkedID) &&
				IGlobalPosition.TryGetPosition(linkedID, out unitPos);
		}


		#endregion




		#region --- API ---


		public static bool IsUnlocked (int id) => UnlockedCheckPoint.Contains(id);


		public static void Link (int idA, int idB) {
			LinkedId.TryAdd(idA, idB);
			LinkedId.TryAdd(idB, idA);
		}


		public static void Unlock (int checkPointID) {
			if (UnlockedCheckPoint.Contains(checkPointID)) return;
			Util.ByteToFile(new byte[0], Util.CombinePaths(UnlockFolderPath, checkPointID.ToString()));
			UnlockedCheckPoint.Add(checkPointID);
		}


		protected void DrawActivatedHighlight () {
			const int LINE_COUNT = 4;
			const int DURATION = 22;
			int localFrame = Game.GlobalFrame % DURATION;
			var rect = Rect;
			var tint = new Color32(128, 255, 128, 255);
			CellRenderer.SetLayerToAdditive();
			for (int i = 0; i < LINE_COUNT; i++) {
				tint.a = (byte)(i == LINE_COUNT - 1 ? Util.RemapUnclamped(0, DURATION, 64, 0, localFrame) : 64);
				rect.y = Y;
				rect.height = i * Height / LINE_COUNT;
				rect.height += Util.RemapUnclamped(0, DURATION, 0, Height / LINE_COUNT, localFrame);
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