using System.Collections;
using System.Collections.Generic;


using AngeliA;

namespace AngeliA.Platformer;

/// <summary>
/// Entity that load from map and keep exists to rendering the content between sky and map-behind layer.
/// </summary>
[EntityAttribute.DontDespawnOutOfRange]
[EntityAttribute.DontDestroyOnZChanged]
[EntityAttribute.DontDrawBehind]
[EntityAttribute.UpdateOutOfRange]
[EntityAttribute.MapEditorGroup("Wallpaper")]
public abstract class Wallpaper : Entity {




	#region --- VAR ---


	// Api
	/// <summary>
	/// Current weight of this wallpaper
	/// </summary>
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
	internal static void OnGameUpdate () {
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


	/// <summary>
	/// Use this function to handle the rendering logic
	/// </summary>
	/// <param name="backgroundRect">Range in global space this wallpaper need to render in</param>
	protected abstract void DrawBackground (IRect backgroundRect);


	#endregion




	#region --- API ---


	/// <summary>
	/// Get sky tint at given Y position
	/// </summary>
	/// <param name="y">(in global space)</param>
	protected Color32 GetSkyTint (int y) {
		return Color32.LerpUnclamped(
			Sky.SkyTintBottomColor, Sky.SkyTintTopColor,
			Util.InverseLerp(Renderer.CameraRect.yMin, Renderer.CameraRect.yMax, y)
		);
	}


	#endregion




}