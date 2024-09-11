using System.Collections;
using System.Collections.Generic;


namespace AngeliA;


public abstract class SnakePlatform : Platform {


	// Api
	public abstract int EndBreakDuration { get; }
	public abstract int Speed { get; }

	// Data
	private Direction4 CurrentDirection = Direction4.Right;
	private Int2 TargetPosition = default;
	private Int2 StartPosition = default;
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
				// Particle
				GlobalEvent.InvokeObjectFreeFall(TypeID, X + Width / 2, Y + Height / 2, rotation: 0);
				// Reset
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
			if (FrameworkUtil.GetPlatformRoute((X + Width / 2).ToUnit(), (Y + Height / 2).ToUnit(), CurrentDirection, out var newDirection)) {
				CurrentDirection = newDirection;
			}
			var normal = CurrentDirection.Normal();
			TargetPosition.x += normal.x * Const.CEL;
			TargetPosition.y += normal.y * Const.CEL;

			// Stop Check
			if (Head == null && Physics.Overlap(
				PhysicsMask.LEVEL, new(TargetPosition.x + HALF, TargetPosition.y + HALF, 1, 1), null
			)) {
				EndReachingFrame = Game.GlobalFrame;
			}

		}

		// Move
		var currentNormal = CurrentDirection.Normal();
		X += currentNormal.x * Speed;
		Y += currentNormal.y * Speed;

	}


	public override void LateUpdate () {
		// Touch Check
		if (TouchedByPlayer && !PrevTouched) {
			PrevTouched = true;
			TouchAllNeighbors();
		}
		// Artwork
		Cell cell;
		if (EndReachingFrame < 0) {
			cell = Renderer.Draw(TypeID, Rect);
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
			Renderer.Draw(TypeID, X + Width / 2 + shakeX, Y + Height / 2 + shakeY, 500, 500, rot, width, height);
		}
		ArtworkScale = ArtworkScale < 995 ? ArtworkScale.LerpTo(1000, 200) : 1000;
	}


	// LGC
	private void TouchAllNeighbors () {

		var left = this;
		var right = this;
		int y = Y + Height / 2;

		// L
		for (int x = -Const.HALF; ; x -= Const.CEL) {
			if (Physics.GetEntity(
				TypeID,
				new IRect(X + x, y, 1, 1), PhysicsMask.ENVIRONMENT, this, OperationMode.ColliderAndTrigger
			) is not SnakePlatform snake) break;
			snake.PrevTouched = true;
			snake.SetTouch();
			left = snake;
		}

		// R
		for (int x = Const.CEL + Const.HALF; ; x += Const.CEL) {
			if (Physics.GetEntity(
				TypeID,
				new IRect(X + x, y, 1, 1), PhysicsMask.ENVIRONMENT, this, OperationMode.ColliderAndTrigger
			) is not SnakePlatform snake) break;
			snake.PrevTouched = true;
			snake.SetTouch();
			right = snake;
		}

		// Non-Head Snake Direction

		if (left != right) {
			// Get Head
			Direction4 targetDir = Direction4.Right;
			var head = right;
			if (FrameworkUtil.GetPlatformRoute(
				(right.X + right.Width / 2).ToUnit(),
				(right.Y + right.Height / 2).ToUnit(),
				Direction4.Right, out var _resultR, TypeID
			)) {
				targetDir = Direction4.Right;
				head = right;
				right.CurrentDirection = _resultR;
			} else if (FrameworkUtil.GetPlatformRoute(
				(left.X + left.Width / 2).ToUnit(),
				(left.Y + left.Height / 2).ToUnit(),
				Direction4.Left, out var _resultL, TypeID
			)) {
				targetDir = Direction4.Left;
				head = left;
				left.CurrentDirection = _resultL;
			}
			// Set Direction
			int leftX = left.X + left.Width / 2;
			int rightX = right.X + right.Width;
			for (int x = leftX; x < rightX; x += Const.CEL) {
				if (Physics.GetEntity(
					TypeID,
					new IRect(x, y, 1, 1), PhysicsMask.ENVIRONMENT,
					null, OperationMode.ColliderAndTrigger
				) is not SnakePlatform snake) continue;
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
			int unitX = (X + Width / 2).ToUnit();
			int unitY = (Y + Height / 2).ToUnit();
			if (FrameworkUtil.GetPlatformRoute(unitX, unitY, Direction4.Right, out var _resultR, TypeID)) {
				CurrentDirection = _resultR;
				Head = this;
			} else if (FrameworkUtil.GetPlatformRoute(unitX, unitY, Direction4.Left, out var _resultL, TypeID)) {
				CurrentDirection = _resultL;
				Head = this;
			}
		}
	}


}