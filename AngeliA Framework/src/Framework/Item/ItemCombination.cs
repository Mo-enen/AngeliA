using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace AngeliA;


public class CombinationData {
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


	public static void LoadCombinationPoolFromCode (Dictionary<Int4, CombinationData> pool) {
		pool.Clear();
		foreach (var type in typeof(Item).AllChildClass()) {
			var iComs = type.GetCustomAttributes<ItemCombinationAttribute>(false);
			if (iComs == null) continue;
			foreach (var com in iComs) {
				if (com.Count <= 0) continue;
				if (
					com.ItemA == null && com.ItemB == null &&
					com.ItemC == null && com.ItemD == null
				) continue;
				int idA = com.ItemA != null ? com.ItemA.AngeHash() : 0;
				int idB = com.ItemB != null ? com.ItemB.AngeHash() : 0;
				int idC = com.ItemC != null ? com.ItemC.AngeHash() : 0;
				int idD = com.ItemD != null ? com.ItemD.AngeHash() : 0;
				var key = GetSortedCombination(idA, idB, idC, idD);
				if (pool.ContainsKey(key)) continue;
				pool.Add(key, new CombinationData() {
					Result = type.AngeHash(),
					ResultCount = com.Count,
					Keep0 = com.ConsumeA ? 0 : idA,
					Keep1 = com.ConsumeB ? 0 : idB,
					Keep2 = com.ConsumeC ? 0 : idC,
					Keep3 = com.ConsumeD ? 0 : idD,
				});
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




}