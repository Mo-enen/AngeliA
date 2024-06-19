using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliaEngine;

public class CharacterAnimationEditorWindow : WindowUI {




	#region --- VAR ---


	// Api
	public static CharacterAnimationEditorWindow Instance { get; private set; }

	// Data
	private List<string> AllRigCharacterNames { get; init; }
	private Project CurrentProject = null;


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



	}


	#endregion




	#region --- API ---


	public override void Save (bool forceSave = false) {
		base.Save(forceSave);
		if (!forceSave && !IsDirty) return;
		IsDirty = false;




	}


	public void SetCurrentProject (Project project) {
		CurrentProject = project;



	}


	#endregion




	#region --- LGC ---



	#endregion




}
