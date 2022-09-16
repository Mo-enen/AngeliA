using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PixelJelly;

namespace PixelJelly {
	public class Cake : JellyBehaviour {



		// VAR
		public override string DisplayName => "Cake";
		public override string DisplayGroup => "Prop";
		public override int MaxFrameCount => 1;

		public override int MaxSpriteCount => 64;
		public Vector2Int Layer { get => m_Layer; set => m_Layer = value; }
		public Vector2Int TopWidth { get => m_TopWidth; set => m_TopWidth = value; }
		public Vector2Int EmbryoRound { get => m_EmbryoRound; set => m_EmbryoRound = value; }
		public Vector2Int CreamHeight { get => m_CreamHeight; set => m_CreamHeight = value; }
		public Vector2 SugarAmount { get => m_SugarAmount; set => m_SugarAmount = value; }
		public Vector2Int ItemCount { get => m_ItemCount; set => m_ItemCount = value; }
		public ColorGradient Embryo { get => m_Embryo; set => m_Embryo = value; }
		public ColorGradient Cream { get => m_Cream; set => m_Cream = value; }
		public ColorGradient Sugar { get => m_Sugar; set => m_Sugar = value; }
		public PixelSprite[] Items { get => m_Items; set => m_Items = value; }

		// Ser
		[SerializeField, MinMaxNumber(1, 64)] Vector2Int m_Layer = new Vector2Int(1, 3);
		[SerializeField, MinMaxNumber(1, 128)] Vector2Int m_TopWidth = new Vector2Int(16, 24);
		[SerializeField, MinMaxNumber(0, 128)] Vector2Int m_EmbryoRound = new Vector2Int(1, 1);
		[SerializeField, MinMaxNumber(0, 128)] Vector2Int m_CreamHeight = new Vector2Int(2, 4);
		[SerializeField, MinMaxNumber(0f, 1f, 0.1f)] Vector2 m_SugarAmount = new Vector2(0f, 0.2f);
		[SerializeField, MinMaxNumber(0, 128)] Vector2Int m_ItemCount = new Vector2Int(3, 5);
		[SerializeField, ColorGradient] ColorGradient m_Embryo = new ColorGradient();
		[SerializeField, ColorGradient] ColorGradient m_Cream = new ColorGradient();
		[SerializeField, ColorGradient] ColorGradient m_Sugar = new ColorGradient();
		[SerializeField, PixelEditor] PixelSprite[] m_Items = new PixelSprite[0];


		// MSG
		protected override void OnCreated () {
			Width = 24;
			Height = 32;
			FrameCount = 1;
			SpriteCount = 6;
			m_Embryo = new ColorGradient(new Color32(252, 195, 81, 255), new Color32(232, 173, 82, 255));
			m_Cream = new ColorGradient(new Color32(255, 255, 255, 255), new Color32(240, 230, 218, 255));
			m_Sugar = new ColorGradient(new Gradient() {
				alphaKeys = new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f) },
				colorKeys = new GradientColorKey[] {
					new GradientColorKey(new Color32(240, 86, 86, 255), 1f/7f),
					new GradientColorKey(new Color32(255, 165, 50, 255), 2f/7f),
					new GradientColorKey(new Color32(255, 255, 0, 255), 3f/7f),
					new GradientColorKey(new Color32(58, 166, 105, 255), 4f/7f),
					new GradientColorKey(new Color32(77, 189, 189, 255), 5f/7f),
					new GradientColorKey(new Color32(47, 86, 164, 255), 6f/7f),
					new GradientColorKey(new Color32(77, 58, 100, 255), 7f/7f),
				},
				mode = GradientMode.Fixed,
			}, new Color32(236, 87, 225, 255), false);
			m_Items = new PixelSprite[] {
				new PixelSprite("3_3_1_0_0_0_0_0_1295672575_1867678719_1295672575_1867678719_1295672575_1867678719_1295672575_1867678719_1295672575_"),
				new PixelSprite("3_3_1_0_0_0_0_0_4286408447_4032190207_4286408447_4032190207_4286408447_4032190207_4286408447_4032190207_4286408447_"),
				new PixelSprite("3_3_1_0_0_0_0_0_4240658943_4289016575_4240658943_4289016575_4240658943_4289016575_4240658943_4289016575_4240658943_"),
				new PixelSprite("3_3_1_0_0_0_0_0_1304280575_996833023_1304280575_996833023_1304280575_996833023_1304280575_996833023_1304280575_"),
				new PixelSprite("2_2_0_0_0_0_0_0_1304280575_996833023_996833023_1304280575_"),
				new PixelSprite("2_2_0_0_0_0_0_0_1295672575_1867678719_1867678719_1295672575_"),
				new PixelSprite("2_2_1_0_0_0_0_0_4286408447_4032190207_4032190207_4286408447_"),
				new PixelSprite("2_2_0_0_0_0_0_0_4240658943_4289016575_4289016575_4240658943_"),
				new PixelSprite("1_8_0_0_0_0_0_0_4032190207_4286408447_4032190207_4286408447_4032190207_4286408447_4032190207_4294967295_"),
				new PixelSprite("1_8_0_0_0_0_0_0_794207487_881580287_794207487_881580287_794207487_881580287_794207487_4294967295_"),
				new PixelSprite("1_8_0_0_0_0_0_0_1369848575_2549437439_1369848575_2549437439_1369848575_2549437439_1369848575_4294967295_"),
				new PixelSprite("1_8_0_0_0_0_0_0_3786092799_4241836799_3786092799_4241836799_3786092799_4241836799_3786092799_4294967295_"),

			};
		}


		protected override void OnPopulatePixels (int width, int height, int frame, int frameCount, int sprite, int spriteCount, out Vector2 pivot, out RectOffset border) {
			pivot = new Vector2(width / 2f, 0);
			border = new RectOffset(0, 0, 0, 0);
			int itemHeight = 0;
			foreach (var item in m_Items) {
				itemHeight = Mathf.Max(itemHeight, item.Height);
			}
			int layer = Util.RemapFloor(0, spriteCount, m_Layer.y, m_Layer.x, sprite);
			int topCreamHeight = m_CreamHeight.RandomBetween(Random);
			DrawCacke(
				new RectInt(0, 0, width, height), layer, m_Layer.y,
				m_TopWidth.RandomBetween(Random),
				m_EmbryoRound.RandomBetween(Random),
				m_CreamHeight, topCreamHeight,
				m_SugarAmount.RandomBetween(Random), m_ItemCount.RandomBetween(Random), itemHeight,
				m_Embryo, m_Cream, m_Sugar, m_Items
			);
		}


		// LGC
		protected virtual void DrawCacke (
			RectInt rect, int layerCount, int maxLayerCount,
			int embryoTopWidth, int embryoRound,
			Vector2Int creamHeightRange, int topCreamHeight,
			float sugarAmount,
			int itemTopCount, int topItemHeight,
			ColorGradient embryo, ColorGradient cream, ColorGradient sugar, PixelSprite[] items
		) {
			//using var _ = new LayerScope();
			var embryoRect = rect;
			embryoRect.height = (rect.height - topItemHeight - topCreamHeight) * layerCount / maxLayerCount;
			embryoTopWidth = Mathf.Clamp(embryoTopWidth, 0, embryoRect.width);
			var topEmbryoRect = new RectInt();
			// Embryo
			Layout_SideBySide(embryoRect.height, layerCount, (_i, _y, _height) => {
				int creamHeight = creamHeightRange.RandomBetween(Random);
				int eWidth = Util.RemapFloor(0, layerCount - 1, embryoRect.width, embryoTopWidth, _i);
				var _rect = new RectInt(embryoRect.x + (embryoRect.width - eWidth) / 2, embryoRect.y + _y, eWidth, _height);
				topEmbryoRect = _rect;
				DrawEmbryo(_rect.Expand(-1, -1, 0, 0), embryoRound, embryo);
				DrawCream(_rect.Expand(0, 0, creamHeight - _rect.height, 0), 1, cream);
			});
			// Top Cream
			DrawShape(new RoundedRect(topEmbryoRect.width, topCreamHeight, 0, 0, embryoRound, embryoRound), topEmbryoRect.x, topEmbryoRect.yMax, cream, BlendMode.OneMinusAlpha);
			// Sugar
			DrawSugar(new RectInt(topEmbryoRect.x, topEmbryoRect.yMax, topEmbryoRect.width, topCreamHeight + 1), sugarAmount, sugar);
			// Item
			DrawItem(new RectInt(topEmbryoRect.x + embryoRound, topEmbryoRect.yMax + topCreamHeight, topEmbryoRect.width - 2 * embryoRound, topItemHeight), itemTopCount, items);
		}


		protected void DrawEmbryo (RectInt rect, int round, ColorGradient gradient) => DrawShape(new RoundedRect(rect.width, rect.height, round, round, 0, 0), rect.x, rect.y, gradient);


		protected void DrawCream (RectInt rect, int iteration, ColorGradient gradient) {
			var heights = new float[rect.width];
			AverageIteration(heights, iteration, 1, 0, Random);
			for (int i = 0; i < rect.width; i++) {
				int height = i == 0 || i == rect.width - 1 ? rect.height : Util.RemapRounded(0f, 1f, 1, rect.height, heights[i]);
				for (int j = 0; j < height; j++) {
					SetColor(rect.x + i, rect.yMax - 1 - j, gradient.GetColor(Random), BlendMode.OneMinusAlpha);
				}
			}
		}


		protected void DrawSugar (RectInt rect, float amount, ColorGradient gradient) {
			for (int i = rect.xMin; i < rect.xMax; i++) {
				for (int j = rect.yMin; j < rect.yMax; j++) {
					if (Random.NextDouble() < amount) {
						SetColor(i, j, gradient.GetColor(Random));
					}
				}
			}
		}


		protected void DrawItem (RectInt rect, int count, PixelSprite[] items) {
			if (rect.width <= 0 || items == null || items.Length == 0) { return; }
			int gap = rect.width / count;
			for (int i = 0; i < count; i++) {
				var item = items[Random.Next(0, items.Length)];
				if (item == null) { continue; }
				int l = rect.xMin + item.PivotX;
				int r = rect.xMax - item.Width + item.PivotX;
				int x = (Random.Next(l, r + 1) - l) / gap * gap + l;
				DrawSprite(new RectInt(x, rect.y, item.Width, item.Height), item, SpriteScaleMode.Original);
			}
		}




	}
}
