using System.Collections;
using System.Collections.Generic;
using AngeliA;
using AngeliaRigged;

namespace AngeliaEngine;

public partial class Engine {




	#region --- VAR ---


	// Const
	private static readonly LanguageCode LOG_ERROR_PROJECT_OBJECT_IS_NULL = ("Log.BuildError.ProjectObjectIsNull", "Build Error: Project object is Null");
	private static readonly LanguageCode LOG_ERROR_PROJECT_FOLDER_INVALID = ("Log.BuildError.ProjectFolderInvalid", "Build Error: Project folder path invalid");
	private static readonly LanguageCode LOG_ERROR_PUBLISH_DIR_INVALID = ("Log.BuildError.PublishDirInvalid", "Build Error: Publish folder path invalid");
	private static readonly LanguageCode LOG_ERROR_PROJECT_FOLDER_NOT_EXISTS = ("Log.BuildError.ProjectFolderNotExists", "Build Error: Project folder not exists");
	private static readonly LanguageCode LOG_ERROR_PRODUCT_NAME_INVALID = ("Log.BuildError.ProductNameInvalid", "Build Error: Product name invalid");
	private static readonly LanguageCode LOG_ERROR_DEV_NAME_INVALID = ("Log.BuildError.DevNameInvalid", "Build Error: Developer name invalid");
	private static readonly LanguageCode LOG_ERROR_RESULT_DLL_NOT_FOUND = ("Log.BuildError.ResultDllNotFound", "Build Error: Result dll file not found");
	private static readonly LanguageCode LOG_ERROR_RUNTIME_FILE_NOT_FOUND = ("Log.BuildError.RuntimeFileNotFound", "Build Error: Runtime file not found in the engine universe folder");
	private static readonly LanguageCode LOG_ERROR_UNIVERSE_FOLDER_NOT_FOUND = ("Log.BuildError.UniverseFolderNotFound", "Build Error: Universe folder not found");
	private static readonly LanguageCode LOG_ERROR_EXE_FOR_RUN_NOT_FOUND = ("Log.BuildError.ExeForRunNotFound", "Build Error: No Exe file to run");
	private static readonly LanguageCode LOG_ERROR_DOTNET_SDK_NOT_FOUND = ("Log.BuildError.DotnetSdkNotFound", "Build Error: Dotnet Sdk not found in the engine universe folder");
	private static readonly LanguageCode LOG_ERROR_ENTRY_PROJECT_NOT_FOUND = ("Log.BuildError.EntryProjectNotFound", "Build Error: Entry exe file for the project not found");
	private static readonly LanguageCode LOG_ERROR_ENTRY_RESULT_NOT_FOUND = ("Log.BuildError.EntryResultNotFound", "Build Error: Entry exe file result not found");
	private static readonly LanguageCode LOG_ERROR_CSPROJ_NOT_EXISTS = ("Log.BuildError.CsprojNotExists", "Csproj file not found");
	private static readonly LanguageCode LOG_BUILD_UNKNOWN = ("Log.BuildError.Unknown", "Unknown error on building project in background. Error code:{0}");
	private static readonly LanguageCode HINT_PUBLISHING = ("Hint.Publishing", "Publishing");

	// Data
	private readonly GUIStyle RigGameHintStyle = new(GUI.Skin.SmallCenterMessage) { LineSpace = 14 };
	private readonly RigTransceiver Transceiver = new(EngineUtil.RiggedExePath);
	private int RigGameFailToStartCount = 0;
	private int RigGameFailToStartFrame = int.MinValue;
	private int GameEditorWindowIndex = 0;
	private int ArtworkWindowIndex = 0;
	private long RequireBackgroundBuildDate = 0;
	private bool IgnoreInputForRig = false;
	private bool CurrentWindowRequireRigGame = false;
	private bool HasCompileError = false;
	private int NoGameRunningFrameCount = 0;
	private int ForceRigGameRunInBackgroundFrame = -1;
	private int RenderingSheetIndex;


	#endregion




	#region --- MSG ---


	[OnProjectBuiltInBackground]
	internal static void RiggedGameRebuild (int code) {

		if (Instance == null) return;

		GameEditor.Instance.CleanDirty();
		Instance.HasCompileError = code != 0;

		switch (code) {

			case 0:
				Instance.RigGameFailToStartCount = 0;
				Instance.RigGameFailToStartFrame = int.MinValue;
				ConsoleWindow.Instance.RemoveAllCompileErrors();
				break;

			default:
				Debug.LogError(string.Format(LOG_BUILD_UNKNOWN, code));
				break;

			case EngineUtil.ERROR_USER_CODE_COMPILE_ERROR:
				ConsoleWindow.Instance.BeginCompileError();
				try {
					while (EngineUtil.BackgroundBuildMessages.Count > 0) {
						Debug.LogError(EngineUtil.BackgroundBuildMessages.Dequeue());
					}
				} catch (System.Exception ex) { Debug.LogException(ex); }
				ConsoleWindow.Instance.EndCompileError();
				break;

			case EngineUtil.ERROR_PROJECT_OBJECT_IS_NULL:
				Debug.LogError(LOG_ERROR_PROJECT_OBJECT_IS_NULL);
				break;

			case EngineUtil.ERROR_PROJECT_FOLDER_INVALID:
				Debug.LogError(LOG_ERROR_PROJECT_FOLDER_INVALID);
				break;

			case EngineUtil.ERROR_PUBLISH_DIR_INVALID:
				Debug.LogError(LOG_ERROR_PUBLISH_DIR_INVALID);
				break;

			case EngineUtil.ERROR_PROJECT_FOLDER_NOT_EXISTS:
				Debug.LogError(LOG_ERROR_PROJECT_FOLDER_NOT_EXISTS);
				break;

			case EngineUtil.ERROR_PRODUCT_NAME_INVALID:
				Debug.LogError(LOG_ERROR_PRODUCT_NAME_INVALID);
				break;

			case EngineUtil.ERROR_DEV_NAME_INVALID:
				Debug.LogError(LOG_ERROR_DEV_NAME_INVALID);
				break;

			case EngineUtil.ERROR_RESULT_DLL_NOT_FOUND:
				Debug.LogError(LOG_ERROR_RESULT_DLL_NOT_FOUND);
				break;

			case EngineUtil.ERROR_RUNTIME_FILE_NOT_FOUND:
				Debug.LogError(LOG_ERROR_RUNTIME_FILE_NOT_FOUND);
				break;

			case EngineUtil.ERROR_UNIVERSE_FOLDER_NOT_FOUND:
				Debug.LogError(LOG_ERROR_UNIVERSE_FOLDER_NOT_FOUND);
				break;

			case EngineUtil.ERROR_EXE_FOR_RUN_NOT_FOUND:
				Debug.LogError(LOG_ERROR_EXE_FOR_RUN_NOT_FOUND);
				break;

			case EngineUtil.ERROR_DOTNET_SDK_NOT_FOUND:
				Debug.LogError(LOG_ERROR_DOTNET_SDK_NOT_FOUND);
				break;

			case EngineUtil.ERROR_ENTRY_PROJECT_NOT_FOUND:
				Debug.LogError(LOG_ERROR_ENTRY_PROJECT_NOT_FOUND);
				break;

			case EngineUtil.ERROR_ENTRY_RESULT_NOT_FOUND:
				Debug.LogError(LOG_ERROR_ENTRY_RESULT_NOT_FOUND);
				break;

			case EngineUtil.ERROR_CSPROJ_NOT_EXISTS:
				Debug.LogError(LOG_ERROR_CSPROJ_NOT_EXISTS);
				break;
		}

	}


	[OnGameQuitting(1)]
	internal static void OnEngineQuittingRig () {
		Instance.Transceiver.Quit();
		var viewPos = Instance.Transceiver.LastRigViewPos;
		var viewHeight = Instance.Transceiver.LastRigViewHeight;
		if (viewPos.HasValue) {
			EngineSetting.LastMapEditorViewX.Value = viewPos.Value.x;
			EngineSetting.LastMapEditorViewY.Value = viewPos.Value.y;
			EngineSetting.LastMapEditorViewZ.Value = viewPos.Value.z;
		}
		if (viewHeight.HasValue) {
			EngineSetting.LastMapEditorViewHeight.Value = viewHeight.Value;
		}
	}


	private void OnGUI_RiggedGame () {

		// Quit if Not Game Project
		if (CurrentProject != null && CurrentProject.Universe.Info.ProjectType != ProjectType.Game) {
			if (Transceiver.RigProcessRunning) {
				Transceiver.Abort();
			}
			return;
		}

		ConsoleWindow.Instance.HaveRunningRigGame = Transceiver.RigProcessRunning;
		if (HasCompileError) return;

		bool currentWindowRequireRigGame = CurrentWindowIndex == GameEditorWindowIndex || Game.GlobalFrame <= ForceRigGameRunInBackgroundFrame;
		bool requireRigGameRender = CurrentWindowIndex == GameEditorWindowIndex;
		bool requireRigInput = CurrentWindowIndex == GameEditorWindowIndex;

		var rigEdt = GameEditor.Instance;
		var calling = Transceiver.CallingMessage;
		var resp = Transceiver.RespondMessage;
		var console = ConsoleWindow.Instance;
		var lanEditor = LanguageEditor.Instance;
		var currentUniverse = CurrentProject?.Universe;
		var currentInfo = currentUniverse?.Info;

		if (console.RequireCodeAnalysis != 0 || lanEditor.RequireAddKeysForAllLanguageCode) {
			ForceRigGameRunInBackgroundFrame = Game.GlobalFrame + 2;
		}

		Transceiver.LogWithPrefix = EngineSetting.AddPrefixMarkForMessageFromGame.Value;

		// Call
		bool called = false;
		bool runningGame = !rigEdt.FrameDebugging || rigEdt.RequireNextFrame;
		rigEdt.RequireNextFrame = false;

		if (
			CurrentProject != null &&
			!EngineUtil.BuildingProjectInBackground &&
			Transceiver.RigProcessRunning &&
			currentWindowRequireRigGame
		) {
			if (Input.AnyMouseButtonDown) {
				IgnoreInputForRig = IgnoreInputForRig ||
					!WindowUI.WindowRect.Contains(Input.MouseGlobalPosition) ||
					rigEdt.PanelRect.MouseInside();
			}
			if (!Input.AnyMouseButtonHolding) {
				IgnoreInputForRig = false;
			}
			if (Input.IgnoringMouseInput) {
				IgnoreInputForRig = true;
			}

			if (rigEdt.DrawCollider) {
				calling.RequireDrawColliderGizmos();
			}
			if (rigEdt.EntityClickerOn) {
				calling.RequireEntityClicker();
			}
			if (rigEdt.RequireReloadPlayerMovement) {
				rigEdt.RequireReloadPlayerMovement = false;
				calling.RequireReloadPlayerMovement();
			}

			// Tool Command
			if (console.RequireCodeAnalysis != 0) {
				ForceRigGameRunInBackgroundFrame = Game.GlobalFrame + 2;
				calling.RequireToolsetCommand = console.RequireCodeAnalysis > 0 ?
					RigCallingMessage.ToolCommand.RunCodeAnalysis :
					RigCallingMessage.ToolCommand.RunCodeAnalysisSilently;
				console.RequireCodeAnalysis = 0;
			} else if (lanEditor.RequireAddKeysForAllLanguageCode) {
				ForceRigGameRunInBackgroundFrame = Game.GlobalFrame + 2;
				calling.RequireToolsetCommand = RigCallingMessage.ToolCommand.AddKeysForAllLanguageCode;
				lanEditor.RequireAddKeysForAllLanguageCode = false;
			}

			// Map Editor Setting Changed
			if (SettingWindow.Instance.MapSettingChanged) {
				SettingWindow.Instance.MapSettingChanged = false;
				calling.RequireChangeSetting(MapEditor.SETTING_QUICK_PLAYER_DROP, EngineSetting.MapEditor_QuickPlayerDrop.Value);
				calling.RequireChangeSetting(MapEditor.SETTING_SHOW_BEHIND, EngineSetting.MapEditor_ShowBehind.Value);
				calling.RequireChangeSetting(MapEditor.SETTING_SHOW_GRID_GIZMOS, EngineSetting.MapEditor_ShowGizmos.Value);
				calling.RequireChangeSetting(MapEditor.SETTING_SHOW_STATE, EngineSetting.MapEditor_ShowState.Value);
			}

			// Lighting Map Setting Changed
			var gameEDT = GameEditor.Instance;
			if (gameEDT.LightMapSettingChanged) {
				gameEDT.LightMapSettingChanged = false;
				if (gameEDT.ForcingInGameDaytime >= 0f) {
					calling.RequireChangeSetting(
						LightingSystem.SETTING_IN_GAME_DAYTIME, (int)(gameEDT.ForcingInGameDaytime * 1000)
					);
				}
				calling.RequireChangeSetting(
					LightingSystem.SETTING_PIXEL_STYLE, currentInfo.LightMap_PixelStyle
				);
				calling.RequireChangeSetting(
					LightingSystem.SETTING_SELF_LERP, (int)(currentInfo.LightMap_SelfLerp * 1000)
				);
				calling.RequireChangeSetting(
					LightingSystem.SETTING_AIR_ILLUMINANCE_DAY, (int)(currentInfo.LightMap_AirIlluminanceDay * 1000)
				);
				calling.RequireChangeSetting(
					LightingSystem.SETTING_AIR_ILLUMINANCE_NIGHT, (int)(currentInfo.LightMap_AirIlluminanceNight * 1000)
				);
				calling.RequireChangeSetting(
					LightingSystem.SETTING_BACKGROUND_TINT, (int)(currentInfo.LightMap_BackgroundTint * 1000)
				);
				calling.RequireChangeSetting(
					LightingSystem.SETTING_SOLID_ILLUMINANCE, (int)(currentInfo.LightMap_SolidIlluminance * 1000)
				);
				calling.RequireChangeSetting(
					LightingSystem.SETTING_LEVEL_ILLUMINATE_REMAIN, (int)(currentInfo.LightMap_LevelIlluminateRemain * 1000)
				);
				// Save Uni-Info to File
				JsonUtil.SaveJsonToPath(currentInfo, currentUniverse.InfoPath, prettyPrint: true);
			}

			// Make the Call
			if (runningGame) {
				Transceiver.Call(
					ignoreMouseInput:
						!requireRigInput || IgnoreInputForRig || Game.PauselessFrame < LastNotInteractableFrame + 6,
					ignoreKeyInput:
						!requireRigInput,
					leftPadding:
						GetEngineLeftBarWidth(out _),
					requiringWindowIndex:
						0
				);
			}
			called = true;
		}

		// Respond
		bool buildingProjectInBackground = EngineUtil.BuildingProjectInBackground;
		NoGameRunningFrameCount = Transceiver.RigProcessRunning || buildingProjectInBackground ? 0 : NoGameRunningFrameCount + 1;

		if (CurrentProject == null) {
			if (Transceiver.RigProcessRunning) {
				Transceiver.Abort();
			}
			return;
		}

		// Abort when Building
		if (Transceiver.RigProcessRunning && buildingProjectInBackground) {
			Transceiver.Abort();
		}

		int sheetIndex = RenderingSheetIndex;
		if (buildingProjectInBackground) {
			// Building in Background
			if (currentWindowRequireRigGame && requireRigGameRender) {
				Transceiver.UpdateLastRespondedRender(currentUniverse, sheetIndex, coverWithBlackTint: true);
			}
		} else if (Transceiver.RigProcessRunning) {
			// Rig Running
			if (called) {
				if (runningGame) {
					// Get Respond
					Transceiver.Respond(
						currentUniverse, sheetIndex,
						CurrentWindowIndex == GameEditorWindowIndex,
						!requireRigGameRender
					);
					rigEdt.RigGameSelectingPlayerID = resp.SelectingPlayerID;
					rigEdt.UpdateUsageData(resp.RenderUsages, resp.RenderCapacities, resp.EntityUsages, resp.EntityCapacities);
					rigEdt.HavingGamePlay = resp.GamePlaying;
					if (CurrentWindowIndex == GameEditorWindowIndex) {
						Sky.ForceSkyboxTint(resp.SkyTop, resp.SkyBottom, 3);
					}
				} else if (requireRigGameRender) {
					Transceiver.UpdateLastRespondedRender(currentUniverse, sheetIndex);
				}
			}
		} else if (
			(RigGameFailToStartCount < 16 && Game.GlobalFrame > RigGameFailToStartFrame + 30) ||
			Game.GlobalFrame > RigGameFailToStartFrame + 6000
		) {
			// No Rig Game Running
			int code = Transceiver.Start(
				CurrentProject.BuildPath,
				CurrentProject.UniversePath
			);
			if (code == 0) {
				// Start
				RigGameFailToStartCount = 0;
				RigGameFailToStartFrame = int.MinValue;
				SettingWindow.Instance.MapSettingChanged = true;
				GameEditor.Instance.LightMapSettingChanged = true;
			} else {
				// Fail to Start
				RigGameFailToStartFrame = Game.GlobalFrame;
				RigGameFailToStartCount++;
			}
			if (currentWindowRequireRigGame && requireRigGameRender) {
				// Still Render Last Image
				Transceiver.UpdateLastRespondedRender(currentUniverse, sheetIndex, coverWithBlackTint: true);
			}
		}

	}


	#endregion




}
