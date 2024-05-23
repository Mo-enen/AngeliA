using System.Collections.Generic;
using System.Collections;
using AngeliA;

namespace AngeliaEngine;

public class ProjectEditor : WindowUI {




	#region --- VAR ---


	// Const
	private static readonly SpriteCode PANEL_BACKGROUND = "UI.Panel.ProjectEditor";
	private static readonly LanguageCode LABEL_EDIT = ("Label.EditCs", "Edit");
	private static readonly LanguageCode LABEL_RECOMPILE = ("Label.Recompile", "Recompile");
	private static readonly LanguageCode LABEL_RUN = ("Label.Run", "Run");
	private static readonly LanguageCode LABEL_PUBLISH = ("Label.Publish", "Publish");
	private static readonly LanguageCode TIP_EDIT = ("Tip.EditCs", "Open .sln or .csproj file in this project folder with default application");
	private static readonly LanguageCode TIP_RECOMPILE = ("Tip.Recompile", "Manually rebuild the project in debug mode (Ctrl + R)");
	private static readonly LanguageCode TIP_RUN = ("Tip.Run", "Run the current built project in a new window in debug mode (Ctrl + Shift + R)");
	private static readonly LanguageCode TIP_PUBLISH = ("Tip.Publish", "Build the project for final product in release mode");
	private static readonly LanguageCode LABEL_PRODUCT_NAME = ("Label.ProductName", "Product Name");
	private static readonly LanguageCode LABEL_VERSION = ("Label.Version", "Version");
	private static readonly LanguageCode LABEL_DEV_NAME = ("Label.DevName", "Developer Name");
	private static readonly LanguageCode TITLE_PUBLISH_PROJECT = ("Title.PublishProject", "Publish Project");
	private static readonly LanguageCode LOG_PRODUCT_NAME_INVALID = ("Log.ProductNameInvalid", "Product name contains invalid characters for file name");
	private static readonly LanguageCode LOG_PRODUCT_NAME_EMPTY = ("Log.ProductNameEmpty", "Product name can not be empty");
	private static readonly LanguageCode LOG_DEV_NAME_INVALID = ("Log.DevNameInvalid", "Developer name contains invalid characters for file name");
	private static readonly LanguageCode LOG_DEV_NAME_EMPTY = ("Log.DevNameEmpty", "Developer name can not be empty");
	private static readonly LanguageCode LABEL_LINK = ("Setting.Link", "Folders");
	private static readonly LanguageCode LABEL_LINK_PROJECT = ("Setting.Link.Project", "Project Folder");
	private static readonly LanguageCode LABEL_LINK_SAVING = ("Setting.Link.Saving", "Saving Folder");

	// Api
	public static ProjectEditor Instance { get; private set; }
	public static Project CurrentProject { get; set; }
	public int RequiringRebuildFrame { get; private set; } = -2;
	protected override bool BlockEvent => true;
	public override string DefaultName => "Project";

	// Data
	private static readonly GUIStyle WorkflowButtonStyle = new(GUI.Skin.DarkButton) { CharSize = 16, };
	private readonly RiggedTransceiver Transceiver;
	private int MasterScrollPos = 0;
	private int MasterScrollMax = 1;


	#endregion




	#region --- MSG ---


	public ProjectEditor (RiggedTransceiver transceiver) {
		Instance = this;
		Transceiver = transceiver;
	}


	public override void BeforeUpdate () {
		base.BeforeUpdate();
		Cursor.RequireCursor();
	}


	public override void UpdateWindowUI () {

		if (CurrentProject == null) return;

		var windowRect = WindowRect;
		var panelRect = windowRect.Shrink(Unify(12), Unify(12), Unify(42), Unify(196));
		int maxPanelWidth = Unify(612);
		if (panelRect.width > maxPanelWidth) {
			panelRect.x += (panelRect.width - maxPanelWidth) / 2;
			panelRect.width = maxPanelWidth;
		}

		using var _ = Scope.GUILabelWidth(Util.Min(Unify(256), panelRect.width / 2));

		var rect = panelRect.EdgeInside(Direction4.Up, Unify(50));
		int extendedContentSize;

		using (var scroll = Scope.GUIScroll(windowRect, MasterScrollPos, 0, MasterScrollMax)) {
			MasterScrollPos = scroll.ScrollPosition;

			// Window
			Update_WorkflowButton(ref rect);
			Update_Config(ref rect);

			extendedContentSize = panelRect.yMax - rect.yMax + Unify(64);
			MasterScrollMax = (extendedContentSize - panelRect.height).GreaterOrEquelThanZero();

		}
		MasterScrollPos = GUI.ScrollBar(
			891236, windowRect.EdgeInside(Direction4.Right, Unify(12)),
			MasterScrollPos, extendedContentSize, panelRect.height
		);

		// BG
		using (Scope.RendererLayer(RenderLayer.DEFAULT)) {
			if (Renderer.TryGetSprite(PANEL_BACKGROUND, out var sprite)) {
				var range = new IRect(panelRect.x, WindowRect.y, panelRect.width, panelRect.yMax - WindowRect.yMin + MasterScrollPos);
				var border = GUI.UnifyBorder(sprite.GlobalBorder, true);
				range = range.Expand(border);
				Renderer.DrawTile(
					sprite, range, Alignment.TopMid, adapt: false,
					borderL: border.left, borderR: border.right, borderD: border.down, borderU: border.up
				);
			}
		}

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
		int padding = Unify(8);
		_rect.width = rect.width / 4 - padding;

		// Edit
		if (GUI.Button(_rect, LABEL_EDIT, WorkflowButtonStyle)) {
			bool found = false;
			foreach (var path in Util.EnumerateFiles(CurrentProject.ProjectPath, true, "*.sln")) {
				found = true;
				Game.OpenUrl(path);
				break;
			}
			if (!found) {
				foreach (var path in Util.EnumerateFiles(CurrentProject.ProjectPath, true, "*.csproj")) {
					Game.OpenUrl(path);
					break;
				}
			}
		}
		RequireTooltip(_rect, TIP_EDIT);
		_rect.SlideRight(padding);

		using (Scope.GUIEnable(!EngineUtil.BuildingProjectInBackground)) {

			// Recompile
			if (GUI.Button(_rect, LABEL_RECOMPILE, WorkflowButtonStyle)) {
				RequiringRebuildFrame = Game.GlobalFrame;
			}
			RequireTooltip(_rect, TIP_RECOMPILE);
			_rect.SlideRight(padding);

			// Run
			if (GUI.Button(_rect, LABEL_RUN, WorkflowButtonStyle)) {
				EngineUtil.RunAngeliaBuild(CurrentProject);
			}
			RequireTooltip(_rect, TIP_RUN);
			_rect.SlideRight(padding);

			// Publish
			if (GUI.Button(_rect, LABEL_PUBLISH, WorkflowButtonStyle)) {
				FileBrowserUI.SaveFolder(TITLE_PUBLISH_PROJECT, CurrentProject.Universe.Info.ProductName, PublishProject);
			}
			RequireTooltip(_rect, TIP_PUBLISH);
			_rect.SlideRight(padding);

		}

		rect.SlideDown(padding);

		// Func
		static void PublishProject (string path) {
			if (string.IsNullOrWhiteSpace(path)) return;
			if (EngineUtil.BuildingProjectInBackground) return;
			if (Instance.Transceiver.RigProcessRunning) {
				Instance.Transceiver.Abort();
			}
			int returnCode = EngineUtil.PublishAngeliaProject(CurrentProject, path);
			if (returnCode != 0) {
				Debug.LogError(returnCode);
			}
			if (Util.FolderExists(path)) {
				Game.OpenUrl(path);
			}
		}
	}


	private void Update_Config (ref IRect rect) {

		int padding = GUI.FieldPadding;
		int itemHeight = GUI.FieldHeight;
		rect.yMin = rect.yMax - itemHeight;
		var info = CurrentProject.Universe.Info;
		int labelWidth = GUI.LabelWidth;

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

		// Open Project Folders
		GUI.SmallLabel(rect, LABEL_LINK);
		var _rect = rect.ShrinkLeft(GUI.LabelWidth);
		if (GUI.SmallLinkButton(_rect, LABEL_LINK_PROJECT, out var bounds)) {
			Game.OpenUrl(CurrentProject.ProjectPath);
		}
		_rect.xMin = bounds.xMax + Unify(12);
		if (GUI.SmallLinkButton(_rect, LABEL_LINK_SAVING)) {
			Game.OpenUrl(CurrentProject.Universe.SavingRoot);
		}
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




}