using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {
	[EntityAttribute.Capacity(16)]
	[EntityAttribute.MapEditorGroup("CheckPoint")]
	public abstract class CheckPoint : Entity, IActionTarget {




		#region --- VAR ---


		// Const
		private static readonly int HINT_TELEPORT = "CtrlHint.TeleportCP".AngeHash();

		// Api
		public static int OnTouchedParticleID { get; set; } = 0;
		public static int LastInvokedCheckPointID { get; private set; } = 0;
		public static Vector3Int LastInvokedCheckPointUnitPosition { get; private set; } = default;
		protected virtual bool OnlySpawnWhenUnlocked => true;

		// Short
		private static string UnlockFolderPath => Util.CombinePaths(AngePath.PlayerDataRoot, "Unlocked CP");
		private Vector4Int Border => CellRenderer.TryGetSprite(TypeID, out var sprite) ? sprite.GlobalBorder : Vector4Int.Zero;

		// Data
		private static readonly HashSet<int> UnlockedCheckPoint = new();
		private static readonly Dictionary<int, int> LinkedId = new();
		private static int LoadedSlot = 0;


		#endregion




		#region --- MSG ---


		[OnGameInitialize(-128)]
		public static void BeforeGameInitialize () {
			LinkedId.Clear();
			LoadUnlockFromFile();
		}


		[OnGameRestart]
		public static void OnGameRestart () {
			if (LoadedSlot != AngePath.CurrentDataSlot) {
				LoadUnlockFromFile();
			}
		}


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
			if (player.Rect.Overlaps(Rect)) {
				var unitPos = new Vector3Int(X.ToUnit(), Y.ToUnit(), Stage.ViewZ);
				if (!Player.RespawnUnitPosition.HasValue || Player.RespawnUnitPosition != unitPos) {
					Player.RespawnUnitPosition = unitPos;
					OnPlayerTouched(unitPos);
				}
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
				var cell = CellRenderer.Draw(TypeID, Rect);
				// Highlight
				if ((this as IActionTarget).IsHighlighted) {
					IActionTarget.HighlightBlink(cell, Direction3.Horizontal, FittingPose.Single);
					// Hint
					ControlHintUI.DrawGlobalHint(
						X, Y + Height + Const.CEL * 2, Gamekey.Action,
						Language.Get(HINT_TELEPORT, "Teleport to Altar"), true
					);
				}
				var unitPos = new Vector3Int(X.ToUnit(), Y.ToUnit(), Stage.ViewZ);
				if (Player.RespawnUnitPosition == unitPos) {
					DrawActivatedHighlight();
				}
			}
		}


		protected virtual void OnPlayerTouched (Vector3Int unitPos) {
			// Particle
			if (Stage.SpawnEntity(
				OnTouchedParticleID,
				X + Const.HALF, Y + Const.HALF
			) is Particle particle) {
				particle.Width = Const.CEL * 2;
				particle.Height = Const.CEL * 2;
				particle.UserData = this;
			}
		}


		#endregion




		#region --- API ---


		public static bool IsUnlocked (int id) => UnlockedCheckPoint.Contains(id);


		public static void Link (int idA, int idB) {
			LinkedId.TryAdd(idA, idB);
			LinkedId.TryAdd(idB, idA);
		}


		public static void Unlock (int checkPointID) {
			if (!UnlockedCheckPoint.Contains(checkPointID)) {
				Util.ByteToFile(new byte[0], Util.CombinePaths(UnlockFolderPath, checkPointID.ToString()));
				UnlockedCheckPoint.Add(checkPointID);
			}
		}


		public static void ClearLastInvoke () => LastInvokedCheckPointID = 0;


		void IActionTarget.Invoke () {
			if (!IsUnlocked(TypeID)) return;
			if (!LinkedId.TryGetValue(TypeID, out int linkedID)) return;
			if (!IGlobalPosition.TryGetPosition(linkedID, out var unitPos)) return;
			LastInvokedCheckPointID = TypeID;
			LastInvokedCheckPointUnitPosition = new Vector3Int(X.ToUnit(), Y.ToUnit(), Stage.ViewZ);
			var player = Player.Selecting;
			var task = TeleportTask.Teleport(
				player.X, player.Y,
				unitPos.x.ToGlobal() + Const.HALF,
				unitPos.y.ToGlobal(),
				unitPos.z
			);
			if (task != null) {
				task.TeleportEntity = player;
				task.WaitDuration = 30;
				task.Duration = 60;
				task.UseVignette = true;
			}
		}


		bool IActionTarget.AllowInvoke () => IsUnlocked(TypeID);


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
			LoadedSlot = AngePath.CurrentDataSlot;
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