using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace PixelJelly {
	// === Var ===
	public partial class Tiles {



		// Api
		public TileStyle Style => m_Style;

		public Color32 DirtLight { get => m_DirtLight; set => m_DirtLight = value; }
		public ColorGradient DirtNormal { get => m_DirtNormal; set => m_DirtNormal = value; }
		public Color32 DirtDark { get => m_DirtDark; set => m_DirtDark = value; }
		public Color32 StoneLight { get => m_StoneLight; set => m_StoneLight = value; }
		public ColorGradient StoneNormal { get => m_StoneNormal; set => m_StoneNormal = value; }
		public Color32 StoneDark { get => m_StoneDark; set => m_StoneDark = value; }
		public Color32 FloorLight { get => m_FloorLight; set => m_FloorLight = value; }
		public ColorGradient FloorNormal { get => m_FloorNormal; set => m_FloorNormal = value; }
		public Color32 FloorDark { get => m_FloorDark; set => m_FloorDark = value; }
		public Color32 WindowLight { get => m_WindowLight; set => m_WindowLight = value; }
		public ColorGradient WindowNormal { get => m_WindowNormal; set => m_WindowNormal = value; }
		public Color32 WindowDark { get => m_WindowDark; set => m_WindowDark = value; }
		public Color32 GlassNormal { get => m_GlassNormal; set => m_GlassNormal = value; }
		public Color32 GlassShadow { get => m_GlassShadow; set => m_GlassShadow = value; }
		public Color32 MarbleLight { get => m_MarbleLight; set => m_MarbleLight = value; }
		public ColorGradient MarbleNormal { get => m_MarbleNormal; set => m_MarbleNormal = value; }
		public ColorGradient MarbleCrack { get => m_MarbleCrack; set => m_MarbleCrack = value; }
		public Color32 MarbleDark { get => m_MarbleDark; set => m_MarbleDark = value; }
		public Color32 BrickLight { get => m_BrickLight; set => m_BrickLight = value; }
		public ColorGradient BrickNormal { get => m_BrickNormal; set => m_BrickNormal = value; }
		public Color32 BrickDark { get => m_BrickDark; set => m_BrickDark = value; }
		public Color32 WallpaperLight { get => m_WallpaperLight; set => m_WallpaperLight = value; }
		public ColorGradient WallpaperNormal { get => m_WallpaperNormal; set => m_WallpaperNormal = value; }
		public Color32 WallpaperDark { get => m_WallpaperDark; set => m_WallpaperDark = value; }
		public Color32 MetalLight { get => m_MetalLight; set => m_MetalLight = value; }
		public ColorGradient MetalNormal { get => m_MetalNormal; set => m_MetalNormal = value; }
		public Color32 MetalDark { get => m_MetalDark; set => m_MetalDark = value; }
		public Color32 MetalRust { get => m_MetalRust; set => m_MetalRust = value; }
		public Color32 IceLight { get => m_IceLight; set => m_IceLight = value; }
		public ColorGradient IceNormal { get => m_IceNormal; set => m_IceNormal = value; }
		public Color32 IceDark { get => m_IceDark; set => m_IceDark = value; }
		public Color32 IceSpecular { get => m_IceSpecular; set => m_IceSpecular = value; }
		public Color32 SandLight { get => m_SandLight; set => m_SandLight = value; }
		public ColorGradient SandNormal { get => m_SandNormal; set => m_SandNormal = value; }
		public ColorGradient SandTrace { get => m_SandTrace; set => m_SandTrace = value; }
		public Color32 SandDark { get => m_SandDark; set => m_SandDark = value; }
		public Color32 CrackedStoneLight { get => m_CrackedStoneLight; set => m_CrackedStoneLight = value; }
		public ColorGradient CrackedStoneNormal { get => m_CrackedStoneNormal; set => m_CrackedStoneNormal = value; }
		public Color32 CrackedStoneDark { get => m_CrackedStoneDark; set => m_CrackedStoneDark = value; }
		public Color32 CrackedStoneCrack { get => m_CrackedStoneCrack; set => m_CrackedStoneCrack = value; }
		public RectOffset Padding { get => m_Padding; set => m_Padding = value; }
		public LightDirection4 Light { get => m_Light; set => m_Light = value; }

		// Ser
		[SerializeField, ArrowNumber] private TileStyle m_Style = TileStyle.Stone;
		[SerializeField, ArrowNumber(true)] LightDirection4 m_Light = LightDirection4.TopRight;
		[SerializeField, RectOffset] RectOffset m_Padding = default;

		[SerializeField] Color32 m_DirtLight = new Color32(232, 184, 111, 255);
		[SerializeField, ColorGradient] private ColorGradient m_DirtNormal = new ColorGradient();
		[SerializeField] Color32 m_DirtDark = new Color32(140, 86, 70, 255);

		[SerializeField] Color32 m_CrackedStoneLight = new Color32(240, 230, 218, 255);
		[SerializeField, ColorGradient] private ColorGradient m_CrackedStoneNormal = new ColorGradient();
		[SerializeField] Color32 m_CrackedStoneDark = new Color32(138, 129, 127, 255);
		[SerializeField] Color32 m_CrackedStoneCrack = new Color32(119, 112, 112, 255);

		[SerializeField] Color32 m_StoneLight = new Color32(240, 230, 218, 255);
		[SerializeField, ColorGradient] private ColorGradient m_StoneNormal = new ColorGradient();
		[SerializeField] Color32 m_StoneDark = new Color32(138, 129, 127, 255);

		[SerializeField] Color32 m_FloorLight = new Color32(232, 184, 111, 255);
		[SerializeField, ColorGradient] private ColorGradient m_FloorNormal = new ColorGradient();
		[SerializeField] Color32 m_FloorDark = new Color32(140, 86, 70, 255);

		[SerializeField] Color32 m_WindowLight = new Color32(187, 162, 161, 255);
		[SerializeField, ColorGradient] private ColorGradient m_WindowNormal = new ColorGradient();
		[SerializeField] Color32 m_WindowDark = new Color32(110, 86, 97, 255);
		[SerializeField] Color32 m_GlassNormal = new Color32(237, 241, 245, 99);
		[SerializeField] Color32 m_GlassShadow = new Color32(197, 203, 205, 99);

		[SerializeField] Color32 m_MarbleLight = new Color32(240, 230, 218, 255);
		[SerializeField, ColorGradient] private ColorGradient m_MarbleNormal = new ColorGradient();
		[SerializeField, ColorGradient] private ColorGradient m_MarbleCrack = new ColorGradient();
		[SerializeField] Color32 m_MarbleDark = new Color32(138, 129, 127, 255);

		[SerializeField] Color32 m_BrickLight = new Color32(187, 162, 161, 255);
		[SerializeField, ColorGradient] private ColorGradient m_BrickNormal = new ColorGradient();
		[SerializeField] Color32 m_BrickDark = new Color32(110, 86, 97, 255);

		[SerializeField] Color32 m_WallpaperLight = new Color32(239, 194, 160, 255);
		[SerializeField, ColorGradient] private ColorGradient m_WallpaperNormal = new ColorGradient();
		[SerializeField] Color32 m_WallpaperDark = new Color32(177, 122, 102, 255);

		[SerializeField] Color32 m_MetalLight = new Color32(237, 241, 245, 255);
		[SerializeField, ColorGradient] private ColorGradient m_MetalNormal = new ColorGradient();
		[SerializeField] Color32 m_MetalDark = new Color32(142, 144, 144, 255);
		[SerializeField] Color32 m_MetalRust = new Color32(54, 41, 30, 190);

		[SerializeField] Color32 m_IceLight = new Color32(77, 189, 189, 255);
		[SerializeField, ColorGradient] private ColorGradient m_IceNormal = new ColorGradient();
		[SerializeField] Color32 m_IceDark = new Color32(42, 61, 74, 255);
		[SerializeField] Color32 m_IceSpecular = new Color32(59, 106, 118, 255);

		[SerializeField] Color32 m_SandLight = new Color32(247, 216, 143, 255);
		[SerializeField, ColorGradient] private ColorGradient m_SandNormal = new ColorGradient();
		[SerializeField, ColorGradient] private ColorGradient m_SandTrace = new ColorGradient();
		[SerializeField] Color32 m_SandDark = new Color32(207, 157, 95, 255);


		private void InitColorGradients () {
			m_DirtNormal = new ColorGradient(
				new Color32(191, 133, 92, 255), new Color32(177, 123, 85, 255)
			);
			m_CrackedStoneNormal = new ColorGradient(
				new Color32(184, 172, 167, 255), new Color32(164, 154, 149, 255)
			);
			m_StoneNormal = new ColorGradient(
				new Color32(184, 172, 167, 255), new Color32(164, 154, 149, 255)
			);
			m_FloorNormal = new ColorGradient(
				new Color32(191, 133, 92, 255), new Color32(172, 120, 83, 255)
			);
			m_WindowNormal = new ColorGradient(
				new Color32(154, 126, 134, 255), new Color32(136, 111, 118, 255)
			);
			m_MarbleNormal = new ColorGradient(
				new Color32(184, 172, 167, 255), new Color32(164, 154, 149, 255)
			);
			m_MarbleCrack = new ColorGradient(
				new Color32(168, 157, 153, 255), new Color32(148, 139, 135, 255)
			);
			m_BrickNormal = new ColorGradient(
				new Color32(154, 126, 134, 255), new Color32(136, 111, 118, 255)
			);
			m_WallpaperNormal = new ColorGradient(
				new Color32(208, 158, 131, 255), new Color32(184, 140, 116, 255)
			);
			m_MetalNormal = new ColorGradient(
				new Color32(197, 203, 205, 255), new Color32(179, 184, 186, 255)
			);
			m_IceNormal = new ColorGradient(
				new Color32(59, 106, 118, 255), new Color32(69, 124, 137, 255)
			);
			m_SandNormal = new ColorGradient(
				new Color32(237, 198, 123, 255), new Color32(215, 180, 112, 255)
			);
			m_SandTrace = new ColorGradient(
				new Gradient() {
					alphaKeys = new GradientAlphaKey[1] { new GradientAlphaKey(1f, 0f) },
					colorKeys = new GradientColorKey[3] {
						new GradientColorKey(new Color32(247, 216, 143, 255), 0.4f),
						new GradientColorKey(new Color32(231, 182, 110, 255), 0.6f),
						new GradientColorKey(new Color32(207, 157, 95, 255), 1f),
					},
					mode = GradientMode.Fixed,
				}, new Color32(207, 157, 95, 255), false
			);

		}


	}
}
