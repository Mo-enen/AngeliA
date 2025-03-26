using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

/// <summary>
/// Interface that makes an entity walks along a given path
/// </summary>
public interface IRouteWalker {

	/// <summary>
	/// Which direction this entity is currently walking
	/// </summary>
	Direction8 CurrentDirection { get; set; }

	/// <summary>
	/// Where does this entity currently walking 
	/// </summary>
	Int2 TargetPosition { get; set; }

	/// <inheritdoc cref="GetNextRoutePosition(Entity, ref Direction8, ref Int2, int, int, bool, BlockType, bool, HashSet{int})"/>
	public static Int2 GetNextRoutePosition (IRouteWalker walker, int pathID, int speed, bool allowTurnBack = false, BlockType pathType = BlockType.Element, bool allowTilt = true, HashSet<int> pathSet = null) {
		if (walker is not Entity eWalker) return default;
		var currentDir = walker.CurrentDirection;
		var targetPos = walker.TargetPosition;
		var result = GetNextRoutePosition(eWalker, ref currentDir, ref targetPos, pathID, speed, allowTurnBack, pathType, allowTilt, pathSet);
		walker.CurrentDirection = currentDir;
		walker.TargetPosition = targetPos;
		return result;
	}

	/// <summary>
	/// Get the position in global space that the walker entity should go to
	/// </summary>
	/// /// <param name="walker">Target walker</param>
	/// <param name="entity">Target entity that walks</param>
	/// <param name="currentDirection">Which direction this entity is currently walking</param>
	/// <param name="targetPosition">Where does this entity currently walking</param>
	/// <param name="pathID">Which map block should be treat as the path marker</param>
	/// <param name="speed">Movement speed in global space</param>
	/// <param name="allowTurnBack">True if the walker turn back when reach the edge</param>
	/// <param name="pathType">Block type of the path marker</param>
	/// <param name="allowTilt">True if the walker can walk diagonally</param>
	/// <param name="pathSet">A hash set of path marks, set to null if there's only one mark</param>
	/// <returns>The final position in global space</returns>
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

	/// <inheritdoc cref="TryGetRouteFromMap(HashSet{int}, int, int, Direction8, out Direction8, BlockType, bool)"/>
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

	/// <summary>
	/// Get path marker block from map
	/// </summary>
	/// /// <param name="pathID">ID of the path marker block</param>
	/// <param name="pathSet">A hash set of path marks, set to null if there's only one mark</param>
	/// <param name="unitX">Position X in unit space</param>
	/// <param name="unitY">Position Y in unit space</param>
	/// <param name="currentDirection">Which direction this entity is currently walking</param>
	/// <param name="result">Direction the walker should go</param>
	/// <param name="pathType">Block type of the path marker</param>
	/// <param name="allowTilt">True if the walker can walk diagonally</param>
	/// <returns>True if the path is successfuly found</returns>
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
