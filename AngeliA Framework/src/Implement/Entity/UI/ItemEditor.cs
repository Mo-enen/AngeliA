using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngeliA;

public class ItemEditor : WindowUI {




	#region --- VAR ---


	// Api
	public static ItemEditor Instance { get; private set; }

	// Data


	#endregion




	#region --- MSG ---


	public ItemEditor () => Instance = this;


	public override void UpdateWindowUI () {

		if (WorldSquad.Enable) Game.StopGame();





	}


	public override void Save (bool forceSave = false) {
		base.Save(forceSave);
		if (!IsDirty && !forceSave) return;




	}


	#endregion




	#region --- API ---



	#endregion




	#region --- LGC ---



	#endregion




}