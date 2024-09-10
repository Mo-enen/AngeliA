using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public abstract class SwappingPlatform : Platform {
	public abstract int TimeOffset { get; }
	public abstract int TimeCycle { get; }
	private bool IsOn;
	private int ArtworkIdOn = -1;
	private int ArtworkIdOff = -1;
	public override void OnActivated () {
		base.OnActivated();
		if (ArtworkIdOn == -1) {
			int id = $"{GetType().AngeName()}.On".AngeHash();
			ArtworkIdOn = Renderer.HasSprite(id) ? id : 0;
		}
		if (ArtworkIdOff == -1) {
			int id = $"{GetType().AngeName()}.Off".AngeHash();
			ArtworkIdOff = Renderer.HasSprite(id) ? id : 0;
		}
	}
	public override void FirstUpdate () {
		IsOn = (Game.SettleFrame + TimeOffset).UMod(TimeCycle) < TimeCycle / 2;
		if (!IsOn) return;
		base.FirstUpdate();
	}
	public override void LateUpdate () {
		int artworkID = IsOn ? ArtworkIdOn : ArtworkIdOff;
		if (artworkID != 0) {
			// Render Sprites
			RenderPlatformBlock(artworkID);
		} else {
			// Failback Rendering
			int start = Renderer.GetUsedCellCount();
			base.LateUpdate();
			if (IsOn) return;
			if (Renderer.GetCells(out var cells, out int count)) {
				for (int i = start; i < count; i++) {
					cells[i].Color.r = 128;
					cells[i].Color.g = 128;
					cells[i].Color.b = 128;
				}
			}
		}
	}
}
