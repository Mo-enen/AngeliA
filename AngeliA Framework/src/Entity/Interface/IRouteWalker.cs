using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public interface IRouteWalker {

	Direction8 CurrentDirection { get; set; }
	Int2 TargetPosition { get; set; }

	public static Int2 GetNextRoutePosition (IRouteWalker walker, int pathID, int speed, bool allowTurnBack = false, BlockType pathType = BlockType.Element, bool allowTilt = true, HashSet<int> pathSet = null) {
		if (walker is not Entity eWalker) return default;
		var currentDir = walker.CurrentDirection;
		var targetPos = walker.TargetPosition;
		var result = GetNextRoutePosition(eWalker, ref currentDir, ref targetPos, pathID, speed, allowTurnBack, pathType, allowTilt, pathSet);
		walker.CurrentDirection = currentDir;
		walker.TargetPosition = targetPos;
		return result;
	}

	public static Int2 GetNextRoutePosition (Entity entity, ref Direction8 currentDirection, ref Int2 targetPosition, int pathID, int speed, bool allowTurnBack = false, BlockType pathType = BlockType.Element, bool allowTilt = true, HashSet<int> pathSet = null) {

		var newPos = entity.XY;

		// Over Moved
		if (currentDirection switch {
			Direction8.Left or Direction8.TopLeft => newPos.x <= targetPosition.x,
			Direction8.Right or Direction8.BottomRight => newPos.x >= targetPosition.x,
			Direction8.Bottom or Direction8.BottomLeft => newPos.y <= targetPosition.y,
			Direction8.Top or Direction8.TopRight => newPos.y >= targetPosition.y,
			_ => false,
		}) {
			// Get Direction
			bool gotRoute;
			Direction8 newDirection;
			if (pathSet != null) {
				gotRoute = TryGetRouteFromMap(
					pathSet,
					(newPos.x + entity.Width / 2).ToUnit(),
					(newPos.y + entity.Height / 2).ToUnit(),
					currentDirection, out newDirection, pathType, allowTilt
				);
			} else {
				gotRoute = TryGetRouteFromMap(
					pathID,
					(newPos.x + entity.Width / 2).ToUnit(),
					(newPos.y + entity.Height / 2).ToUnit(),
					currentDirection, out newDirection, pathType, allowTilt
				);
			}
			if (gotRoute) {
				currentDirection = newDirection;
			} else if (allowTurnBack) {
				currentDirection = currentDirection.Opposite();
			}
			var normal = currentDirection.Normal();
			var targetPos = targetPosition.ToUnifyGlobal();
			targetPos.x += normal.x * Const.CEL;
			targetPos.y += normal.y * Const.CEL;
			targetPosition = targetPos;
		}

		// Valid Target Pos
		if (currentDirection.IsHorizontal()) {
			newPos.y = newPos.y.MoveTowards((newPos.y + entity.Height / 2).ToUnifyGlobal(), 6);
			targetPosition = new Int2(targetPosition.x, newPos.y);
		} else if (currentDirection.IsVertical()) {
			newPos.x = newPos.x.MoveTowards((newPos.x + entity.Width / 2).ToUnifyGlobal(), 6);
			targetPosition = new Int2(newPos.x, targetPosition.y);
		}

		// Move
		var currentNormal = currentDirection.Normal();
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
