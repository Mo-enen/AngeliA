using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AngeliA;


public class CombinationData {
	public int Result;
	public int ResultCount;
	public int DontConsume0;
	public int DontConsume1;
	public int DontConsume2;
	public int DontConsume3;
}


public static class ItemCombination {




	#region --- VAR ---



	#endregion




	#region --- API ---


	public static void LoadCombinationFromFile (Dictionary<Int4, CombinationData> pool, string filePath) {

		if (!Util.FileExists(filePath)) return;

		var builder = new StringBuilder();
		foreach (string _line in Util.ForAllLines(filePath)) {
			if (string.IsNullOrEmpty(_line)) continue;
			string line = _line.TrimWhiteForStartAndEnd();
			if (line.StartsWith('#')) continue;
			builder.Clear();
			var com = Int4.zero;
			var noConsume = Int4.zero;
			int appendingComIndex = 0;
			bool appendingResultCount = false;
			int resultID = 0;
			int resultCount = 1;
			foreach (var c in line) {
				if (c == '+' || c == '=') {
					if (builder.Length > 0 && appendingComIndex < 4) {
						if (builder[0] == '^') {
							builder.Remove(0, 1);
							noConsume[appendingComIndex] = 1;
						}
						com[appendingComIndex] = int.TryParse(builder.ToString(), out int _item) ? _item : 0;
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
					} else {
						builder.Append(c);
					}
				}
			}

			// Result
			if (builder.Length > 0) {
				resultID = int.TryParse(builder.ToString(), out int _res) ? _res : 0;
			}

			// Add to Pool
			if (com != Int4.zero && resultCount >= 1 && resultID != 0) {
				var from = GetSortedCombination(com.x, com.y, com.z, com.w);
				if (!pool.ContainsKey(from)) {
					pool[from] = new CombinationData() {
						Result = resultID,
						ResultCount = resultCount,
						DontConsume0 = noConsume[0] == 0 ? 0 : com.x,
						DontConsume1 = noConsume[1] == 0 ? 0 : com.y,
						DontConsume2 = noConsume[2] == 0 ? 0 : com.z,
						DontConsume3 = noConsume[3] == 0 ? 0 : com.w,
					};
				}

			}
		}
	}


	public static void SaveCombinationToFile (Dictionary<Int4, CombinationData> pool, string filePath) {

		var builder = new StringBuilder();

		foreach (var (com, data) in pool) {

			if (com.x != 0) {
				if (data.DontConsume0 == 0) builder.Append('^');
				builder.Append(com.x);
			}
			if (com.y != 0) {
				builder.Append('+');
				if (data.DontConsume1 == 0) builder.Append('^');
				builder.Append(com.y);
			}
			if (com.z != 0) {
				builder.Append('+');
				if (data.DontConsume2 == 0) builder.Append('^');
				builder.Append(com.z);
			}
			if (com.w != 0) {
				builder.Append('+');
				if (data.DontConsume3 == 0) builder.Append('^');
				builder.Append(com.w);
			}
			builder.Append('=');
			builder.Append(data.ResultCount);
			builder.Append('*');
			builder.Append(data.Result);
			builder.Append('\n');

		}

		Util.TextToFile(builder.ToString(), filePath);

	}


	public static bool TryGetCombinationFromPool (
		Dictionary<Int4, CombinationData> pool, int item0, int item1, int item2, int item3,
		out int result, out int resultCount,
		out int ignoreConsume0, out int ignoreConsume1, out int ignoreConsume2, out int ignoreConsume3
	) {
		var from = GetSortedCombination(item0, item1, item2, item3);
		if (pool.TryGetValue(from, out var resultValue)) {
			result = resultValue.Result;
			resultCount = resultValue.ResultCount;
			ignoreConsume0 = resultValue.DontConsume0;
			ignoreConsume1 = resultValue.DontConsume1;
			ignoreConsume2 = resultValue.DontConsume2;
			ignoreConsume3 = resultValue.DontConsume3;
			return true;
		}
		result = 0;
		resultCount = 0;
		ignoreConsume0 = ignoreConsume1 = ignoreConsume2 = ignoreConsume3 = 0;
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