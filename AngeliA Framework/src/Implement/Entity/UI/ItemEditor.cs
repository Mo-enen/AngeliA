using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngeliA;

public class ItemEditor : WindowUI {




	#region --- VAR ---


	public static ItemEditor Instance { get; private set; }


	#endregion




	#region --- MSG ---


	public ItemEditor () => Instance = this;


	public override void UpdateWindowUI () {

		if (WorldSquad.Enable) Game.StopGame();





	}


	#endregion




	#region --- API ---



	#endregion




	#region --- LGC ---



	#endregion




}