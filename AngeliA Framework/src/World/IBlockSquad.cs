using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public interface IBlockSquad {

	private static readonly Dictionary<int, int> SYSTEM_NUMBER_POOL = new(10) {
		{ NumberZero.TYPE_ID, 0 },
		{ NumberOne.TYPE_ID, 1 },
		{ NumberTwo.TYPE_ID, 2 },
		{ NumberThree.TYPE_ID, 3 },
		{ NumberFour.TYPE_ID, 4 },
		{ NumberFive.TYPE_ID, 5 },
		{ NumberSix.TYPE_ID, 6 },
		{ NumberSeven.TYPE_ID, 7 },
		{ NumberEight.TYPE_ID, 8 },
		{ NumberNine.TYPE_ID, 9 },
	};

	bool WorldExists (int worldX, int worldY, int worldZ);

	int GetBlockAt (int unitX, int unitY, int z, BlockType type);

	(int level, int bg, int entity, int element) GetAllBlocksAt (int unitX, int unitY, int z);

	void SetBlockAt (int unitX, int unitY, int z, BlockType type, int newID);

	public sealed bool ReadSystemNumber (int unitX, int unitY, int z, out int number) {
		number = 0;
		bool hasH = ReadSystemNumber(unitX, unitY, z, Direction4.Right, out int numberH);
		bool hasV = ReadSystemNumber(unitX, unitY, z, Direction4.Down, out int numberV);
		if (!hasH && !hasV) return false;
		if (hasH == hasV) {
			number = Util.Max(numberH, numberV);
			return true;
		} else {
			number = hasH ? numberH : numberV;
			return true;
		}
	}

	public sealed bool ReadSystemNumber (int unitX, int unitY, int z, Direction4 direction, out int number) {

		number = 0;
		int digitCount = 0;
		int x = unitX;
		int y = unitY;
		var delta = direction.Normal();

		// Find Start
		int left = int.MinValue;
		int down = int.MinValue;
		while (HasSystemNumber(x, y, z)) {
			left = x;
			down = y;
			x -= delta.x;
			y -= delta.y;
		}
		if (left == int.MinValue) return false;

		// Get Number
		x = left;
		y = down;
		while (digitCount < 9 && TryGetSingleSystemNumber(x, y, z, out int digit)) {
			number *= 10;
			number += digit;
			digitCount++;
			x += delta.x;
			y += delta.y;
		}
		return digitCount > 0;
	}

	public sealed bool HasSystemNumber (int unitX, int unitY, int z) {
		int id = GetBlockAt(unitX, unitY, z, BlockType.Element);
		return id != 0 && SYSTEM_NUMBER_POOL.ContainsKey(id);
	}

	public sealed bool TryGetSingleSystemNumber (int unitX, int unitY, int z, out int digitValue) {
		digitValue = 0;
		int id = GetBlockAt(unitX, unitY, z, BlockType.Element);
		return id != 0 && SYSTEM_NUMBER_POOL.TryGetValue(id, out digitValue);
	}

}