using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliaEngine;

public class CharacterAnimationEditorWindow : WindowUI {




	#region --- SUB ---


	private class PreviewCharacter : PoseCharacter {



	}


	#endregion




	#region --- VAR ---


	// Api
	public static CharacterAnimationEditorWindow Instance { get; private set; }
	public int SheetIndex { get; set; } = -1;

	// Data
	private readonly Dictionary<int, CharacterConfig> ConfigPool = new();
	private readonly List<string> AllRigCharacterNames;
	private PreviewCharacter Preview;
	private Project CurrentProject = null;
	private int AnimationFrame = 0;


	#endregion




	#region --- MSG ---


	public CharacterAnimationEditorWindow (List<string> allRigCharacterNames) {
		Instance = this;
		AllRigCharacterNames = allRigCharacterNames;
	}


	public override void BeforeUpdate () {
		base.BeforeUpdate();
		Cursor.RequireCursor();
	}


	public override void UpdateWindowUI () {
		if (CurrentProject == null) return;

		Update_Toolbar();
		Update_Preview();

	}


	private void Update_Toolbar () {



	}


	private void Update_Preview () {

		if (Game.PauselessFrame < 2) return;
		if (Preview == null) {
			SetPreviewCharacter(nameof(DefaultPlayer));
		}
		if (Preview == null) return;

		using (new SheetIndexScope(SheetIndex)) {
			FrameworkUtil.DrawPoseCharacterAsUI(
				new IRect(Input.MouseGlobalPosition, new Int2(Unify(256), Unify(512))),
				Preview, Game.GlobalFrame, out _, out _
			);
		}

		if (Input.KeyboardDown(KeyboardKey.Digit1)) {
			SetPreviewCharacter("DefaultPlayer");
		}
		if (Input.KeyboardDown(KeyboardKey.Digit2)) {
			SetPreviewCharacter("Yaya");
		}

	}


	#endregion




	#region --- API ---


	public override void Save (bool forceSave = false) {
		if (!forceSave && !IsDirty) return;
		CleanDirty();




	}


	public void SetCurrentProject (Project project) {
		CurrentProject = project;
		ConfigPool.Clear();

	}


	#endregion




	#region --- LGC ---


	private void SetPreviewCharacter (string characterName) {
		using var _ = new SheetIndexScope(SheetIndex);
		Preview ??= new PreviewCharacter() { Active = true, };
		Preview.OnActivated();
		int charID = characterName.AngeHash();
		if (!ConfigPool.TryGetValue(charID, out var config)) {
			config = PoseCharacter.CreateCharacterConfigFromSheet(characterName);
			ConfigPool[charID] = config;
			// Body Gadget


			// Cloth


		}
		if (config != null) {
			Preview.LoadCharacterFromConfig(config);
		}
	}


	#endregion




}
