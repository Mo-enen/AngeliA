using System.Collections;
using System.Collections.Generic;


namespace AngeliaFramework {

	public class CommonWater : Water { }

	// Source
	public class CommonWaterSourceLeft : WaterSource<CommonWater> {
		public override Direction4 Direction => Direction4.Left;
	}
	public class CommonWaterSourceRight : WaterSource<CommonWater> {
		public override Direction4 Direction => Direction4.Right;
	}
	public class CommonWaterSourceDown : WaterSource<CommonWater> {
		public override Direction4 Direction => Direction4.Down;
	}
	public class CommonWaterSourceUp : WaterSource<CommonWater> {
		public override Direction4 Direction => Direction4.Up;
	}




	public abstract class WaterSource<W> : Entity where W : Water {


		// Api
		public virtual int TargetVolume => 400;
		public virtual int SpawnFrequency => 20;
		public abstract Direction4 Direction { get; }

		// Data
		private readonly int WaterTypeID = 0;

		// MSG
		public WaterSource () {
			WaterTypeID = typeof(W).AngeHash();
		}


		public override void PhysicsUpdate () {
			base.PhysicsUpdate();
			// Spawn Water
			if (Game.SettleFrame.UMod(SpawnFrequency) == 0) {
				Water.SpawnWater(WaterTypeID, Rect, Direction, TargetVolume);
			}
		}


	}


	[EntityAttribute.UpdateOutOfRange]
	public abstract class Water : Entity {




		#region --- VAR ---


		// Api
		protected virtual int StableVolume => 60;
		public int Volume { get; set; } = 1000;

		// Data
		private bool IsGrounded = false;
		private int CurrentSpeed = 0;
		private int RequireTransferLeft = 0;
		private int RequireTransferRight = 0;
		private int RequireTransferDown = 0;
		private int RequireTransferUp = 0;
		private int ArtworkHeight = 0;
		private Water WaterLeft = null;
		private Water WaterRight = null;
		private Water WaterDown = null;
		private Water WaterUp = null;


		#endregion




		#region --- MSG ---


		public override void OnActivated () {
			base.OnActivated();
			IsGrounded = false;
			Volume = 1000;
			CurrentSpeed = 0;
			RequireTransferLeft = 0;
			RequireTransferRight = 0;
			RequireTransferDown = 0;
			RequireTransferUp = 0;
			ArtworkHeight = 0;
			WaterLeft = null;
			WaterRight = null;
			WaterUp = null;
		}


		public override void FillPhysics () {
			base.FillPhysics();
			CellPhysics.FillEntity(IsGrounded ? PhysicsLayer.LEVEL : PhysicsLayer.ENVIRONMENT, this, true, Const.WATER_TAG);
		}


		// Before Update
		public override void BeforePhysicsUpdate () {
			base.BeforePhysicsUpdate();

			RequireTransferLeft = 0;
			RequireTransferRight = 0;
			RequireTransferDown = 0;
			RequireTransferUp = 0;
			WaterLeft = null;
			WaterRight = null;
			WaterUp = null;
			IsGrounded = GroundCheck();

			// Movement
			BeforeUpdate_Movement();

			// Try Merge
			if (CellPhysics.GetEntity(
					TypeID, IsGrounded ? new IRect(X + Width / 2, Y, 1, 1) : Rect,
					PhysicsMask.MAP, this, OperationMode.TriggerOnly, Const.WATER_TAG
				) is Water overlapWater
			) {
				MergeWater(this, overlapWater);
			}

			if (IsGrounded) {

				var rect = Rect.Shrink(1);

				// Get Neighbors
				WaterLeft = CellPhysics.GetEntity(
					TypeID, rect.Shift(-Const.CEL, 0), PhysicsMask.LEVEL,
					this, OperationMode.TriggerOnly, Const.WATER_TAG
				) as Water;

				WaterRight = CellPhysics.GetEntity(
					TypeID, rect.Shift(Const.CEL, 0), PhysicsMask.LEVEL,
					this, OperationMode.TriggerOnly, Const.WATER_TAG
				) as Water;

				WaterDown = CellPhysics.GetEntity(
					TypeID, rect.Shift(0, -Const.CEL), PhysicsMask.LEVEL,
					this, OperationMode.TriggerOnly, Const.WATER_TAG
				) as Water;

				WaterUp = CellPhysics.GetEntity(
					TypeID, rect.Shift(0, Const.CEL), PhysicsMask.LEVEL,
					this, OperationMode.TriggerOnly, Const.WATER_TAG
				) as Water;

				// Transfer
				BeforeUpdate_Transfer();

			}

		}


		private void BeforeUpdate_Movement () {
			if (!IsGrounded) {
				// Dropping
				const int GRAVITY = 5;
				const int MAX_SPEED = 48;
				CurrentSpeed = (CurrentSpeed + GRAVITY).Clamp(0, MAX_SPEED);
				Y = CellPhysics.MoveImmediately(
					PhysicsMask.LEVEL, new(X, Y), Direction4.Down,
					CurrentSpeed, new(Width, Height), this, true
				).y;
				Height = (Height + CurrentSpeed).Clamp(0, Const.CEL);
			} else {
				// Grounded
				CurrentSpeed = 0;
				X = X.ToUnifyGlobal();
				Y = Y.ToUnifyGlobal();
				Width = Const.CEL;
				Height = (Const.CEL * Volume.Clamp(0, 1000) / 1000).Clamp(Const.HALF, Const.CEL);
			}
		}


		private void BeforeUpdate_Transfer () {

			int volume = Volume;
			int totalTransfer = volume - StableVolume;
			var rect = Rect.Shrink(1);

			// Balance Down
			if (WaterDown != null && WaterDown.Volume < 1000) {
				RequireTransferDown = volume.Clamp(0, 1000 - WaterDown.Volume);
				volume -= RequireTransferDown;
				totalTransfer -= RequireTransferDown;
			}

			// Transfer Up
			if (volume > 1000) {
				// Balance Up
				if (WaterUp != null && WaterUp.Volume < 1000) {
					RequireTransferUp += (1000 - WaterUp.Volume).Clamp(0, 40);
				}
				// Flow Up
				if (
					WaterUp == null &&
					!CellPhysics.Overlap(PhysicsMask.LEVEL, rect.Shift(0, Const.CEL), this)
				) {
					RequireTransferUp += (volume - 1000).Clamp(0, 1000);
				}
				volume -= RequireTransferUp;
				totalTransfer -= RequireTransferUp;
			}

			// Transfer H
			if (totalTransfer > 0) {

				int transferL = 0;
				int transferR = 0;

				// Balance
				if (WaterLeft != null && volume > WaterLeft.Volume) {
					transferL = (volume - WaterLeft.Volume) / 4;
				}
				if (WaterRight != null && volume > WaterRight.Volume) {
					transferR = (volume - WaterRight.Volume) / 4;
				}

				// Flow
				if (
					volume > StableVolume && WaterLeft == null &&
					!CellPhysics.Overlap(PhysicsMask.LEVEL, rect.Shift(-Const.CEL, 0), this)
				) {
					transferL += volume - StableVolume / 2;
				}
				if (
					volume > StableVolume && WaterRight == null &&
					!CellPhysics.Overlap(PhysicsMask.LEVEL, rect.Shift(Const.CEL, 0), this)
				) {
					transferR += volume - StableVolume / 2;
				}

				// Transfer Fix
				if (transferL + transferR > totalTransfer) {
					if (transferL != 0 && transferR != 0) {
						transferL = transferL * totalTransfer / (transferL + transferR);
						transferR = totalTransfer - transferL;
					} else {
						transferL = transferL.Clamp(0, totalTransfer);
						transferR = transferR.Clamp(0, totalTransfer);
					}
				}
				RequireTransferLeft = transferL;
				RequireTransferRight = transferR;
			}
		}


		// Physics Update
		public override void PhysicsUpdate () {
			base.PhysicsUpdate();

			if (IsGrounded) {
				// Grounded
				var unifyRect = new IRect(X.ToUnifyGlobal(), Y.ToUnifyGlobal(), Const.CEL, Const.CEL);

				// Perform Transfer Left
				if (RequireTransferLeft > 0) {
					if (WaterLeft != null) {
						// Balance
						WaterLeft.Volume += RequireTransferLeft;
						Volume -= RequireTransferLeft;
					} else {
						// Flow
						var newWater = SpawnWater(
							TypeID, unifyRect.Shift(-Const.CEL, 0), Direction4.Left, RequireTransferLeft
						);
						if (newWater != null) Volume -= RequireTransferLeft;
					}
				}

				// Perform Transfer Right
				if (RequireTransferRight > 0) {
					if (WaterRight != null) {
						// Balance
						WaterRight.Volume += RequireTransferRight;
						Volume -= RequireTransferRight;
					} else {
						// Flow
						var newWater = SpawnWater(
							TypeID, unifyRect.Shift(Const.CEL, 0), Direction4.Right, RequireTransferRight
						);
						if (newWater != null) Volume -= RequireTransferRight;
					}
				}

				// Perform Transfer Down
				if (RequireTransferDown > 0) {
					if (WaterDown != null) {
						WaterDown.Volume += RequireTransferDown;
						Volume -= RequireTransferDown;
					}
				}

				// Perform Transfer Up
				if (RequireTransferUp > 0) {
					if (WaterUp != null) {
						// Balance
						WaterUp.Volume += RequireTransferUp;
						Volume -= RequireTransferUp;
					} else {
						// Flow
						var newWater = SpawnWater(
							TypeID, unifyRect.Shift(0, Const.CEL), Direction4.Up, RequireTransferUp
						);
						if (newWater != null) {
							Volume -= RequireTransferUp;
							newWater.IsGrounded = true;
						}
					}
				}

			}
		}


		// Frame Update
		public override void FrameUpdate () {
			base.FrameUpdate();

			if (Volume == 0 || !Active) {
				Active = false;
				return;
			}

			if (CellRenderer.TryGetSpriteFromGroup(TypeID, Game.GlobalFrame / 5, out var sprite, true, true)) {

				bool showTopBorder = IsGrounded && WaterUp == null;
				bool showBottomBorder = !IsGrounded;

				// Rect
				var rect = Rect;
				if (IsGrounded) {
					if (WaterUp != null) {
						rect.height = Const.CEL;
					} else {
						rect.height = Const.CEL * Volume.Clamp(0, 1000) / 1000;
					}
				} else {
					rect.height = Const.CEL * Volume.Clamp(0, 1000) / 1000;
				}

				// Lerp Height
				if (IsGrounded) {
					rect.height = ArtworkHeight = ArtworkHeight.LerpTo(
						rect.height.Clamp(Const.HALF / 2, Const.CEL),
						WaterUp != null ? 1000 : 10
					);
				} else {
					ArtworkHeight = Const.HALF / 2;
				}

				// Tint
				var tint = Const.WHITE;
				const int TINT_DURATION = 10;
				if (Game.GlobalFrame - SpawnFrame < TINT_DURATION) {
					tint.a = (byte)Util.Remap(0, TINT_DURATION, 0, 255, Game.GlobalFrame - SpawnFrame);
				}

				// Draw
				if (showTopBorder) {
					rect = rect.Expand(0, 0, 0, sprite.GlobalBorder.up);
				}
				if (showBottomBorder) {
					rect = rect.Expand(0, 0, sprite.GlobalBorder.down, 0);
				}
				CellRenderer.Draw_9Slice(
					sprite.GlobalID,
					rect,
					sprite.GlobalBorder.left,
					sprite.GlobalBorder.right,
					showBottomBorder ? sprite.GlobalBorder.down : 0,
					showTopBorder ? sprite.GlobalBorder.up : 0,
					tint
				);
			}

		}


		#endregion




		#region --- API ---


		public static Water SpawnWater (int waterID, IRect sourceRect, Direction4 direction, int volume) {
			const int width = Const.HALF;
			const int height = Const.HALF;
			var targetRect = new IRect(
				direction switch {
					Direction4.Left => sourceRect.x + sourceRect.width - width,
					Direction4.Right => sourceRect.x,
					Direction4.Down or Direction4.Up => sourceRect.CenterX() - width / 2,
					_ => sourceRect.x,
				},
				direction switch {
					Direction4.Left or Direction4.Right => sourceRect.y,
					Direction4.Down => sourceRect.yMax - height,
					Direction4.Up => sourceRect.y,
					_ => sourceRect.y,
				},
				width, height
			);

			if (
				Stage.TrySpawnEntity(waterID, targetRect.x, targetRect.y, out var entity) &&
				entity is Water water
			) {
				water.Volume = volume;
				water.X = targetRect.x;
				water.Y = targetRect.y;
				water.Width = targetRect.width;
				water.Height = targetRect.height;
				return water;
			}

			return null;
		}


		public static bool MergeWater (Water a, Water b) {
			if (a == null || b == null || !a.Active || !b.Active) return false;
			var newWater = a.SpawnFrame > b.SpawnFrame ? a : b;
			var oldWater = a.SpawnFrame > b.SpawnFrame ? b : a;
			if (newWater.IsGrounded && !oldWater.IsGrounded) {
				(newWater, oldWater) = (oldWater, newWater);
			}
			oldWater.Volume += newWater.Volume;
			newWater.Volume = 0;
			newWater.Active = false;
			return true;
		}


		#endregion




		#region --- LGC ---


		private bool GroundCheck () {
			if (IsGrounded && CellPhysics.Overlap(
				PhysicsMask.LEVEL, new IRect(X + 1, Y - Const.CEL + 1, Const.CEL - 2, Const.CEL - 2),
				out var hit, this, OperationMode.TriggerOnly, Const.WATER_TAG
			) && hit.Entity is Water water && water.IsGrounded) return true;
			return !CellPhysics.RoomCheck(
				PhysicsMask.LEVEL, Rect, this, Direction4.Down
			) || !CellPhysics.RoomCheckOneway(
				PhysicsMask.LEVEL, Rect, this, Direction4.Down, true, true
			);
		}


		#endregion



	}

}