using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public sealed class CharacterBuff {




	#region --- SUB ---


	private class State {
		public bool IsActived => Frame >= Game.GlobalFrame;
		public int Frame;
		public object Data;
	}


	#endregion




	#region --- VAR ---


	// Api
	public int BuffCount => Game.GlobalFrame <= ActivedCountUpdatedFrame + 1 ? ActivedCountData : 0;

	// Data
	private readonly Character Character;
	private readonly State[] BuffStates;
	private int ActivedCountData = 0;
	private int ActivedCountUpdatedFrame = -1;


	#endregion




	#region --- MSG ---


	internal CharacterBuff (Character target) {
		Character = target;
		BuffStates = new State[Buff.AllBuffCount].FillWithNewValue();
	}


	internal void ApplyOnBeforeUpdate () {
		var span = BuffStates.GetReadOnlySpan();
		ActivedCountData = 0;
		ActivedCountUpdatedFrame = Game.GlobalFrame;
		for (int i = 0; i < span.Length; i++) {
			var state = span[i];
			if (!state.IsActived) continue;
			ActivedCountData++;
			Buff.GetBuffAtIndex(i).BeforeUpdate(Character, ref state.Data);
		}
	}


	internal void ApplyOnLateUpdate () {
		var span = BuffStates.GetReadOnlySpan();
		for (int i = 0; i < span.Length; i++) {
			var state = span[i];
			if (!state.IsActived) continue;
			Buff.GetBuffAtIndex(i).LateUpdate(Character, ref state.Data);
		}
	}


	internal void ApplyOnAttack (Bullet bullet) {
		var span = BuffStates.GetReadOnlySpan();
		for (int i = 0; i < span.Length; i++) {
			var state = span[i];
			if (!state.IsActived) continue;
			var buff = Buff.GetBuffAtIndex(i);
			buff.OnCharacterAttack(Character, bullet, ref state.Data);
		}
	}


	#endregion




	#region --- API ---


	public bool HasBuff (int id) {
		if (Buff.TryGetBuffIndex(id, out int index)) {
			return BuffStates[index].IsActived;
		} else {
			return false;
		}
	}


	public bool HasBuffAtIndex (int index) => BuffStates[index].IsActived;


	public void GiveBuff (int id, int duration = 1) {
		if (!Buff.TryGetBuffIndex(id, out int index)) return;
		var state = BuffStates[index];
		state.Frame = Util.Max(state.Frame, Game.GlobalFrame + duration);
	}


	public void ClearBuff (int id) {
		if (!Buff.TryGetBuffIndex(id, out int index)) return;
		var state = BuffStates[index];
		state.Frame = -1;
		state.Data = null;
	}


	public void ClearAllBuffs () {
		var span = BuffStates.GetReadOnlySpan();
		for (int i = 0; i < span.Length; i++) {
			var state = span[i];
			state.Frame = -1;
			state.Data = null;
		}
	}


	public object GetBuffData (int id) => Buff.TryGetBuffIndex(id, out int index) ? BuffStates[index].Data : null;


	public void SetBuffData (int id, object data) {
		if (!Buff.TryGetBuffIndex(id, out int index)) return;
		BuffStates[index].Data = data;
	}


	#endregion




}
