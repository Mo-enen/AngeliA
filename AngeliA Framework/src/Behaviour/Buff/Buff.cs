using System.Collections;
using System.Collections.Generic;

namespace AngeliA;


public abstract class Buff {




	#region --- VAR ---


	// Api
	public int TypeID { get; init; }
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


	public Buff () => TypeID = GetType().AngeHash();


	#endregion




	#region --- API ---


	public static bool TryGetBuffIndex (int id, out int index) => BuffIndexPool.TryGetValue(id, out index);


	public static Buff GetBuffAtIndex (int index) => AllBuffs[index];


	public virtual void BeforeUpdate (Character target, ref object data) { }
	public virtual void LateUpdate (Character target, ref object data) { }
	public virtual void OnCharacterAttack (Character target, Bullet bullet, ref object data) { }




	#endregion




}
