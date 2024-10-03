namespace AngeliA;

public readonly struct GUIInteractableScope : System.IDisposable {
	private readonly bool OldInteractable;
	public GUIInteractableScope (bool interactable) {
		OldInteractable = GUI.Interactable;
		GUI.Interactable = interactable;
	}
	public readonly void Dispose () => GUI.Interactable = OldInteractable;
}
