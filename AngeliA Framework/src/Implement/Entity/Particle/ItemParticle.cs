using System.Collections;
using System.Collections.Generic;


namespace AngeliA.Framework; 
// Collect
[EntityAttribute.Capacity(16, 0)]
[EntityAttribute.ForceSpawn]
public class ItemCollectParticle : Particle {
	private static readonly int TYPE_ID = typeof(ItemCollectParticle).AngeHash();
	public override int Duration => 30;
	public override bool Loop => false;
	[OnGameInitializeLater]
	public static void OnGameInitialize () {
		ItemHolder.OnItemCollected -= OnItemCollected;
		ItemHolder.OnItemCollected += OnItemCollected;
		static void OnItemCollected (Entity collector, int itemID, int count) {
			if (collector == null || itemID == 0 || count == 0) return;
			if (Stage.SpawnEntity(
				TYPE_ID,
				collector.X,
				collector.Y + Const.CEL * 2
			) is Particle particle) {
				particle.UserData = itemID;
			}
		}
	}
	public override void OnActivated () {
		base.OnActivated();
		Width = Const.CEL * 2 / 3;
		Height = Const.CEL * 2 / 3;
		X -= Width / 2;
	}
	public override void FrameUpdate () {
		base.FrameUpdate();
		if (!Active) return;
		Y += 4;
		Tint = new Color32(
			255, 255, 255,
			(byte)Util.RemapUnclamped(0, Duration, 512, 1, LocalFrame).Clamp(0, 255)
		);
		Renderer.SetLayerToUI();
		Renderer.Draw((int)UserData, Rect, Tint, 0);
		Renderer.SetLayerToDefault();
	}
}


// Lost
[EntityAttribute.Capacity(16, 0)]
[EntityAttribute.ForceSpawn]
public class ItemLostParticle : FreeFallParticle {

	private static readonly int LOST_TYPE_ID = typeof(ItemLostParticle).AngeHash();

	public override int Duration => 120;
	[OnGameInitializeLater]
	public static void OnGameInitialize () {
		Item.OnItemLost += OnItemLost;
		static void OnItemLost (Entity holder, int item) {
			if (holder == null) return;
			if (Stage.SpawnEntity(LOST_TYPE_ID, holder.X, holder.Y) is not ItemLostParticle particle) return;
			particle.ArtworkID = item;
		}
	}
	public override void OnActivated () {
		base.OnActivated();
		Width = Const.CEL * 2 / 3;
		Height = Const.CEL * 2 / 3;
		Y += Const.CEL;
		AirDragX = 1;
		// Speed
		CurrentSpeedX = Util.RandomInt(32, 48) * (Util.RandomInt(0, 2) == 0 ? 1 : -1);
		CurrentSpeedY = 64;
		RotateSpeed = 6;
	}
}


// Damage
[EntityAttribute.Capacity(4, 0)]
[EntityAttribute.ForceSpawn]
public class ItemDamageParticle : Particle {
	private static readonly int TYPE_ID = typeof(ItemDamageParticle).AngeHash();
	public override int Duration => 60;
	public override bool Loop => false;
	private int ItemBeforeID = 0;
	private int ItemAfterID = 0;
	[OnGameInitializeLater]
	public static void OnGameInitialize () {
		Item.OnItemDamage += OnItemDamage;
		static void OnItemDamage (Entity holder, int itemBefore, int itemAfter) {
			if (holder == null || itemBefore == itemAfter) return;
			if (Stage.SpawnEntity(TYPE_ID, holder.X, holder.Y) is not ItemDamageParticle particle) return;
			particle.ItemBeforeID = itemBefore;
			particle.ItemAfterID = itemAfter;
		}
	}
	public override void OnActivated () {
		base.OnActivated();
		Width = Const.CEL;
		Height = Const.CEL;
		Y += Const.CEL * 3;
		X += Width / 2;
	}
	public override void DrawParticle () {
		base.DrawParticle();
		Renderer.SetLayerToUI();

		float ease01 = Ease.OutCirc((float)LocalFrame / Duration);
		float ease010 = Util.PingPong(ease01, 0.5f) * 2f;
		int deltaSize = (int)Util.Lerp(Const.CEL, -Const.CEL, ease010).Clamp(-Const.HALF / 4, 0);
		int rotation = ease01 < 0.5f ? (int)(ease01 * 30f * (LocalFrame % 2 == 0 ? 1f : -1f)) : 0;

		byte rgb = (byte)Util.Lerp(512, 196, ease01).Clamp(0, 255);
		var tint = new Color32(
			rgb, rgb, rgb,
			(byte)Util.Lerp(512, 0, ease01).Clamp(0, 255)
		);

		// Draw
		Renderer.Draw(
			ease01 < 0.5f ? ItemBeforeID : ItemAfterID,
			X, Y, 500, 500, rotation,
			Width + deltaSize, Height + deltaSize,
			tint, 0
		);
		Renderer.SetLayerToDefault();
	}
}


// Insufficient
[EntityAttribute.Capacity(1, 0)]
[EntityAttribute.ForceSpawn]
public class ItemInsufficientParticle : Particle {
	private static readonly int TYPE_ID = typeof(ItemInsufficientParticle).AngeHash();
	public override int Duration => 60;
	public override bool Loop => false;
	private int ItemID = 0;
	private Entity Holder = null;
	[OnGameInitializeLater]
	public static void OnGameInitialize () {
		Item.OnItemInsufficient += OnItemInsufficient;
		static void OnItemInsufficient (Entity holder, int item) {
			if (holder == null) return;
			if (Stage.SpawnEntity(TYPE_ID, holder.X, holder.Y) is not ItemInsufficientParticle particle) return;
			particle.ItemID = item;
			particle.Holder = holder;
		}
	}
	public override void OnActivated () {
		base.OnActivated();
		Width = Const.CEL * 2 / 3;
		Height = Const.CEL * 2 / 3;
	}
	public override void DrawParticle () {
		base.DrawParticle();

		if (Holder == null) return;
		X = Holder.X;
		Y = Holder.Y;

		Renderer.SetLayerToUI();

		float lerp01 = (float)LocalFrame / Duration;
		float ease01 = Ease.OutExpo(lerp01);
		int deltaX = (int)Util.Lerp(-Const.HALF / 2, Const.HALF / 2, Util.PingPong(ease01 * 4.5f, 1f));
		const int deltaY = Const.CEL * 2 + Const.HALF;
		byte alpha = (byte)Util.Lerp(1024, 0, lerp01).Clamp(0, 255);

		// Draw
		Renderer.Draw(
			ItemID,
			X + deltaX, Y + deltaY, 500, 500, 0,
			Width, Height,
			new Color32(255, 255, 255, alpha), 0
		);
		Renderer.Draw(
			Const.PIXEL,
			X + deltaX, Y + deltaY, 500, 500, 0,
			Width * 10 / 8, Height * 10 / 8,
			new Color32(255, 64, 64, alpha), -1
		);
		Renderer.SetLayerToDefault();
	}
}