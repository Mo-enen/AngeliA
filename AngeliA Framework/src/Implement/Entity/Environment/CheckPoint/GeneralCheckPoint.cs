using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace AngeliA;


public class GeneralCheckPoint : CheckPoint {


	[OnGameInitializeLater(1)]
	public static TaskResult OnGameInitialize_CP () {
		if (!UnlockedPoolReady) return TaskResult.Continue;
		Unlock(typeof(GeneralCheckPoint).AngeHash());
		return TaskResult.End;
	}


	protected override bool TryGetAltarPosition (out Int3 altarUnitPos) {
		altarUnitPos = default;
		return false;
	}


}
