using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaFramework {
	[EntityAttribute.Capacity(16, 0)]
	[EntityAttribute.ForceSpawn]
	public class ItemCollectParticle : Particle {
		public override int Duration => 30;
		public override bool Loop => false;
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
	public class ItemLostParticle : FreeFallParticle {
		public override int Duration => 120;
		public override bool Loop => false;
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


	[EntityAttribute.Capacity(16, 0)]
	[EntityAttribute.ForceSpawn]
	public class EquipmentBrokeParticle : Particle {
		public override int Duration => 60;
		public override bool Loop => false;
		public override void OnActivated () {
			base.OnActivated();
			Width = Const.CEL * 2 / 3;
			Height = Const.CEL * 2 / 3;
			Y += Const.CEL * 4;
		}
		public override void DrawParticle () {
			base.DrawParticle();
			CellRenderer.SetLayerToUI();

			float ease01 = Ease.OutCirc((float)LocalFrame / Duration);
			int deltaX = (int)Mathf.Lerp(0, Const.CEL, ease01).Clamp(0, Const.HALF);
			int deltaY = (int)Mathf.Lerp(-Const.HALF, Const.CEL * 2, ease01).Clamp(0, Const.CEL);
			int deltaRot = (int)Mathf.Lerp(0, 45, ease01);
			byte rgb = (byte)Mathf.Lerp(512, 196, ease01).Clamp(0, 255);
			var tint = new Color32(
				rgb, rgb, rgb,
				(byte)Mathf.Lerp(512, 0, ease01).Clamp(0, 255)
			);

			var cellL = CellRenderer.Draw((int)UserData, X - deltaX, Y - deltaY, 500, 500, -deltaRot, Width, Height, tint, 0);
			var cellR = CellRenderer.Draw((int)UserData, X + deltaX, Y - deltaY, 500, 500, deltaRot, Width, Height, tint, 0);
			cellL.Shift = new Vector4Int(0, Width / 2, 0, 0);
			cellR.Shift = new Vector4Int(Width / 2, 0, 0, 0);
			CellRenderer.SetLayerToDefault();
		}
	}


	[EntityAttribute.Capacity(16, 0)]
	[EntityAttribute.ForceSpawn]
	public class EquipmentDamageParticle : Particle {
		public override int Duration => 60;
		public override bool Loop => false;
		public override void OnActivated () {
			base.OnActivated();
			Width = Const.CEL;
			Height = Const.CEL;
			Y += Const.CEL * 3;
			X += Width / 2;
		}
		public override void DrawParticle () {
			base.DrawParticle();
			CellRenderer.SetLayerToUI();

			float ease01 = Ease.OutCirc((float)LocalFrame / Duration);
			float ease010 = Mathf.PingPong(ease01, 0.5f) * 2f;
			int deltaSize = (int)Mathf.Lerp(Const.CEL, -Const.CEL, ease010).Clamp(-Const.HALF / 4, 0);
			int rotation = ease01 < 0.5f ? (int)(ease01 * 30f * (LocalFrame % 2 == 0 ? 1f : -1f)) : 0;

			byte rgb = (byte)Mathf.Lerp(512, 196, ease01).Clamp(0, 255);
			var tint = new Color32(
				rgb, rgb, rgb,
				(byte)Mathf.Lerp(512, 0, ease01).Clamp(0, 255)
			);

			// Draw
			CellRenderer.Draw(
				ease01 < 0.5f ? ((Vector2Int)UserData).x : ((Vector2Int)UserData).y,
				X, Y, 500, 500, rotation,
				Width + deltaSize, Height + deltaSize,
				tint, 0
			);
			CellRenderer.SetLayerToDefault();
		}
	}
}