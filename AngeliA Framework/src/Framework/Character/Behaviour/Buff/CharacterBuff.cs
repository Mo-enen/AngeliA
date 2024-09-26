using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public sealed class CharacterBuff {




	#region --- SUB ---


	private class State {
		public int Frame;
		public object Data;
	}


	#endregion




	#region --- VAR ---


	// Data
	private readonly Character Character;
	private readonly State[] BuffStates;


	#endregion




	#region --- MSG ---


	internal CharacterBuff (Character target) {
		Character = target;
		BuffStates = new State[Buff.AllBuffCount].FillWithNewValue();
	}


	#endregion




	#region --- API ---


	internal void ApplyOnUpdate () {
		var span = BuffStates.GetReadOnlySpan();
		for (int i = 0; i < span.Length; i++) {
			var state = span[i];
			if (state.Frame < Game.GlobalFrame) continue;
			var buff = Buff.GetBuffAt(i);
			buff.ApplyToCharacter(Character, ref state.Data);
		}
	}


	internal void ApplyOnAttack (Bullet bullet) {
		var span = BuffStates.GetReadOnlySpan();
		for (int i = 0; i < span.Length; i++) {
			var state = span[i];
			if (state.Frame < Game.GlobalFrame) continue;
			var buff = Buff.GetBuffAt(i);
			buff.OnCharacterAttack(Character, bullet, ref state.Data);
		}
	}


	public bool HasBuff<B> (BuffIndex<B> index) where B : Buff => BuffStates[index].Frame >= Game.GlobalFrame;


	public void GiveBuff<B> (BuffIndex<B> index, int duration = 1) where B : Buff {
		var state = BuffStates[index];
		state.Frame = Util.Max(state.Frame, Game.GlobalFrame + duration);
	}


	public void ClearBuff<B> (BuffIndex<B> index) where B : Buff {
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


	#endregion




}
