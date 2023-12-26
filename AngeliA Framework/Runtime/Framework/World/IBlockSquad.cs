using System.Collections;
using System.Collections.Generic;


namespace AngeliaFramework {
	public interface IBlockSquad {

		private static readonly Dictionary<int, int> SYSTEM_NUMBER_POOL = new(10) {
			{ typeof(Number0).AngeHash(), 0 },
			{ typeof(Number1).AngeHash(), 1 },
			{ typeof(Number2).AngeHash(), 2 },
			{ typeof(Number3).AngeHash(), 3 },
			{ typeof(Number4).AngeHash(), 4 },
			{ typeof(Number5).AngeHash(), 5 },
			{ typeof(Number6).AngeHash(), 6 },
			{ typeof(Number7).AngeHash(), 7 },
			{ typeof(Number8).AngeHash(), 8 },
			{ typeof(Number9).AngeHash(), 9 },
		};

		int GetBlockAt (int unitX, int unitY, int z, BlockType type);

		void SetBlockAt (int unitX, int unitY, int z, BlockType type, int newID);

		public sealed int GetBlockAt (int unitX, int unitY, int z) {
			int result = GetBlockAt(unitX, unitY, z, BlockType.Entity);
			if (result == 0) result = GetBlockAt(unitX, unitY, z, BlockType.Level);
			if (result == 0) result = GetBlockAt(unitX, unitY, z, BlockType.Background);
			return result;
		}

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
			int id = GetBlockAt(unitX, unitY, z, BlockType.Entity);
			return id != 0 && SYSTEM_NUMBER_POOL.ContainsKey(id);
		}

		public sealed bool TryGetSingleSystemNumber (int unitX, int unitY, int z, out int digitValue) {
			digitValue = 0;
			int id = GetBlockAt(unitX, unitY, z, BlockType.Entity);
			return id != 0 && SYSTEM_NUMBER_POOL.TryGetValue(id, out digitValue);
		}

	}
}