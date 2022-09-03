using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {


	[EntityAttribute.ExcludeInMapEditor]
	[EntityAttribute.EntityCapacity(1)]
	[EntityAttribute.ForceUpdate]
	[EntityAttribute.EntityBounds(-Const.CELL_SIZE / 2, 0, Const.CELL_SIZE, Const.CELL_SIZE * 2)]
	[EntityAttribute.DontDestroyOnSquadTransition]
	public abstract class ePlayer : eCharacter { }



	[FirstSelectedPlayer]
	public class eYaya : ePlayer { }




}
