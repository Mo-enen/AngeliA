using System.Collections;
using System.Collections.Generic;

namespace AngeliA;


public abstract class Buff {




	#region --- VAR ---


	// Api
	public static int AllBuffCount => AllBuffs.Length;

	// Data
	private static Buff[] AllBuffs = { new FailbackBuff() };


	#endregion




	#region --- MSG ---


	[OnGameInitialize]
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


	public static Buff GetBuffAt (int index) => AllBuffs[index];


	public abstract void ApplyToCharacter (Character target);


	public static void SetBuffOverlap (IRect range, int buffIndex, int duration = 1) {
		var hits = Physics.OverlapAll(PhysicsMask.CHARACTER, range, out int count);
		for (int i = 0; i < count; i++) {
			var hit = hits[i];
			if (hit.Entity is not Character character) continue;
			character.Buff.SetBuff(buffIndex, duration);
		}
	}


	public static void SetBuffInRadius (int x, int y, int radius, int buffIndex, int duration = 1) {
		var range = new IRect(x - radius, y - radius, radius * 2, radius * 2);
		var hits = Physics.OverlapAll(PhysicsMask.CHARACTER, range, out int count);
		for (int i = 0; i < count; i++) {
			var hit = hits[i];
			if (hit.Entity is not Character character) continue;
			var hitRect = hit.Rect;
			if (!Util.OverlapRectCircle(radius, x, y, hitRect.x, hitRect.y, hitRect.xMax, hitRect.yMax)) continue;
			character.Buff.SetBuff(buffIndex, duration);
		}
	}


	#endregion




	#region --- LGC ---



	#endregion




}
