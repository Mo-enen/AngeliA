using System.Collections.Generic;
using System.Collections;
using AngeliA;

namespace AngeliaEngine;

public class ProjectEditor : WindowUI {




	#region --- VAR ---


	// Const
	private static readonly LanguageCode LABEL_RUN = ("Label.Run", "Run");
	private static readonly LanguageCode LABEL_PUBLISH = ("Label.Publish", "Publish");
	private static readonly LanguageCode LABEL_PRODUCT_NAME = ("Label.ProductName", "Product Name");
	private static readonly LanguageCode LABEL_VERSION = ("Label.Version", "Version");
	private static readonly LanguageCode LABEL_DEV_NAME = ("Label.DevName", "Developer Name");
	private static readonly LanguageCode TITLE_PUBLISH_PROJECT = ("Title.PublishProject", "Publish Project");
	private static readonly LanguageCode TIP_RUN = ("Tip.Run", "Build and run the project (Ctrl + R)");
	private static readonly LanguageCode LOG_PRODUCT_NAME_INVALID = ("Log.ProductNameInvalid", "Product name contains invalid characters for file name");
	private static readonly LanguageCode LOG_PRODUCT_NAME_EMPTY = ("Log.ProductNameEmpty", "Product name can not be empty");
	private static readonly LanguageCode LOG_DEV_NAME_INVALID = ("Log.DevNameInvalid", "Developer name contains invalid characters for file name");
	private static readonly LanguageCode LOG_DEV_NAME_EMPTY = ("Log.DevNameEmpty", "Developer name can not be empty");
	private static readonly LanguageCode LABEL_CLOSE_CMD = ("Setting.CloseCmd", "Close Command Window on Game Quit");

	// Api
	protected override bool BlockEvent => true;
	public static Project CurrentProject { get; set; }

	// Data
	private static readonly GUIStyle WorkflowButtonStyle = new(GUISkin.DarkButton) { CharSize = 20, };
	private int MasterScrollPos = 0;
	private int MasterScrollMax = 1;


	#endregion




	#region --- MSG ---


	public override void BeforeUpdate () {
		base.BeforeUpdate();
		Cursor.RequireCursor();
	}


	public override void UpdateWindowUI () {
		if (CurrentProject == null) return;
		using var _ = Scope.GUILabelWidth(384);
		var panelRect = WindowRect.Shrink(Unify(128), Unify(128), Unify(42), Unify(42));
		var rect = panelRect.EdgeInside(Direction4.Up, Unify(64));
		int extendedContentSize;
		using (var scroll = Scope.GUIScroll(panelRect, MasterScrollPos, 0, MasterScrollMax)) {
			MasterScrollPos = scroll.ScrollPosition;
			int top = rect.yMax;
			Update_WorkflowButton(ref rect);
			rect.y -= Unify(12);
			Update_Config(ref rect);
			extendedContentSize = top - rect.yMax + Unify(64);
			MasterScrollMax = (extendedContentSize - panelRect.height).GreaterOrEquelThanZero();
		}
		MasterScrollPos = GUI.ScrollBar(
			891236, WindowRect.EdgeInside(Direction4.Right, Unify(12)),
			MasterScrollPos, extendedContentSize, panelRect.height
		);
		// Hotkey
		if (Input.KeyboardHolding(KeyboardKey.LeftCtrl)) {
			// Ctrl + S
			if (Input.KeyboardDown(KeyboardKey.S)) {
				Save();
			}
		}
	}


	private void Update_WorkflowButton (ref IRect rect) {

		var _rect = rect;
		_rect.width /= 2;
		int padding = Unify(24);

		// Run
		if (GUI.Button(_rect.Shrink(padding, padding / 2, 0, 0), LABEL_RUN, WorkflowButtonStyle)) {
			EngineUtil.BuildAngeliaProject(CurrentProject, runAfterBuild: true);
		}
		RequireTooltip(_rect, TIP_RUN);
		_rect.SlideRight();

		// Publish
		if (GUI.Button(_rect.Shrink(padding / 2, padding, 0, 0), LABEL_PUBLISH, WorkflowButtonStyle)) {
			FileBrowserUI.SaveFolder(TITLE_PUBLISH_PROJECT, CurrentProject.Universe.Info.ProductName, PublishProject);
		}
		rect.SlideDown();

		// Func
		static void PublishProject (string path) {
			EngineUtil.PublishAngeliaProject(CurrentProject, path);
			if (Util.FolderExists(path)) {
				Game.OpenUrl(path);
			}
		}
	}


	private void Update_Config (ref IRect rect) {

		int padding = Unify(6);
		rect.yMin = rect.yMax - Unify(32);
		var info = CurrentProject.Universe.Info;
		int labelWidth = Unify(GUI.LabelWidth);

		// Product Name
		GUI.SmallLabel(rect.EdgeInside(Direction4.Left, labelWidth), LABEL_PRODUCT_NAME);
		string newProductName = GUI.SmallInputField(834267, rect.Shrink(labelWidth, 0, 0, 0), info.ProductName);
		if (newProductName != info.ProductName) {
			if (string.IsNullOrWhiteSpace(newProductName)) {
				Debug.LogWarning(LOG_PRODUCT_NAME_EMPTY);
			} else if (!Util.IsValidForFileName(newProductName)) {
				Debug.LogWarning(LOG_PRODUCT_NAME_INVALID);
			} else {
				info.ProductName = newProductName;
				SetDirty();
			}
		}
		rect.SlideDown(padding);

		// Dev Name
		GUI.SmallLabel(rect.EdgeInside(Direction4.Left, labelWidth), LABEL_DEV_NAME);
		string newDevName = GUI.SmallInputField(834268, rect.ShrinkLeft(labelWidth), info.DeveloperName);
		if (newDevName != info.DeveloperName) {
			if (string.IsNullOrWhiteSpace(newDevName)) {
				Debug.LogWarning(LOG_DEV_NAME_EMPTY);
			} else if (!Util.IsValidForFileName(newDevName)) {
				Debug.LogWarning(LOG_DEV_NAME_INVALID);
			} else {
				info.DeveloperName = newDevName;
				SetDirty();
			}
		}
		rect.SlideDown(padding);

		// Version
		GUI.SmallLabel(rect.EdgeInside(Direction4.Left, labelWidth), LABEL_VERSION);
		var versionRect = rect.ShrinkLeft(labelWidth);
		versionRect.width = Util.Min(versionRect.width / 3, Unify(96));

		info.MajorVersion = GUI.SmallIntDial(versionRect, info.MajorVersion, out bool vChanged, min: 0);
		versionRect.SlideRight();
		if (vChanged) SetDirty();

		info.MinorVersion = GUI.SmallIntDial(versionRect, info.MinorVersion, out vChanged, min: 0);
		versionRect.SlideRight();
		if (vChanged) SetDirty();

		info.PatchVersion = GUI.SmallIntDial(versionRect, info.PatchVersion, out vChanged, min: 0);
		versionRect.SlideRight();
		if (vChanged) SetDirty();

		rect.SlideDown(padding);

		// Close Command Window on Game Quit
		info.CloseCmdOnQuit = GUI.Toggle(
			rect, info.CloseCmdOnQuit, LABEL_CLOSE_CMD,
			labelStyle: GUISkin.SmallLabel
		);
		rect.SlideDown(padding);

	}


	public override void Save (bool forceSave = false) {
		base.Save(forceSave);
		if (CurrentProject == null) return;
		if (!IsDirty && !forceSave) return;
		IsDirty = false;
		// Save Universe Info to Disk
		string infoPath = CurrentProject.Universe.InfoPath;
		var info = CurrentProject.Universe.Info;
		JsonUtil.SaveJsonToPath(info, infoPath, prettyPrint: true);
	}


	#endregion




	#region --- LGC ---





	#endregion




}