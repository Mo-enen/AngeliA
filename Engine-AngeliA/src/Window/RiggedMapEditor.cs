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
	private static readonly LanguageCode BUILDING_HINT = ("UI.Rig.BuildingHint", "Recompiling...");
	private static readonly LanguageCode BUILD_ERROR_HINT = ("UI.Rig.BuildError", "Error in game script :(\nAll errors must be fixed before the game can run");
	private static readonly LanguageCode RIG_FAIL_HINT = ("UI.Rig.NotRunning", "Rigged Game Not Running :(\nThis should not happen. Please contact the developer and report this problem.");

	// Api
	public override string DefaultName => "Map Editor";

	// Data
	private readonly GUIStyle FailHintStyle = new(GUI.Skin.SmallCenterMessage) { LineSpace = 14 };
	private RiggedTransceiver RiggedGame;
	private int NoGameRunningFrameCount = 0;


	#endregion




	#region --- MSG ---


	public void Initialize (RiggedTransceiver riggedGame) => RiggedGame = riggedGame;


	public override void UpdateWindowUI () {

		bool building = EngineUtil.BuildingProjectInBackground;

		if (!RiggedGame.RigProcessRunning) {
			if (building) {
				GUI.BackgroundLabel(
					WindowRect, BUILDING_HINT, Color32.BLACK,
					backgroundPadding: Unify(12), style: FailHintStyle
				);
			} else if (EngineUtil.LastBackgroundBuildReturnCode != 0) {
				GUI.BackgroundLabel(
					WindowRect, BUILD_ERROR_HINT, Color32.BLACK,
					backgroundPadding: Unify(12), style: FailHintStyle
				);
			} else {
				NoGameRunningFrameCount++;
				if (NoGameRunningFrameCount > 60) {
					GUI.BackgroundLabel(
						WindowRect, RIG_FAIL_HINT, Color32.BLACK,
						backgroundPadding: Unify(12), style: FailHintStyle
					);
				}
			}
			return;
		}
		NoGameRunningFrameCount = 0;

		if (building) return;

	}


	#endregion




	#region --- LGC ---





	#endregion




}