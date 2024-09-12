namespace AngeliA;


public interface IRouteWalker {

	Direction8 CurrentDirection { get; set; }
	Int2 TargetPosition { get; set; }

	public static void MoveToRoute (IRouteWalker walker, int pathID, int speed) {

		if (walker is not Entity eWalker) return;

		// Over Moved
		if (walker.CurrentDirection switch {
			Direction8.Left or Direction8.TopLeft => eWalker.X <= walker.TargetPosition.x,
			Direction8.Right or Direction8.BottomRight => eWalker.X >= walker.TargetPosition.x,
			Direction8.Bottom or Direction8.BottomLeft => eWalker.Y <= walker.TargetPosition.y,
			Direction8.Top or Direction8.TopRight => eWalker.Y >= walker.TargetPosition.y,
			_ => false,
		}) {
			// Fix Position Back
			eWalker.X -= (eWalker.X + Const.HALF).UMod(Const.CEL) - Const.HALF;
			eWalker.Y -= (eWalker.Y + Const.HALF).UMod(Const.CEL) - Const.HALF;

			// Get Direction
			if (GetRouteFromMap(
				(eWalker.X + eWalker.Width / 2).ToUnit(),
				(eWalker.Y + eWalker.Height / 2).ToUnit(),
				walker.CurrentDirection, out var newDirection, pathID
			)) {
				walker.CurrentDirection = newDirection;
			}
			var normal = walker.CurrentDirection.Normal();
			var targetPos = walker.TargetPosition;
			targetPos.x += normal.x * Const.CEL;
			targetPos.y += normal.y * Const.CEL;
			walker.TargetPosition = targetPos;
		}

		// Move
		var currentNormal = walker.CurrentDirection.Normal();
		if (currentNormal.x != 0 && currentNormal.y != 0) {
			speed = speed * 100000 / 141421;
		}
		eWalker.X += currentNormal.x * speed;
		eWalker.Y += currentNormal.y * speed;

	}

	public static bool GetRouteFromMap (int unitX, int unitY, Direction8 currentDirection, out Direction8 result, int pathID, int ignoreTypeID = 0) {

		result = currentDirection;
		var dir = currentDirection.Normal();
		if (HasPathIndicatorAtDirection(pathID, unitX + dir.x, unitY + dir.y, ignoreTypeID)) return true;

		for (int i = 0; i < 3; i++) {
			// CW
			result = currentDirection.Clockwise(i + 1);
			dir = result.Normal();
			if (HasPathIndicatorAtDirection(pathID, unitX + dir.x, unitY + dir.y, ignoreTypeID)) return true;
			// ACW
			result = currentDirection.AntiClockwise(i + 1);
			dir = result.Normal();
			if (HasPathIndicatorAtDirection(pathID, unitX + dir.x, unitY + dir.y, ignoreTypeID)) return true;
		}

		return false;

		// Func
		static bool HasPathIndicatorAtDirection (int pathID, int unitX, int unitY, int ignoreTypeID) {
			int id = WorldSquad.Front.GetBlockAt(unitX, unitY, BlockType.Element);
			return id == pathID || (ignoreTypeID != 0 && id == ignoreTypeID);
		}
	}

}



