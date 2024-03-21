using System.Collections;
using System.Collections.Generic;


[assembly: AngeliA.RequireGlobalSprite(atlas: "Entity", "Wallpaper")]


namespace AngeliA.Framework;
[EntityAttribute.DontDestroyOutOfRange]
[EntityAttribute.DontDestroyOnZChanged]
[EntityAttribute.DontDrawBehind]
[EntityAttribute.UpdateOutOfRange]
[EntityAttribute.MapEditorGroup("Wallpaper")]
[RequireSprite("{0}")]
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
			} else if (Player.Selecting != null && PrevTargetX.HasValue) {
				// Cross to Trigger
				if ((Player.Selecting.X - X).Sign() != (PrevTargetX.Value - X).Sign()) {
					Current = this;
				}
			}
		}
		PrevTargetX = Player.Selecting?.X;
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
		using (GUIScope.Layer(RenderLayer.WALLPAPER)) {
			DrawBackground(Renderer.CameraRect);
		}
	}


	protected abstract void DrawBackground (IRect backgroundRect);


	#endregion




	#region --- API ---


	protected Color32 GetSkyTint (int y) => Color32.LerpUnclamped(
		Sky.SkyTintBottomColor, Sky.SkyTintTopColor,
		Util.InverseLerp(Renderer.CameraRect.yMin, Renderer.CameraRect.yMax, y)
	);


	#endregion




	#region --- LGC ---





	#endregion




}