using System.Collections;
using System.Collections.Generic;


using AngeliA;namespace AngeliA.Platformer;


public abstract class SnakePlatform : StepTriggerPlatform, IRouteWalker {


	// Api
	protected sealed override IUnitable.UniteMode TriggerMode => IUnitable.UniteMode.Horizontal;
	public abstract int EndBreakDuration { get; }
	public abstract int Speed { get; }
	public Direction8 CurrentDirection { get; set; }
	public Int2 TargetPosition { get; set; }

	// Data
	private Int2 StartPosition = default;
	private SnakePlatform Head = null;
	private int EndReachingFrame = int.MinValue;
	private int ArtworkScale = 0;


	// MSG
	public override void OnActivated () {
		base.OnActivated();
		EndReachingFrame = int.MinValue;
		CurrentDirection = Direction8.Right;
		TargetPosition = new(X, Y);
		StartPosition.x = X;
		StartPosition.y = Y;
		Head = null;
		ArtworkScale = 0;
	}


	protected override void Move () {

		// Touched Check
		if (TriggeredData == null) return;

		// Check Head Reach End
		if (Head != null && EndReachingFrame < 0) EndReachingFrame = Head.EndReachingFrame;

		// Reached End
		if (EndReachingFrame >= 0) {
			if (Game.GlobalFrame > EndReachingFrame + EndBreakDuration) {
				FrameworkUtil.InvokeObjectFreeFall(TypeID, X + Width / 2, Y + Height / 2, rotation: 0);
				// Reset
				X = StartPosition.x;
				Y = StartPosition.y;
				OnActivated();
			}
			return;
		}

		// Move
		IRouteWalker.MoveToRoute(this, PlatformPath.TYPE_ID, Speed, out int newX, out int newY);
		X = newX;
		Y = newY;

		// Stop Check
		if (Head == null && Physics.Overlap(
			PhysicsMask.LEVEL, new(TargetPosition.x + Const.HALF, TargetPosition.y + Const.HALF, 1, 1), null
		)) {
			EndReachingFrame = Game.GlobalFrame;
		}

	}


	public override void LateUpdate () {
		base.LateUpdate();

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


	public override void Trigger (object data = null) {
		base.Trigger(data);

		var left = this;
		var right = this;
		int y = Y + Height / 2;

		// L
		for (int x = -Const.HALF; ; x -= Const.CEL) {
			if (Physics.GetEntity(
				TypeID,
				new IRect(X + x, y, 1, 1), PhysicsMask.ENVIRONMENT, this, OperationMode.ColliderAndTrigger
			) is not SnakePlatform snake) break;
			snake.TriggeredData = data;
			left = snake;
		}

		// R
		for (int x = Const.CEL + Const.HALF; ; x += Const.CEL) {
			if (Physics.GetEntity(
				TypeID,
				new IRect(X + x, y, 1, 1), PhysicsMask.ENVIRONMENT, this, OperationMode.ColliderAndTrigger
			) is not SnakePlatform snake) break;
			snake.TriggeredData = data;
			right = snake;
		}

		// Non-Head Snake Direction
		if (left != right) {
			Direction8 targetDir = Direction8.Right;
			var head = right;

			// Get Head
			if (WorldSquad.Front.GetBlockAt(
				(right.X + right.Width / 2).ToUnit() + 1,
				(right.Y + right.Height / 2).ToUnit(),
				BlockType.Element
			) == PlatformPath.TYPE_ID) {
				targetDir = Direction8.Right;
				right.CurrentDirection = Direction8.Right;
				head = right;
			} else if (WorldSquad.Front.GetBlockAt(
				(left.X + left.Width / 2).ToUnit() - 1,
				(left.Y + left.Height / 2).ToUnit(),
				BlockType.Element
			) == PlatformPath.TYPE_ID) {
				targetDir = Direction8.Left;
				left.CurrentDirection = Direction8.Left;
				head = left;
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
			CurrentDirection = Direction8.Right;
			int unitX = (X + Width / 2).ToUnit();
			int unitY = (Y + Height / 2).ToUnit();
			if (WorldSquad.Front.GetBlockAt(unitX + 1, unitY, BlockType.Element) == PlatformPath.TYPE_ID) {
				CurrentDirection = Direction8.Right;
				Head = this;
			} else if (WorldSquad.Front.GetBlockAt(unitX - 1, unitY, BlockType.Element) == PlatformPath.TYPE_ID) {
				CurrentDirection = Direction8.Left;
				Head = this;
			}

		}
	}


}