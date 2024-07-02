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
	private readonly List<string> AllRigCharacterNames = new();
	private int RigGameFailToStartCount = 0;
	private int RigGameFailToStartFrame = int.MinValue;
	private int RigMapEditorWindowIndex = 0;
	private int CharAniEditorWindowIndex = 0;
	private long RequireBackgroundBuildDate = 0;
	private bool IgnoreInputForRig = false;
	private bool CurrentWindowRequireRigGame = false;
	private int NoGameRunningFrameCount = 0;


	#endregion




	#region --- MSG ---


	// Rebuild
	[OnProjectBuiltInBackground]
	internal static void RiggedGameRebuild (int code) {

		if (Instance == null) return;

		RiggedMapEditor.Instance.CleanDirty();

		switch (code) {

			case 0:
				Instance.RigGameFailToStartCount = 0;
				Instance.RigGameFailToStartFrame = int.MinValue;
				Console.Instance.RemoveAllCompileErrors();
				break;

			default:
				Debug.LogError(string.Format(LOG_BUILD_UNKNOWN, code));
				break;

			case EngineUtil.ERROR_USER_CODE_COMPILE_ERROR:
				Console.Instance.BeginCompileError();
				try {
					while (EngineUtil.BackgroundBuildMessages.Count > 0) {
						Debug.LogError(EngineUtil.BackgroundBuildMessages.Dequeue());
					}
				} catch (System.Exception ex) { Debug.LogException(ex); }
				Console.Instance.EndCompileError();
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

		var rigEdt = RiggedMapEditor.Instance;
		var pixEdt = PixelEditor.Instance;
		var calling = Transceiver.CallingMessage;
		var resp = Transceiver.RespondMessage;

		// Call
		bool called = false;
		bool runningGame = !rigEdt.FrameDebugging || rigEdt.RequireNextFrame;
		rigEdt.RequireNextFrame = false;

		if (
			CurrentProject != null &&
			!EngineUtil.BuildingProjectInBackground &&
			Transceiver.RigProcessRunning &&
			CurrentWindowRequireRigGame
		) {
			if (Input.AnyMouseButtonDown) {
				IgnoreInputForRig = IgnoreInputForRig ||
					!WindowUI.WindowRect.Contains(Input.MouseGlobalPosition) ||
					rigEdt.PanelRect.MouseInside();
			}
			if (!Input.AnyMouseButtonHolding) {
				IgnoreInputForRig = false;
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

			if (SettingWindow.Instance.RigSettingChanged) {
				SettingWindow.Instance.RigSettingChanged = false;
				calling.RequireSettingChange = true;
				calling.Setting_MEDT_Enable = EngineSetting.MapEditor_Enable.Value;
				calling.Setting_MEDT_AutoZoom = EngineSetting.MapEditor_AutoZoom.Value;
				calling.Setting_MEDT_QuickPlayerDrop = EngineSetting.MapEditor_QuickPlayerDrop.Value;
				calling.Setting_MEDT_ShowBehind = EngineSetting.MapEditor_ShowBehind.Value;
				calling.Setting_MEDT_ShowState = EngineSetting.MapEditor_ShowState.Value;
			}

			// Make the Call
			if (runningGame) {
				Transceiver.Call(
					ignoreMouseInput:
						IgnoreInputForRig || Game.PauselessFrame < LastNotInteractableFrame + 6,
					ignoreKeyInput:
						false,
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

		var toolPanelRect = rigEdt.PanelRect;
		int sheetIndex = pixEdt.SheetIndex;
		if (buildingProjectInBackground) {
			// Building in Background
			if (CurrentWindowRequireRigGame) {
				Transceiver.UpdateLastRespondedRender(sheetIndex, toolPanelRect, coverWithBlackTint: true);
			}
		} else if (Transceiver.RigProcessRunning) {
			// Rig Running
			if (called) {
				if (runningGame) {
					// Get Respond
					bool responded = Transceiver.Respond(
						sheetIndex,
						CurrentWindowIndex == RigMapEditorWindowIndex,
						toolPanelRect
					);
					rigEdt.RigGameSelectingPlayerID = resp.SelectingPlayerID;
					rigEdt.UpdateUsageData(resp.RenderUsages, resp.RenderCapacities, resp.EntityUsages, resp.EntityCapacities);
					rigEdt.HavingGamePlay = resp.GamePlaying;
					if (CurrentWindowIndex == RigMapEditorWindowIndex) {
						Sky.ForceSkyboxTint(resp.SkyTop, resp.SkyBottom, 3);
					}
					if (responded && resp.RespondCount == 1) {
						ReloadCharacterNames();
					}
				} else {
					Transceiver.UpdateLastRespondedRender(sheetIndex, toolPanelRect);
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
				SettingWindow.Instance.RigSettingChanged = true;
			} else {
				// Fail to Start
				RigGameFailToStartFrame = Game.GlobalFrame;
				RigGameFailToStartCount++;
			}
			if (CurrentWindowRequireRigGame) {
				// Still Render Last Image
				Transceiver.UpdateLastRespondedRender(sheetIndex, toolPanelRect, coverWithBlackTint: true);
			}
		}

	}


	private void ReloadCharacterNames () {
		AllRigCharacterNames.Clear();
		if (CurrentProject == null) return;
		string path = CurrentProject.Universe.CharacterRenderingConfigRoot;
		foreach (var filePath in Util.EnumerateFiles(path, true, "*.json")) {
			string name = Util.GetNameWithoutExtension(filePath);
			AllRigCharacterNames.Add(name);
		}
	}


	#endregion




}
