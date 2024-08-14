namespace AngeliA;
public class ChangeSavingSlotTask : TaskItem {
	public static readonly int TYPE_ID = typeof(ChangeSavingSlotTask).AngeHash();
	public override TaskResult FrameUpdate () {
		if (UserData is not int slot || slot == Universe.BuiltIn.CurrentSavingSlot) return TaskResult.End;
		Game.PassEffect_RetroDarken(1f);
		if (LocalFrame == 0) {
			return TaskResult.Continue;
		} else {
			Universe.BuiltIn.ReloadSavingSlot(slot);
			Game.RestartGame();
			return TaskResult.End;
		}
	}
}