using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public abstract class SwappingPlatform : Platform {

	// Api
	public abstract int TimeOffset { get; }
	public abstract int TimeCycle { get; }

	// Data
	private bool IsOn;
	private bool PrevIsOn;
	private int ArtworkIdOn = -1;
	private int ArtworkIdOff = -1;
	private int Scale = 1000;

	// MSG
	public override void OnActivated () {
		base.OnActivated();
		PrevIsOn = IsOn;
		Scale = 1000;
		if (ArtworkIdOn == -1) {
			int id = $"{GetType().AngeName()}.On".AngeHash();
			ArtworkIdOn = Renderer.HasSprite(id) ? id : TypeID;
		}
		if (ArtworkIdOff == -1) {
			int id = $"{GetType().AngeName()}.Off".AngeHash();
			ArtworkIdOff = Renderer.HasSprite(id) ? id : TypeID;
		}
	}
	public override void FirstUpdate () {
		IsOn = (Game.SettleFrame + TimeOffset).UMod(TimeCycle) < TimeCycle / 2;
		if (IsOn != PrevIsOn) {
			PrevIsOn = IsOn;
			OnStateChanged();
		}
		if (!IsOn) return;
		base.FirstUpdate();
	}
	public override void LateUpdate () {
		int start = Renderer.GetUsedCellCount();
		int spriteID = IsOn ? ArtworkIdOn : ArtworkIdOff;
		bool scaled = Scale != 1000;
		RenderPlatformBlock(spriteID);
		if (scaled) {
			Scale = Scale.LerpTo(1000, 0.08f);
		}
		if ((!IsOn || scaled) && Renderer.GetCells(out var cells, out int count)) {
			bool darken = !IsOn && ArtworkIdOff == TypeID && ArtworkIdOn == TypeID;
			for (int i = start; i < count; i++) {
				var cell = cells[i];
				if (darken) {
					cell.Color.r = 128;
					cell.Color.g = 128;
					cell.Color.b = 128;
				}
				if (!IsOn) {
					cell.TextSprite = CharSprite.NONE;
				}
				if (scaled) {
					cell.ScaleFrom(Scale, cell.X + cell.Width / 2, cell.Y + cell.Height / 2);
				}
			}
		}
	}
	protected virtual void OnStateChanged () {
		Scale = IsOn ? 1200 : 1000;
	}

}
