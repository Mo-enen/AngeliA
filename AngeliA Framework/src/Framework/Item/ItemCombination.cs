using System.Collections.Generic;
using System.Text;

namespace AngeliA;


public class CombinationData {
	public int Order;
	public int Result;
	public int ResultCount;
	public int Keep0;
	public int Keep1;
	public int Keep2;
	public int Keep3;
	public bool Keep (int id) => Keep0 == id || Keep1 == id || Keep2 == id || Keep3 == id;
}


public static class ItemCombination {




	#region --- VAR ---



	#endregion




	#region --- API ---


	public static IEnumerable<KeyValuePair<Int4, CombinationData>> ForAllCombinationInFile (string filePath) {

		if (!Util.FileExists(filePath)) yield break;

		int count = 0;
		var builder = new StringBuilder();
		foreach (string _line in Util.ForAllLinesInFile(filePath)) {
			if (string.IsNullOrEmpty(_line)) continue;
			string line = _line.TrimWhiteForStartAndEnd();
			if (line.StartsWith('#')) continue;
			// Order
			builder.Clear();
			var com = Int4.zero;
			var keep = Int4.zero;
			int appendingComIndex = 0;
			bool appendingResultCount = false;
			int resultID = 0;
			int resultCount = 1;
			foreach (var c in line) {
				if (c == ' ') continue;
				if (c == '+' || c == '=') {
					if (builder.Length > 0 && appendingComIndex < 4) {
						bool keepCurrent = false;
						if (builder[0] == '^') {
							builder.Remove(0, 1);
							keepCurrent = true;
						}
						com[appendingComIndex] = builder.ToString().AngeHash();
						keep[appendingComIndex] = keepCurrent ? com[appendingComIndex] : 0;
						appendingComIndex++;
					}
					if (c == '=') {
						appendingResultCount = true;
					}
					builder.Clear();
				} else {
					if (appendingResultCount && !char.IsDigit(c)) {
						appendingResultCount = false;
						if (builder.Length > 0 && int.TryParse(builder.ToString(), out int _resultCount)) {
							resultCount = _resultCount;
						}
						builder.Clear();
					}
					builder.Append(c);
				}
			}

			// Result
			if (builder.Length > 0) {
				resultID = builder.ToString().AngeHash();
			}

			// Add to Pool
			if (com != Int4.zero && resultCount >= 1 && resultID != 0) {
				var from = GetSortedCombination(com.x, com.y, com.z, com.w);
				yield return new(from, new CombinationData() {
					Order = count,
					Result = resultID,
					ResultCount = resultCount,
					Keep0 = keep[0],
					Keep1 = keep[1],
					Keep2 = keep[2],
					Keep3 = keep[3],
				});
				count++;
			}
		}
	}


	public static bool TryGetCombinationFromPool (
		Dictionary<Int4, CombinationData> pool, int item0, int item1, int item2, int item3,
		out int result, out int resultCount,
		out int keep0, out int keep1, out int keep2, out int keep3
	) {
		var from = GetSortedCombination(item0, item1, item2, item3);
		if (pool.TryGetValue(from, out var resultValue)) {
			result = resultValue.Result;
			resultCount = resultValue.ResultCount;
			keep0 = resultValue.Keep0;
			keep1 = resultValue.Keep1;
			keep2 = resultValue.Keep2;
			keep3 = resultValue.Keep3;
			return true;
		}
		result = 0;
		resultCount = 0;
		keep0 = keep1 = keep2 = keep3 = 0;
		return false;
	}


	public static Int4 GetSortedCombination (int a, int b, int c, int d) {

		// Sort for Zero
		if (a == 0 && b != 0) (a, b) = (b, a);
		if (b == 0 && c != 0) (b, c) = (c, b);
		if (c == 0 && d != 0) (c, d) = (d, c);
		if (a == 0 && b != 0) (a, b) = (b, a);
		if (b == 0 && c != 0) (b, c) = (c, b);
		if (a == 0 && b != 0) (a, b) = (b, a);

		// Sort for Size
		if (a != 0 && b != 0 && a > b) (a, b) = (b, a);
		if (b != 0 && c != 0 && b > c) (b, c) = (c, b);
		if (c != 0 && d != 0 && c > d) (c, d) = (d, c);
		if (a != 0 && b != 0 && a > b) (a, b) = (b, a);
		if (b != 0 && c != 0 && b > c) (b, c) = (c, b);
		if (a != 0 && b != 0 && a > b) (a, b) = (b, a);

		return new Int4(a, b, c, d);
	}


	#endregion




	#region --- LGC ---



	#endregion




}