using System.Collections;
using System.Collections.Generic;
using AngeliaFramework;


namespace AngeliaFramework {
	public class SleepParticle : Particle {


		private static readonly int TYPE_ID = typeof(SleepParticle).AngeHash();
		public override int Duration => 120;
		public override int FramePerSprite => 1;
		public override bool Loop => false;
		public override int Scale => 800;
		private static int GlobalShift = 0;


		[OnGameInitialize(64)]
		public static void OnGameInitialize () {
			Character.OnSleeping += OnSleeping;
			static void OnSleeping (Character character) {
				Stage.TrySpawnEntity(TYPE_ID, character.X, character.Y + character.Height / 2, out _);
			}
		}


		public override void OnActivated () {
			base.OnActivated();
			Width = 0;
			Height = 0;
			X += GlobalShift * 12 + Const.HALF;
			Y += Const.HALF;
			GlobalShift = (GlobalShift + 1) % 3;
		}


		public override void FrameUpdate () {
			base.FrameUpdate();
			int frame = LocalFrame;
			X += (frame + X).PingPong(40) / (frame / 12 + 3);
			Y += (frame + Y + 16).PingPong(40) / (frame / 12 + 3);
			Tint = new(255, 255, 255, (byte)Util.Remap(0, Duration, 600, 0, frame).Clamp(0, 255));
		}


	}



	[EntityAttribute.Capacity(4)]
	public class SlideDust : Particle {
		private static readonly int TYPE_ID = typeof(SlideDust).AngeHash();
		public override int Duration => 20;
		public override bool Loop => false;
		public override int FramePerSprite => 4;
		[OnGameInitialize(64)]
		public static void OnGameInitialize () {
			Character.OnSlideStepped += OnSlideStepped;
			static void OnSlideStepped (Character character) {
				var rect = character.Rect;
				Stage.SpawnEntity(
					TYPE_ID, character.FacingRight ? rect.xMax : rect.xMin, rect.yMin + rect.height * 3 / 4
				);
			}
		}
	}



	[EntityAttribute.Capacity(36)]
	public class CharacterFootstep : Particle {

		private static readonly int TYPE_ID = typeof(CharacterFootstep).AngeHash();
		public override int Duration => 20;
		public override bool Loop => false;
		public override int FramePerSprite => 5;
		public override int RenderingZ => -1024;

		[OnGameInitialize(64)]
		public static void OnGameInitialize () {
			Character.OnFootStepped += OnFootStepped;
			Character.OnDashStepped += OnFootStepped;
			static void OnFootStepped (Character character) {
				if (
					Stage.TrySpawnEntity(TYPE_ID, character.X, character.Y, out var entity) &&
					entity is Particle particle
				) {
					if (CellRenderer.TryGetSprite(character.GroundedID, out var sprite)) {
						particle.Tint = sprite.SummaryTint;
					} else {
						particle.Tint = Const.WHITE;
					}
				}
			}
		}
	}



}