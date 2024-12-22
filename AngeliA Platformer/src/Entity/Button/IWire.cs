using System.Collections.Generic;
using System.Collections;
using AngeliA;

namespace AngeliA.Platformer;

public interface IWire {
	private static readonly HashSet<int> WireIdPool = [];
	[OnGameInitialize]
	internal static void OnGameInitialize () {
		WireIdPool.Clear();
		foreach (var type in typeof(IWire).AllClassImplemented()) {
			WireIdPool.Add(type.AngeHash());
		}
	}
	public static bool IsWire (int id) => WireIdPool.Contains(id);
}