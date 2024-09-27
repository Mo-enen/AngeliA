using System.Collections;
using System.Collections.Generic;

namespace AngeliA;


public abstract class Buff {




	#region --- VAR ---


	// Api
	public static int AllBuffCount => AllBuffs.Length;

	// Data
	private static Buff[] AllBuffs = [new FailbackBuff()];


	#endregion




	#region --- MSG ---


	[OnGameInitialize(-128)]
	internal static void OnGameInitialize () {
		var buffList = new List<Buff> { new FailbackBuff() };
		foreach (var type in typeof(Buff).AllChildClass()) {
			if (System.Activator.CreateInstance(type) is not Buff buff) continue;
			if (buff is FailbackBuff) continue;
			buffList.Add(buff);
		}
		AllBuffs = buffList.ToArray();
	}


	#endregion




	#region --- API ---


	internal static Buff GetBuffAt (int index) => AllBuffs[index];


	public virtual void ApplyToCharacter (Character target, ref object data) { }


	public virtual void OnCharacterAttack (Character target, Bullet bullet, ref object data) { }


	#endregion




}
