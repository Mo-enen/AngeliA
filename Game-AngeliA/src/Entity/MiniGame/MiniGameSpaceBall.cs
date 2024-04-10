using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliaGame;

public class MiniGameSpaceBall : MiniGame {




	#region --- VAR ---


	// Api
	protected override bool RequireMouseCursor => false;
	protected override Int2 WindowSize => new(800, 800);
	protected override string DisplayName => Language.Get(TypeID, "Space Ball");

	// Data
	private readonly IntToChars LevelIndexToChars = new();
	private BadgesSaveData Saving;

	// Saving
	private static readonly SavingInt UnlockedLevel = new("SpaceBall.UnlockedLevel", 0);


	#endregion




	#region --- MSG ---


	protected override void StartMiniGame () {
		Saving = LoadGameDataFromFile<BadgesSaveData>();
		Saving.FixBadgeCount(5);
		LoadLevel(UnlockedLevel.Value);
	}


	protected override void GameUpdate () {
		GamePlayUpdate();
		RenderingUpdate();
	}


	private void GamePlayUpdate () {



	}


	private void RenderingUpdate () {



	}


	#endregion




	#region --- LGC ---


	private void LoadLevel (int index) {
		UnlockedLevel.Value = Util.Max(index, UnlockedLevel.Value);




	}


	#endregion




}