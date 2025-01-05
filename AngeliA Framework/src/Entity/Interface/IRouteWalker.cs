namespace AngeliA;


public interface IRouteWalker {

	Direction8 CurrentDirection { get; set; }
	Int2 TargetPosition { get; set; }

	public static void MoveToRoute (IRouteWalker walker, int pathID, int speed, out int newX, out int newY) {

		newX = 0;
		newY = 0;
		if (walker is not Entity eWalker) return;
		newX = eWalker.X;
		newY = eWalker.Y;

		// Over Moved
		if (walker.CurrentDirection switch {
			Direction8.Left or Direction8.TopLeft => newX <= walker.TargetPosition.x,
			Direction8.Right or Direction8.BottomRight => newX >= walker.TargetPosition.x,
			Direction8.Bottom or Direction8.BottomLeft => newY <= walker.TargetPosition.y,
			Direction8.Top or Direction8.TopRight => newY >= walker.TargetPosition.y,
			_ => false,
		}) {
			// Fix Position Back
			int lostX = (newX + Const.HALF).UMod(Const.CEL) - Const.HALF;
			int lostY = (newY + Const.HALF).UMod(Const.CEL) - Const.HALF;
			newX -= lostX;
			newY -= lostY;

			// Get Direction
			if (GetRouteFromMap(
				(newX + eWalker.Width / 2).ToUnit(),
				(newY + eWalker.Height / 2).ToUnit(),
				walker.CurrentDirection, out var newDirection, pathID
			)) {
				walker.CurrentDirection = newDirection;
			}
			var normal = walker.CurrentDirection.Normal();
			var targetPos = walker.TargetPosition;
			targetPos.x += normal.x * Const.CEL;
			targetPos.y += normal.y * Const.CEL;
			walker.TargetPosition = targetPos;

			// Compensate Lost Length
			speed += lostX.Abs() + lostY.Abs();

		}

		// Move
		var currentNormal = walker.CurrentDirection.Normal();
		if (currentNormal.x != 0 && currentNormal.y != 0) {
			speed = speed * 100000 / 141421;
		}
		newX += currentNormal.x * speed;
		newY += currentNormal.y * speed;

	}

	public static bool GetRouteFromMap (int unitX, int unitY, Direction8 currentDirection, out Direction8 result, int pathID) {

		result = currentDirection;
		if (WorldSquad.Front.GetBlockAt(unitX, unitY, BlockType.Element) == 0) {
			return false;
		}

		var dir = currentDirection.Normal();
		if (HasPathIndicator(pathID, unitX + dir.x, unitY + dir.y)) return true;

		for (int i = 0; i < 3; i++) {
			// CW
			result = currentDirection.Clockwise(i + 1);
			dir = result.Normal();
			if (HasPathIndicator(pathID, unitX + dir.x, unitY + dir.y)) return true;
			// ACW
			result = currentDirection.AntiClockwise(i + 1);
			dir = result.Normal();
			if (HasPathIndicator(pathID, unitX + dir.x, unitY + dir.y)) return true;
		}

		return false;

		// Func
		static bool HasPathIndicator (int pathID, int unitX, int unitY) {
			return WorldSquad.Front.GetBlockAt(unitX, unitY, BlockType.Element) == pathID;
		}
	}

}
