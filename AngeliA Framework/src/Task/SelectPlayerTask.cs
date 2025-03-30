namespace AngeliA;

/// <summary>
/// Task that select current player
/// </summary>
public class SelectPlayerTask : Task {
	public static readonly int TYPE_ID = typeof(SelectPlayerTask).AngeHash();
	public override TaskResult FrameUpdate () {
		if (UserData is not int) return TaskResult.End;
		if (UserData is Character target) {
			PlayerSystem.SetCharacterAsPlayer(target);
		} else if (UserData is int id) {
			PlayerSystem.SelectCharacterAsPlayer(id);
		}
		return TaskResult.End;
	}
}

