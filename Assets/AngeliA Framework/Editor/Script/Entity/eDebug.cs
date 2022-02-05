using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework.Physics;
using AngeliaFramework.Rendering;


namespace AngeliaFramework.Entities.Editor {
	public class eDebug : eRigidbody {


		public override bool Despawnable => false;
		public override int PushLevel => 1;

		public Color32 Color = new(255, 255, 255, 255);
		public string SpriteName = "Pixel";
		public string Tag = "";
		public bool IsTrigger = false;
		public int PingPongSpeedX = 0;
		public int PingPongSpeedY = 0;
		public int PingPongFrame = 0;


		// MSG
		public override void OnCreate (int frame) {
			if (Width == 0) {
				Width = Const.CELL_SIZE;
			}
			if (Height == 0) {
				Height = Const.CELL_SIZE;
			}
			base.OnCreate(frame);
		}


		public override void FillPhysics (int frame) {
			if (Width <= Const.CELL_SIZE && Height <= Const.CELL_SIZE) {
				CellPhysics.FillEntity(
					Layer, this,
					IsTrigger, string.IsNullOrEmpty(Tag) ? 0 : Tag.ACode()
				);
			}
		}


		public override void PhysicsUpdate (int frame) {
			// Ping Pong Movement
			if (PingPongFrame > 0) {
				if (PingPongSpeedX != 0) {
					VelocityX = frame % (PingPongFrame * 2) >= PingPongFrame ? PingPongSpeedX : -PingPongSpeedX;
				}
				if (PingPongSpeedY != 0) {
					VelocityY = frame % (PingPongFrame * 2) >= PingPongFrame ? PingPongSpeedY : -PingPongSpeedY;
				}
			}
			base.PhysicsUpdate(frame);
		}


		public override void FrameUpdate (int frame) {
			// Draw Sprite
			CellRenderer.Draw(
				SpriteName.ACode(),
				X, Y, 0, 0,
				0, Width, Height, Color
			);
			base.FrameUpdate(frame);
		}


	}
}
