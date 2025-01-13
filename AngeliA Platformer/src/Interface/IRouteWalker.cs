using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliA.Platformer;

public interface IRouteWalker {
	Direction8 CurrentDirection { get; set; }
	Int2 TargetPosition { get; set; }
	public static Int2 GetNextRoutePosition (IRouteWalker walker, int pathID, int speed, bool allowTurnBack = false, BlockType pathType = BlockType.Element) {

		var newPos = new Int2();
		if (walker is not Entity eWalker) return newPos;
		newPos.x = eWalker.X;
		newPos.y = eWalker.Y;

		// Over Moved
		if (walker.CurrentDirection switch {
			Direction8.Left or Direction8.TopLeft => newPos.x <= walker.TargetPosition.x,
			Direction8.Right or Direction8.BottomRight => newPos.x >= walker.TargetPosition.x,
			Direction8.Bottom or Direction8.BottomLeft => newPos.y <= walker.TargetPosition.y,
			Direction8.Top or Direction8.TopRight => newPos.y >= walker.TargetPosition.y,
			_ => false,
		}) {
			// Fix Position Back
			int lostX = (newPos.x + Const.HALF).UMod(Const.CEL) - Const.HALF;
			int lostY = (newPos.y + Const.HALF).UMod(Const.CEL) - Const.HALF;
			newPos.x -= lostX;
			newPos.y -= lostY;

			// Get Direction
			if (TryGetRouteFromMap(
				pathID,
				(newPos.x + eWalker.Width / 2).ToUnit(),
				(newPos.y + eWalker.Height / 2).ToUnit(),
				walker.CurrentDirection, out var newDirection, pathType
			)) {
				walker.CurrentDirection = newDirection;
			} else if (allowTurnBack) {
				walker.CurrentDirection = walker.CurrentDirection.Opposite();
			}
			var normal = walker.CurrentDirection.Normal();
			var targetPos = walker.TargetPosition.ToUnifyGlobal();
			targetPos.x += normal.x * Const.CEL;
			targetPos.y += normal.y * Const.CEL;
			walker.TargetPosition = targetPos;

			// Compensate Lost Length
			speed += lostX.Abs() + lostY.Abs();

		}

		// Valid Target Pos
		if (walker.CurrentDirection.IsHorizontal()) {
			walker.TargetPosition = new Int2(walker.TargetPosition.x, newPos.y.ToUnifyGlobal());
		} else if (walker.CurrentDirection.IsVertical()) {
			walker.TargetPosition = new Int2(newPos.x.ToUnifyGlobal(), walker.TargetPosition.y);
		}

		// Move
		var currentNormal = walker.CurrentDirection.Normal();
		if (currentNormal.x != 0 && currentNormal.y != 0) {
			speed = speed * 100000 / 141421;
		}
		newPos.x += currentNormal.x * speed;
		newPos.y += currentNormal.y * speed;

		return newPos;
	}
	public static bool TryGetRouteFromMap (int pathID, int unitX, int unitY, Direction8 currentDirection, out Direction8 result, BlockType pathType = BlockType.Element) {

		var squad = WorldSquad.Front;

		result = currentDirection;
		if (squad.GetBlockAt(unitX, unitY, pathType) == 0) return false;

		var dir = currentDirection.Normal();
		if (squad.GetBlockAt(unitX + dir.x, unitY + dir.y, pathType) == pathID) return true;

		for (int i = 0; i < 3; i++) {
			// CW
			result = currentDirection.Clockwise(i + 1);
			dir = result.Normal();
			if (squad.GetBlockAt(unitX + dir.x, unitY + dir.y, pathType) == pathID) return true;
			// ACW
			result = currentDirection.AntiClockwise(i + 1);
			dir = result.Normal();
			if (squad.GetBlockAt(unitX + dir.x, unitY + dir.y, pathType) == pathID) return true;
		}

		return false;
	}
}
