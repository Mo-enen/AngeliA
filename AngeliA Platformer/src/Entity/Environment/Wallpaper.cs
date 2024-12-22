using System.Collections;
using System.Collections.Generic;


using AngeliA;namespace AngeliA.Platformer;


[EntityAttribute.DontDespawnOutOfRange]
[EntityAttribute.DontDestroyOnZChanged]
[EntityAttribute.DontDrawBehind]
[EntityAttribute.UpdateOutOfRange]
[EntityAttribute.MapEditorGroup("Wallpaper")]
public abstract class Wallpaper : Entity {




	#region --- VAR ---


	// Api
	protected int Amount { get; private set; } = 0;

	// Data
	private static Wallpaper Current = null;
	private int? PrevTargetX = null;


	#endregion




	#region --- MSG ---


	public override void OnActivated () {
		base.OnActivated();
		Amount = 0;
	}


	[OnGameUpdate]
	public static void OnGameUpdate () {
		if (Current != null && !Current.Active) Current = null;
	}


	public override void BeforeUpdate () {
		base.BeforeUpdate();
		if (Current != null && Current.Active && Current.TypeID == TypeID && Current.SpawnFrame < SpawnFrame) {
			Active = false;
		}
	}


	public override void Update () {
		base.Update();
		if (!Active) return;
		// Switch Current
		if (Current != this) {
			if (Current == null) {
				Current = this;
			} else if (PlayerSystem.Selecting != null && PrevTargetX.HasValue) {
				// Cross to Trigger
				if ((PlayerSystem.Selecting.X - X).Sign() != (PrevTargetX.Value - X).Sign()) {
					Current = this;
				}
			}
		}
		PrevTargetX = PlayerSystem.Selecting?.X;
		// Update Amount
		const int DELTA = 6;
		if (Current == this) {
			Amount = (Amount + DELTA).Clamp(0, 1000);
		} else {
			Amount = (Amount - DELTA).Clamp(0, 1000);
		}
	}


	public sealed override void LateUpdate () {
		if (!Active) return;
		if (Current != this && Amount == 0) {
			if (!FromWorld || InstanceID.z != Stage.ViewZ || !Rect.Overlaps(Stage.SpawnRect)) {
				Active = false;
			}
			return;
		}
		base.LateUpdate();
		using (new LayerScope(RenderLayer.WALLPAPER)) {
			DrawBackground(Renderer.CameraRect);
		}
	}


	protected abstract void DrawBackground (IRect backgroundRect);


	#endregion




	#region --- API ---


	protected Color32 GetSkyTint (int y) {
		return Color32.LerpUnclamped(
			Sky.SkyTintBottomColor, Sky.SkyTintTopColor,
			Util.InverseLerp(Renderer.CameraRect.yMin, Renderer.CameraRect.yMax, y)
		);
	}


	#endregion




}