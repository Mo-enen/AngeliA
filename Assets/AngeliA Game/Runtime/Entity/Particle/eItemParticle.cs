using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaGame {
	[EntityAttribute.Capacity(16, 0)]
	[EntityAttribute.ForceSpawn]
	public class ItemCollectParticle : Particle {
		public override int Duration => 30;
		public override bool Loop => false;
		[AfterGameInitialize]
		public static void AfterGameInitialize () {
			ItemHolder.CollectParticleID = typeof(ItemCollectParticle).AngeHash();
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
			CellRenderer.SetLayerToUI();
			CellRenderer.Draw((int)UserData, Rect, Tint, 0);
			CellRenderer.SetLayerToDefault();
		}
	}


	[EntityAttribute.Capacity(16, 0)]
	[EntityAttribute.ForceSpawn]
	public class EquipmentLostParticle : FreeFallParticle {
		public override int Duration => 120;
		public override bool Loop => false;
		[AfterGameInitialize]
		public static void AfterGameInitialize () {
			Equipment.EquipmentLostParticleID = typeof(EquipmentLostParticle).AngeHash();
		}
		public override void OnActivated () {
			base.OnActivated();
			Width = Const.CEL * 2 / 3;
			Height = Const.CEL * 2 / 3;
			Y += Const.CEL;
			AirDragX = 1;
			// Speed
			CurrentSpeedX = AngeUtil.RandomInt(32, 48) * (AngeUtil.RandomInt(0, 2) == 0 ? 1 : -1);
			CurrentSpeedY = 64;
			RotateSpeed = 6;
		}
		public override void FrameUpdate () {
			base.FrameUpdate();
			CellRenderer.SetLayerToUI();
			CellRenderer.Draw((int)UserData, X, Y, 500, 500, Rotation, Width, Height, 0);
			CellRenderer.SetLayerToDefault();
		}
	}
}