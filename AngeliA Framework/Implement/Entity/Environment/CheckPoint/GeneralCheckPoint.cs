using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace AngeliA.Framework; 


public class GeneralCheckPoint : CheckPoint {
	[OnGameInitialize(1024)]
	public static void OnGameInitialize_CP () {
		Unlock(typeof(GeneralCheckPoint).AngeHash());
	}
	protected override bool TryGetAltarPosition (out Int3 altarUnitPos) {
		altarUnitPos = default;
		return false;
	}
}
