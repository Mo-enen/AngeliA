using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace PixelJelly {
	[HideRandomButton]
	public class JellyFish : JellyBehaviour {



		// SUB
		[System.Serializable]
		public class TentacleData {

			[Delayed] public string Name = "";
			[ArrowNumber(int.MinValue, int.MaxValue, 1)] public int X = 0;
			[ArrowNumber(0f, 1f, 0.1f)] public float Length = 1f;
			[MinMaxNumber(0, ignoreCompare: true)] public Vector2Int Size = new(2, 1);
			public AnimationCurve Shape = AnimationCurve.Linear(0f, 1f, 1f, 1f);
			public Color32 Color = new(234, 174, 238, 255);

		}


		[System.Serializable]
		public class AnimationData {
			[Delayed] public string Name = "";
			[Clamp(0)] public int Frame = 1;
			[Clamp(0)] public int Duration = 40;
			[Header("Head")]
			[Label("Width")] public AnimationCurve HeadWidth = AnimationCurve.Linear(0f, 0f, 1f, 0f);
			[Label("Height")] public AnimationCurve HeadHeight = AnimationCurve.Linear(0f, 0f, 1f, 0f);
			[Label("X")] public AnimationCurve HeadX = AnimationCurve.Linear(0f, 0f, 1f, 0f);
			[Label("Y")] public AnimationCurve HeadY = AnimationCurve.Linear(0f, 0f, 1f, 0f);
			[Header("Tentacle")]
			[Label("X")] public AnimationCurve TentacleX = AnimationCurve.Linear(0f, 1f, 1f, 1f);
			public AnimationCurve Length = AnimationCurve.Linear(0f, 1f, 1f, 1f);
			public Vector2 Size = new(0.5f, 2f);
			public bool Mirror = false;

		}



		// Api
		public override string DisplayName => "Jelly Fish";
		public override string DisplayGroup => "Character";
		public override int MaxFrameCount => 128;
		public override int MaxSpriteCount => 2;
		public AnimationData[] Animations => m_Animations;

		// Short
		private AnimationData CurrentAnimation => m_Animations != null && m_Animations.Length > 0 ? m_Animations[Mathf.Clamp(m_SelectingAnimation, 0, m_Animations.Length - 1)] : null;

		// Ser
		[SerializeField, HideInInspector] int m_SelectingAnimation = 0;
		[SerializeField, MinMaxNumber(0, ignoreCompare: true)] Vector2Int m_HeadSize = new(8, 6);
		[SerializeField, MinMaxNumber(int.MinValue, ignoreCompare: true)] Vector2Int m_HeadOffset = new(0, 0);
		[SerializeField, MinMaxNumber(0, ignoreCompare: true)] Vector2Int m_FaceSize = new(8, 6);
		[SerializeField, MinMaxNumber(int.MinValue, ignoreCompare: true)] Vector2Int m_FaceOffset = new(0, 0);
		[SerializeField, PixelEditor] PixelSprite m_Head = new(1, 1);
		[SerializeField, PixelEditor] PixelSprite m_HeadBack = new(1, 1);
		[SerializeField, PixelEditor] PixelSprite m_Face = new(1, 1);
		[SerializeField] TentacleData[] m_Tentacles = new TentacleData[1];
		[SerializeField] AnimationData[] m_Animations = new AnimationData[1];


		// MSG
		protected override void OnCreated () {
			Width = 16;
			Height = 16;
			FrameCount = 1;
			SpriteCount = 2;
			FrameDuration = 33;
			m_Head = new PixelSprite(1, 1);
			m_HeadBack = new PixelSprite(1, 1);
			m_Face = new PixelSprite(1, 1);
		}


		protected override void BeforePopulateAllPixels () {
			var ani = CurrentAnimation;
			FrameCount = ani != null ? Mathf.Clamp(ani.Frame, 1, MaxFrameCount) : 1;
			FrameDuration = ani != null ? ani.Duration : 1;
			
		}


		protected override void OnPopulatePixels (int width, int height, int frame, int frameCount, int sprite, int spriteCount, out Vector2 pivot, out RectOffset border) {

			pivot = new Vector2(width / 2f, 0);
			border = new RectOffset(0, 0, 0, 0);
			float frame01 = (float)frame / FrameCount;

			if (frame == 0 && sprite == 0) {
				if (m_Head == null) {
					AddInspectorMessage("Head can not be null", MessageType.Warning, "m_Head");
				}
				if (m_HeadSize.x <= 0 || m_HeadSize.y <= 0) {
					AddInspectorMessage("HeadSize must be greater than 0", MessageType.Warning, "m_HeadSize");
				}
			}
			if (m_Head == null) { return; }
			var headSprite = sprite == 0 ? m_Head : m_HeadBack;
			var headRect = new RectInt(
				(width - m_HeadSize.x) / 2 + m_HeadOffset.x,
				height - m_HeadSize.y + m_HeadOffset.y,
				m_HeadSize.x,
				m_HeadSize.y
			);

			// Tentacle
			var tZone = new RectInt(
				headRect.xMin, 0,
				headRect.width, headRect.yMin + headSprite.PivotY
			);

			for (int i = 0; i < m_Tentacles.Length; i++) {
				DrawTentacle(
					tZone,
					m_Tentacles[sprite == 0 ? i : m_Tentacles.Length - i - 1], sprite == 1,
					frame01, CurrentAnimation
				);
			}

			// Head
			DrawHead(
				headRect,
				m_FaceSize, m_FaceOffset, frame01,
				headSprite, sprite == 0 ? m_Face : null, CurrentAnimation
			);


		}


		protected override string GetSpriteName (int frame, int frameCount, int sprite, int spriteCount) => $"{FinalDisplayName}_{(sprite == 0 ? "front" : "back")} (f{frame})";


		protected virtual void DrawHead (RectInt headRect, Vector2Int faceSize, Vector2Int faceOffset, float frame01, PixelSprite head, PixelSprite face, AnimationData animation = null) {

			// Head
			if (animation != null && animation.HeadWidth != null && animation.HeadHeight != null && animation.HeadX != null && animation.HeadY != null) {
				headRect.x += animation.HeadX.Evaluate(frame01).RoundToInt();
				headRect.y += animation.HeadY.Evaluate(frame01).RoundToInt();
				headRect.width += animation.HeadWidth.Evaluate(frame01).RoundToInt();
				headRect.height += animation.HeadHeight.Evaluate(frame01).RoundToInt();
			}
			DrawSprite(headRect, head, SpriteScaleMode.Slice);

			// Face
			if (face != null) {
				DrawSprite(new RectInt(
					(headRect.center.x - faceSize.x / 2f).CeilToInt() + faceOffset.x,
					headRect.yMax - faceSize.y + faceOffset.y,
					faceSize.x, faceSize.y
				), face, SpriteScaleMode.Slice);
			}

		}


		protected virtual void DrawTentacle (RectInt zone, TentacleData tentacle, bool flipX, float waveTime, AnimationData animation = null) {
			if (tentacle == null || tentacle.Shape == null || tentacle.Shape.length == 0) { return; }
			int pivotX = zone.xMin + zone.width / 2 + (flipX ? -tentacle.X : tentacle.X);
			int prevX = 0;
			float length01 = tentacle.Length * (animation != null ? animation.Length.Evaluate(waveTime) : 1f);
			int yMin = Mathf.RoundToInt(Mathf.Lerp(zone.yMax - 1f, zone.yMin, length01));
			int yMax = zone.yMax;
			int prevY = zone.yMax - 1;
			for (int y = yMax - 1; y >= yMin; y--) {
				float y01 = Mathf.InverseLerp(yMax - 1, yMin, y);
				float x01 = tentacle.Shape.Evaluate(y01);
				int x = Mathf.RoundToInt(Mathf.LerpUnclamped(0, zone.width - 1, x01)) * (flipX ? -1 : 1);
				int size = Mathf.RoundToInt(Mathf.Lerp(tentacle.Size.x, tentacle.Size.y, y01));
				if (animation != null && animation.TentacleX != null) {
					float waveValue = animation.TentacleX.Evaluate(waveTime + y01);
					waveValue = Mathf.Lerp(animation.Size.x, animation.Size.y, y01) * waveValue;
					if (animation.Mirror && flipX == pivotX > zone.xMin + zone.width / 2) {
						waveValue *= -1f;
					}
					x += Mathf.RoundToInt(waveValue) * (flipX ? -1 : 1);
				}
				DrawLine(
					new Vector2Int(pivotX + prevX, prevY),
					new Vector2Int(pivotX + x, y),
					tentacle.Color,
					new Rectangle(size, 1, new Vector2Int(size / 2, 0))
				);
				prevX = x;
				prevY = y;
			}
		}


	}
}
