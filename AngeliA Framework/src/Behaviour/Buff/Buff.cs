using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public abstract class Buff {




	#region --- VAR ---


	// Api
	public int TypeID { get; init; }
	public int NameID { get; init; }
	public int DescriptionID { get; init; }
	public string TypeName { get; init; }
	public static int AllBuffCount => AllBuffs.Length;

	// Data
	private static readonly Dictionary<int, int> BuffIndexPool = [];
	private static Buff[] AllBuffs = [new FailbackBuff()];


	#endregion




	#region --- MSG ---


	[OnGameInitialize(-128)]
	internal static void OnGameInitialize () {
		var failback = new FailbackBuff();
		var buffList = new List<Buff> { failback };
		BuffIndexPool.Add(failback.TypeID, 0);
		foreach (var type in typeof(Buff).AllChildClass()) {
			if (System.Activator.CreateInstance(type) is not Buff buff) continue;
			if (buff is FailbackBuff) continue;
			if (BuffIndexPool.ContainsKey(buff.TypeID)) continue;
			BuffIndexPool.Add(buff.TypeID, buffList.Count);
			buffList.Add(buff);
		}
		AllBuffs = [.. buffList];
	}


	public Buff () {
		string name = GetType().AngeName();
		TypeID = name.AngeHash();
		NameID = $"iName.{name}".AngeHash();
		DescriptionID = $"iDes.{name}".AngeHash();
		TypeName = name;
	}


	public virtual void BeforeUpdate (Character target) { }
	public virtual void LateUpdate (Character target) { }
	public virtual void OnCharacterAttack (Character target, Bullet bullet) { }


	#endregion




	#region --- API ---


	public static bool TryGetBuffIndex (int id, out int index) => BuffIndexPool.TryGetValue(id, out index);


	public static Buff GetBuffAtIndex (int index) => AllBuffs[index];


	public static string GetBuffDisplayName (int id) {
		if (!TryGetBuffIndex(id, out int index)) return "";
		var buff = AllBuffs[index];
		return Language.Get(buff.NameID, buff.TypeName);
	}


	public static string GetBuffDescription (int id) {
		if (!TryGetBuffIndex(id, out int index)) return "";
		return Language.Get(AllBuffs[index].DescriptionID);
	}


	#endregion




}
