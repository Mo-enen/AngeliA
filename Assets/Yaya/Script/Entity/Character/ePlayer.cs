using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {


	[ExcludeInMapEditor]
	[EntityCapacity(1)]
	[ForceUpdate]
	[EntityBounds(-Const.CELL_SIZE / 2, 0, Const.CELL_SIZE, Const.CELL_SIZE * 2)]
	[DontDestroyOnSquadTransition]
	public class ePlayer : eCharacter { }



	[FirstSelectedPlayer]
	public class eYaya : ePlayer { }




}
