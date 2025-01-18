using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public interface IRouteWalker {

	Direction8 CurrentDirection { get; set; }
	Int2 TargetPosition { get; set; }

	public static Int2 GetNextRoutePosition (IRouteWalker walker, int pathID, int speed, bool allowTurnBack = false, BlockType pathType = BlockType.Element, bool allowTilt = true, HashSet<int> pathSet = null) {

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
			// Get Direction
			bool gotRoute;
			Direction8 newDirection;
			if (pathSet != null) {
				gotRoute = TryGetRouteFromMap(
					pathSet,
					(newPos.x + eWalker.Width / 2).ToUnit(),
					(newPos.y + eWalker.Height / 2).ToUnit(),
					walker.CurrentDirection, out newDirection, pathType, allowTilt
				);
			} else {
				gotRoute = TryGetRouteFromMap(
					pathID,
					(newPos.x + eWalker.Width / 2).ToUnit(),
					(newPos.y + eWalker.Height / 2).ToUnit(),
					walker.CurrentDirection, out newDirection, pathType, allowTilt
				);
			}
			if (gotRoute) {
				walker.CurrentDirection = newDirection;
			} else if (allowTurnBack) {
				walker.CurrentDirection = walker.CurrentDirection.Opposite();
			}
			var normal = walker.CurrentDirection.Normal();
			var targetPos = walker.TargetPosition.ToUnifyGlobal();
			targetPos.x += normal.x * Const.CEL;
			targetPos.y += normal.y * Const.CEL;
			walker.TargetPosition = targetPos;
		}

		// Valid Target Pos
		if (walker.CurrentDirection.IsHorizontal()) {
			newPos.y = newPos.y.MoveTowards((newPos.y + eWalker.Height / 2).ToUnifyGlobal(), 6);
			walker.TargetPosition = new Int2(walker.TargetPosition.x, newPos.y);
		} else if (walker.CurrentDirection.IsVertical()) {
			newPos.x = newPos.x.MoveTowards((newPos.x + eWalker.Width / 2).ToUnifyGlobal(), 6);
			walker.TargetPosition = new Int2(newPos.x, walker.TargetPosition.y);
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

	public static bool TryGetRouteFromMap (int pathID, int unitX, int unitY, Direction8 currentDirection, out Direction8 result, BlockType pathType = BlockType.Element, bool allowTilt = true) {

		var squad = WorldSquad.Front;

		result = currentDirection;
		if (squad.GetBlockAt(unitX, unitY, pathType) == 0) return false;

		var dir = currentDirection.Normal();
		if (squad.GetBlockAt(unitX + dir.x, unitY + dir.y, pathType) == pathID) return true;

		if (allowTilt) {
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
		} else {
			// CW
			result = currentDirection.Clockwise(2);
			dir = result.Normal();
			if (squad.GetBlockAt(unitX + dir.x, unitY + dir.y, pathType) == pathID) return true;
			// ACW
			result = currentDirection.AntiClockwise(2);
			dir = result.Normal();
			if (squad.GetBlockAt(unitX + dir.x, unitY + dir.y, pathType) == pathID) return true;
		}

		return false;
	}

	public static bool TryGetRouteFromMap (HashSet<int> pathSet, int unitX, int unitY, Direction8 currentDirection, out Direction8 result, BlockType pathType = BlockType.Element, bool allowTilt = true) {

		var squad = WorldSquad.Front;

		result = currentDirection;
		if (squad.GetBlockAt(unitX, unitY, pathType) == 0) return false;

		var dir = currentDirection.Normal();
		if (pathSet.Contains(squad.GetBlockAt(unitX + dir.x, unitY + dir.y, pathType))) return true;

		if (allowTilt) {
			for (int i = 0; i < 3; i++) {
				// CW
				result = currentDirection.Clockwise(i + 1);
				dir = result.Normal();
				if (pathSet.Contains(squad.GetBlockAt(unitX + dir.x, unitY + dir.y, pathType))) return true;
				// ACW
				result = currentDirection.AntiClockwise(i + 1);
				dir = result.Normal();
				if (pathSet.Contains(squad.GetBlockAt(unitX + dir.x, unitY + dir.y, pathType))) return true;
			}
		} else {
			// CW
			result = currentDirection.Clockwise(2);
			dir = result.Normal();
			if (pathSet.Contains(squad.GetBlockAt(unitX + dir.x, unitY + dir.y, pathType))) return true;
			// ACW
			result = currentDirection.AntiClockwise(2);
			dir = result.Normal();
			if (pathSet.Contains(squad.GetBlockAt(unitX + dir.x, unitY + dir.y, pathType))) return true;
		}

		return false;
	}

}
