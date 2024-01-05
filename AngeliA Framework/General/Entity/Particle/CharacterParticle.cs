using System.Collections;
using System.Collections.Generic;


namespace AngeliaFramework {
	public class PassOutStarParticle : Particle {

		private static readonly int TYPE_ID = typeof(PassOutStarParticle).AngeHash();
		private static readonly int STAR_CODE = "PassOutStar".AngeHash();

		public override int Duration => 66;
		public override int FramePerSprite => 1;
		public override bool Loop => true;
		public Character Character => UserData as Character;

		[OnGameInitializeLater(64)]
		public static void OnGameInitialize () {
			Character.OnPassOut += OnPassOut;
			static void OnPassOut (Character character) {
				if (Stage.SpawnEntity(TYPE_ID, character.X, character.Y) is Particle particle) {
					particle.UserData = character;
				}
			}
		}

		public override void DrawParticle () {
			var character = Character;
			if (character == null || !character.Active || character.CharacterState != CharacterState.PassOut) {
				Active = false;
				return;
			}
			DrawStar(Duration * 0 / 3, Duration * 1 / 3);
			DrawStar(Duration * 1 / 3, Duration * 0 / 3);
			DrawStar(Duration * 2 / 3, Duration * 2 / 3);
		}

		private void DrawStar (int posOffset, int sizeOffset) {

			int sizeDuration = Duration + 24;
			int posFrame = (LocalFrame + posOffset).UMod(Duration);
			int sizeFrame = (LocalFrame + sizeOffset).UMod(sizeDuration);

			var charRect = Character.Rect;
			int centerX = charRect.x + charRect.width / 2;
			int centerY = charRect.yMax;
			const int rangeX = Const.HALF;
			const int rangeY = Const.HALF / 3;

			int x = Util.Remap(
				-1f, 1f, centerX - rangeX, centerX + rangeX,
				Util.Cos(Util.Remap(0, Duration, 0f, 360f, posFrame) * Util.Deg2Rad)
			).RoundToInt();

			int y = Util.Remap(
				-1f, 1f, centerY - rangeY, centerY + rangeY,
				Util.Sin(Util.Remap(0, Duration, 0f, 360f, posFrame) * Util.Deg2Rad)
			).RoundToInt();

			int size = Util.Remap(
				0, sizeDuration / 2,
				60, 140,
				sizeFrame.PingPong(sizeDuration / 2)
			);

			var cell = CellRenderer.Draw(STAR_CODE, x, y, 500, 500, 0, size, size);
			cell.Z *= posFrame < Duration / 2 ? 1 : -1;

		}

	}


	public class SleepParticle : Particle {


		private static readonly int TYPE_ID = typeof(SleepParticle).AngeHash();
		public override int Duration => 120;
		public override int FramePerSprite => 1;
		public override bool Loop => false;
		public override int Scale => 800;
		private static int GlobalShift = 0;


		[OnGameInitializeLater(64)]
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
	public class SlideDustParticle : Particle {
		private static readonly int TYPE_ID = typeof(SlideDustParticle).AngeHash();
		public override int Duration => 20;
		public override bool Loop => false;
		public override int FramePerSprite => 4;
		[OnGameInitializeLater(64)]
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
	public class FootstepParticle : Particle {

		private static readonly int TYPE_ID = typeof(FootstepParticle).AngeHash();
		public override int Duration => 20;
		public override bool Loop => false;
		public override int FramePerSprite => 5;
		public override int RenderingZ => -1024;

		[OnGameInitializeLater(64)]
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



	public class JumpParticle : Particle {

		private static readonly int TYPE_ID = typeof(JumpParticle).AngeHash();
		public override int Duration => 10;
		public override bool Loop => false;
		public override int FramePerSprite => 2;
		public override int RenderingZ => int.MaxValue - 1024;
		public override int Scale => _Scale;
		private int _Scale = 1000;

		[OnGameInitializeLater(64)]
		public static void OnGameInitialize () {
			Character.OnJump += OnJumpFly;
			Character.OnFly += OnJumpFly;
			static void OnJumpFly (Character character) {
				if (character.InWater || character.InSand) return;
				if (character.CurrentJumpCount > character.JumpCount + 1) return;
				// Fly without Rise
				if (character.CurrentJumpCount > character.JumpCount && character.FlyRiseSpeed <= 0) return;
				// Spawn Particle
				if (Stage.SpawnEntity(TYPE_ID, character.X, character.Y - character.DeltaPositionY) is not JumpParticle particle) return;
				bool firstJump = character.CurrentJumpCount <= 1;
				particle._Scale = firstJump ? 618 : 900;
				if (firstJump && CellRenderer.TryGetSprite(character.GroundedID, out var sprite)) {
					particle.Tint = sprite.SummaryTint;
				} else {
					particle.Tint = Const.WHITE;
				}
			}
		}

	}

}