using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public sealed class CharacterBuff {




	#region --- VAR ---


	// Data
	private readonly Character Character;
	private readonly int[] BuffStates;


	#endregion




	#region --- MSG ---


	public CharacterBuff (Character target) {
		Character = target;
		BuffStates = new int[Buff.AllBuffCount];

	}


	#endregion




	#region --- API ---


	public void Apply () {
		int frame = Game.PauselessFrame;
		var span = BuffStates.GetReadOnlySpan();
		for (int i = 0; i < span.Length; i++) {
			if (span[i] < frame) continue;
			Buff.GetBuffAt(i).ApplyToCharacter(Character);
		}
	}


	public bool IsBuffEnabled (int index) => BuffStates[index] >= Game.PauselessFrame;


	public void SetBuff (int index, int duration = 1) => BuffStates[index] = Util.Max(BuffStates[index], Game.PauselessFrame + duration);


	public void ClearBuff (int index) => BuffStates[index] = -1;


	public void ClearAllBuffs () => new System.Span<int>(BuffStates).Fill(-1);


	#endregion




	#region --- LGC ---



	#endregion




}
