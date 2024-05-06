using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AngeliA;

namespace AngeliaEngine;

[RequireLanguageFromField]
public class RiggedMapEditor : WindowUI {





	#region --- VAR ---


	// Const
	private static readonly LanguageCode RIG_FAIL_HINT = ("UI.RigNotRunning", "Rigged Game Not Running :(\nThis should not happen. Please contact the developer and report this problem.");

	// Api
	public override string DefaultName => "Map Editor";

	// Data
	private readonly GUIStyle FailHintStyle = new(GUI.Skin.SmallCenterMessage) { LineSpace = 14 };
	private RiggedGame RiggedGame;
	private int NoGameRunningFrameCount = 0;


	#endregion




	#region --- MSG ---


	public void SetRiggedGame (RiggedGame riggedGame) {
		RiggedGame = riggedGame;
	}


	public override void UpdateWindowUI () {

		if (EngineUtil.BuildingProjectInBackground) return;

		if (!RiggedGame.RigProcessRunning) {
			NoGameRunningFrameCount++;
			if (NoGameRunningFrameCount > 60) {
				// Failed Hint
				GUI.Label(WindowRect, RIG_FAIL_HINT, FailHintStyle);
			}
			return;
		}

		NoGameRunningFrameCount = 0;






	}


	#endregion




	#region --- API ---



	#endregion




	#region --- LGC ---



	#endregion




}