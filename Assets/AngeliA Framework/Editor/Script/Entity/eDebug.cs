using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework.Physics;
using AngeliaFramework.Rendering;


namespace AngeliaFramework.Entities.Editor {
	public class eDebug : eRigidbody {


		private static readonly int PIXEL_CODE = "Pixel".ACode();

		public override bool Despawnable => false;
		public override int PushLevel => 0;

		public Color32 Color = new(255, 255, 255, 255);
		public string SpriteName = "Pixel";
		public string Tag = "";
		public bool PhysicsCheck = false;
		public bool IsTrigger = false;
		public int PingPongSpeedX = 0;
		public int PingPongSpeedY = 0;
		public int PingPongFrame = 0;

		private Color32? PhysicsCheckTint = null;


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
				CellPhysics.Fill(
					Layer, new RectInt(X, Y, Width, Height), this,
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
			// Physics Check
			if (PhysicsCheck) {
				bool success = false;
				CellPhysics.ForAllOverlaps(Layer, new RectInt(X, Y, Width, Height), (info) => {
					if (info.Entity != this && info.Entity is eDebug dEntity) {
						if (!PhysicsCheckTint.HasValue) {
							PhysicsCheckTint = Random.ColorHSV(0f, 1f, 1f, 1f, 1f, 1f);
						}
						CellRenderer.Draw(
							PIXEL_CODE,
							dEntity.X, dEntity.Y, 0, 0,
							0, dEntity.Width, dEntity.Height, PhysicsCheckTint.Value
						);
						success = true;
					}
					return true;
				});
				if (!success) {
					PhysicsCheckTint = null;
				}
			}
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
