using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {


	[EntityAttribute.MapEditorGroup("Platform")]
	public class eSnakePath : IMapEditorItem { }



	public abstract class SnakePlatform : Platform {


		// Const
		private static readonly int PATH_ID = typeof(eSnakePath).AngeHash();

		// Api
		public abstract int EndBreakDuration { get; }
		public abstract int Speed { get; }

		// Data
		private Direction4 CurrentDirection = Direction4.Right;
		private Vector2Int TargetPosition = default;
		private Vector2Int StartPosition = default;
		private SnakePlatform Head = null;
		private bool PrevTouched = false;
		private int EndReachingFrame = int.MinValue;
		private int ArtworkScale = 0;


		// MSG
		public override void OnActivated () {
			base.OnActivated();
			PrevTouched = false;
			EndReachingFrame = int.MinValue;
			CurrentDirection = Direction4.Right;
			TargetPosition.x = X;
			TargetPosition.y = Y;
			StartPosition.x = X;
			StartPosition.y = Y;
			Head = null;
			ArtworkScale = 0;
		}


		protected override void Move () {

			// Touched Check
			if (!TouchedByPlayer) return;

			// Check Head Reach End
			if (Head != null && EndReachingFrame < 0) EndReachingFrame = Head.EndReachingFrame;

			// Reached End
			if (EndReachingFrame >= 0) {
				if (Game.GlobalFrame > EndReachingFrame + EndBreakDuration) {
					X = StartPosition.x;
					Y = StartPosition.y;
					OnActivated();
				}
				return;
			}

			// Over Moved
			if (CurrentDirection switch {
				Direction4.Left => X <= TargetPosition.x,
				Direction4.Right => X >= TargetPosition.x,
				Direction4.Down => Y <= TargetPosition.y,
				Direction4.Up => Y >= TargetPosition.y,
				_ => false,
			}) {
				// Fix Position Back
				const int HALF = Const.HALF;
				X -= (X + HALF).UMod(Const.CEL) - HALF;
				Y -= (Y + HALF).UMod(Const.CEL) - HALF;

				// Get Direction
				if (GetDirectionIgnoreOpposite(CurrentDirection, out var newDirection)) {
					CurrentDirection = newDirection;
				}
				var normal = CurrentDirection.Normal();
				TargetPosition.x += normal.x * Const.CEL;
				TargetPosition.y += normal.y * Const.CEL;

				// Stop Check
				if (Head == null && CellPhysics.Overlap(
					Const.MASK_LEVEL, new(TargetPosition.x + HALF, TargetPosition.y + HALF, 1, 1), null
				)) {
					EndReachingFrame = Game.GlobalFrame;
				}

			}

			// Move
			var currentNormal = CurrentDirection.Normal();
			X += currentNormal.x * Speed;
			Y += currentNormal.y * Speed;

		}


		public override void FrameUpdate () {
			// Touch Check
			if (TouchedByPlayer && !PrevTouched) {
				PrevTouched = true;
				TouchAllNeighbors();
			}
			// Artwork
			Cell cell;
			if (EndReachingFrame < 0) {
				cell = CellRenderer.Draw(TypeID, Rect);
				if (ArtworkScale != 1000) {
					cell.Width = cell.Width * ArtworkScale / 1000;
					cell.Height = cell.Height * ArtworkScale / 1000;
					int offset = Util.RemapUnclamped(0, 1000, Const.HALF, 0, ArtworkScale);
					cell.X += offset;
					cell.Y += offset;
				}
			} else {
				int shakeX = ((Game.GlobalFrame + Y / Const.CEL).PingPong(6) - 3) * 6;
				int shakeY = ((Game.GlobalFrame + X / Const.CEL).PingPong(6) - 3) * 6;
				int rot = (Game.GlobalFrame + (X + Y) / Const.CEL).PingPong(6) - 3;
				int width = Width * ArtworkScale / 1000;
				int height = Height * ArtworkScale / 1000;
				CellRenderer.Draw(TypeID, X + Width / 2 + shakeX, Y + Height / 2 + shakeY, 500, 500, rot, width, height);
			}
			ArtworkScale = ArtworkScale < 995 ? ArtworkScale.LerpTo(1000, 200) : 1000;
		}


		// API
		public bool GetDirectionIgnoreOpposite (Direction4 currentDirection, out Direction4 result, bool ignoreTypeID = true) {
			result = currentDirection;
			if (HasSnakeBlockAtDirection(result, ignoreTypeID)) return true;
			result = currentDirection.Clockwise();
			if (HasSnakeBlockAtDirection(result, ignoreTypeID)) return true;
			result = currentDirection.AntiClockwise();
			if (HasSnakeBlockAtDirection(result, ignoreTypeID)) return true;
			return false;
		}


		public bool HasSnakeBlockAtDirection (Direction4 direction, bool ignoreTypeID) {
			var normal = direction.Normal();
			int unitX = (X + Width / 2).UDivide(Const.CEL) + normal.x;
			int unitY = (Y + Height / 2).UDivide(Const.CEL) + normal.y;
			int id = WorldSquad.Front.GetBlockAt(unitX, unitY, BlockType.Entity);
			return id == PATH_ID || (!ignoreTypeID && id == TypeID);
		}


		// LGC
		private void TouchAllNeighbors () {

			var left = this;
			var right = this;
			int y = Y + Height / 2;

			// L
			for (int x = -Const.HALF; ; x -= Const.CEL) {
				var snake = CellPhysics.GetEntity<SnakePlatform>(
					new RectInt(X + x, y, 1, 1), Const.MASK_ENVIRONMENT, this, OperationMode.ColliderAndTrigger
				);
				if (snake == null) break;
				snake.PrevTouched = true;
				snake.SetPlayerTouch(true);
				left = snake;
			}

			// R
			for (int x = Const.CEL + Const.HALF; ; x += Const.CEL) {
				var snake = CellPhysics.GetEntity<SnakePlatform>(
					new RectInt(X + x, y, 1, 1), Const.MASK_ENVIRONMENT, this, OperationMode.ColliderAndTrigger
				);
				if (snake == null) break;
				snake.PrevTouched = true;
				snake.SetPlayerTouch(true);
				right = snake;
			}

			// Non-Head Snake Direction
			if (left != right) {
				// Get Head
				Direction4 targetDir = Direction4.Right;
				var head = right;
				if (right.GetDirectionIgnoreOpposite(Direction4.Right, out var _resultR, false)) {
					targetDir = Direction4.Right;
					head = right;
					right.CurrentDirection = _resultR;
				} else if (left.GetDirectionIgnoreOpposite(Direction4.Left, out var _resultL, false)) {
					targetDir = Direction4.Left;
					head = left;
					left.CurrentDirection = _resultL;
				}
				// Set Direction
				int leftX = left.X + left.Width / 2;
				int rightX = right.X + right.Width;
				for (int x = leftX; x < rightX; x += Const.CEL) {
					var snake = CellPhysics.GetEntity<SnakePlatform>(
						new RectInt(x, y, 1, 1), Const.MASK_ENVIRONMENT,
						null, OperationMode.ColliderAndTrigger
					);
					if (snake == null) continue;
					if (snake == head) {
						snake.Head = null;
						continue;
					}
					snake.CurrentDirection = targetDir;
					snake.Head = head;
				}
			} else {
				// Single Snake
				Head = null;
				CurrentDirection = Direction4.Right;
				if (GetDirectionIgnoreOpposite(Direction4.Right, out var _resultR, false)) {
					CurrentDirection = _resultR;
					Head = this;
				} else if (GetDirectionIgnoreOpposite(Direction4.Left, out var _resultL, false)) {
					CurrentDirection = _resultL;
					Head = this;
				}
			}
		}


	}
}