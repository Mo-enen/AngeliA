using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace PixelJelly {
	public partial class TilesPro {


		// Api
		public Vector2 StoneEdge { get => m_StoneEdge; set => m_StoneEdge = value; }
		public Vector2Int StoneGroupX { get => m_StoneGroupX; set => m_StoneGroupX = value; }
		public Vector2Int StoneGroupY { get => m_StoneGroupY; set => m_StoneGroupY = value; }
		public Vector2 CrackedStoneEdge { get => m_CrackedStoneEdge; set => m_CrackedStoneEdge = value; }
		public Vector2Int CrackedStonePointCount { get => m_CrackedStonePointCount; set => m_CrackedStonePointCount = value; }
		public Vector2 MarbleCrackAmount { get => m_MarbleCrackAmount; set => m_MarbleCrackAmount = value; }
		public Vector2 DirtEdge { get => m_DirtEdge; set => m_DirtEdge = value; }
		public Vector2 DirtGravel { get => m_DirtGravel; set => m_DirtGravel = value; }
		public Vector2Int DirtGravelGapX { get => m_DirtGravelGapX; set => m_DirtGravelGapX = value; }
		public Vector2Int DirtGravelGapY { get => m_DirtGravelGapY; set => m_DirtGravelGapY = value; }
		public Vector2 IceHighlight { get => m_IceHighlight; set => m_IceHighlight = value; }
		public Vector2Int IceHighlightSize { get => m_IceHighlightSize; set => m_IceHighlightSize = value; }
		public Vector2Int IceSpecularSize { get => m_IceSpecularSize; set => m_IceSpecularSize = value; }
		public Vector2Int IceDarkSpecularSize { get => m_IceDarkSpecularSize; set => m_IceDarkSpecularSize = value; }
		public Vector2Int BrickRow { get => m_BrickRow; set => m_BrickRow = value; }
		public Vector2Int BrickWidth { get => m_BrickWidth; set => m_BrickWidth = value; }
		public bool MetalEdge { get => m_MetalEdge; set => m_MetalEdge = value; }
		public Vector2 MetalHighlightTint { get => m_MetalHighlightTint; set => m_MetalHighlightTint = value; }
		public Vector2Int MetalHighlight { get => m_MetalHighlight; set => m_MetalHighlight = value; }
		public Vector2Int MetalNailCountX { get => m_MetalNailCountX; set => m_MetalNailCountX = value; }
		public Vector2Int MetalNailCountY { get => m_MetalNailCountY; set => m_MetalNailCountY = value; }
		public Vector2Int MetalSunkenSizeX { get => m_MetalSunkenSizeX; set => m_MetalSunkenSizeX = value; }
		public Vector2Int MetalSunkenSizeY { get => m_MetalSunkenSizeY; set => m_MetalSunkenSizeY = value; }
		public Vector2 MetalRustAmount { get => m_MetalRustAmount; set => m_MetalRustAmount = value; }
		public bool IronEdge { get => m_IronEdge; set => m_IronEdge = value; }
		public Vector2 IronHighlightTint { get => m_IronHighlightTint; set => m_IronHighlightTint = value; }
		public Vector2Int IronHighlight { get => m_IronHighlight; set => m_IronHighlight = value; }
		public Vector2Int IronPatternSize { get => m_IronPatternSize; set => m_IronPatternSize = value; }
		public Vector2 IronRustAmount { get => m_IronRustAmount; set => m_IronRustAmount = value; }
		public bool FloorNail { get => m_FloorNail; set => m_FloorNail = value; }
		public Vector2Int FloorRow { get => m_FloorRow; set => m_FloorRow = value; }
		public Vector2Int WindowGlassColumn { get => m_WindowGlassColumn; set => m_WindowGlassColumn = value; }
		public Vector2Int WindowGlassRow { get => m_WindowGlassRow; set => m_WindowGlassRow = value; }
		public bool WindowGlassDithering { get => m_WindowGlassDithering; set => m_WindowGlassDithering = value; }
		public Vector2Int WallpaperStripeCount { get => m_WallpaperStripeCount; set => m_WallpaperStripeCount = value; }
		public Vector2Int WallpaperPatternCount { get => m_WallpaperPatternCount; set => m_WallpaperPatternCount = value; }
		public PixelSprite WallpaperPattern { get => m_WallpaperPattern; set => m_WallpaperPattern = value; }
		public Vector2 SandEdge { get => m_SandEdge; set => m_SandEdge = value; }
		public Vector2 SandGravel { get => m_SandGravel; set => m_SandGravel = value; }
		public Vector2Int SandGravelGapX { get => m_SandGravelGapX; set => m_SandGravelGapX = value; }
		public Vector2Int SandGravelGapY { get => m_SandGravelGapY; set => m_SandGravelGapY = value; }
		public Vector2Int SandTraceCountX { get => m_SandTraceCountX; set => m_SandTraceCountX = value; }
		public Vector2Int SandTraceCountY { get => m_SandTraceCountY; set => m_SandTraceCountY = value; }
		public Vector2Int SandTraceLength { get => m_SandTraceLength; set => m_SandTraceLength = value; }

		// Ser
		[SerializeField, MinMaxNumber(0f, 1f, 0.1f)] Vector2 m_StoneEdge = new Vector2(0.2f, 0.8f);
		[SerializeField, MinMaxNumber(1, 128)] Vector2Int m_StoneGroupX = new Vector2Int(1, 4);
		[SerializeField, MinMaxNumber(1, 128)] Vector2Int m_StoneGroupY = new Vector2Int(1, 4);

		[SerializeField, MinMaxNumber(0f, 1f, 0.1f)] Vector2 m_CrackedStoneEdge = new Vector2(0.2f, 0.8f);
		[SerializeField, MinMaxNumber(1, 128)] Vector2Int m_CrackedStonePointCount = new Vector2Int(2, 6);

		[SerializeField, MinMaxNumber(0f, 1f, 0.1f)] Vector2 m_MarbleCrackAmount = new Vector2(0.2f, 0.8f);

		[SerializeField, MinMaxNumber(0f, 1f, 0.1f)] Vector2 m_DirtEdge = new Vector2(0.2f, 0.8f);
		[SerializeField, MinMaxNumber(0f, 1f, 0.1f)] Vector2 m_DirtGravel = new Vector2(0.2f, 0.8f);
		[SerializeField, MinMaxNumber(0)] Vector2Int m_DirtGravelGapX = new Vector2Int(2, 5);
		[SerializeField, MinMaxNumber(0)] Vector2Int m_DirtGravelGapY = new Vector2Int(2, 5);

		[SerializeField, MinMaxNumber(0f, 1f, 0.1f)] Vector2 m_IceHighlight = new Vector2(0.1f, 0.6f);
		[SerializeField, MinMaxNumber(0)] Vector2Int m_IceHighlightSize = new Vector2Int(1, 4);
		[SerializeField, MinMaxNumber(0)] Vector2Int m_IceSpecularSize = new Vector2Int(2, 6);
		[SerializeField, MinMaxNumber(0)] Vector2Int m_IceDarkSpecularSize = new Vector2Int(1, 2);

		[SerializeField, MinMaxNumber(1)] Vector2Int m_BrickRow = new Vector2Int(2, 4);
		[SerializeField, MinMaxNumber(2)] Vector2Int m_BrickWidth = new Vector2Int(4, 10);

		[SerializeField] bool m_MetalEdge = true;
		[SerializeField, MinMaxNumber(0f, 1f, 0.1f)] Vector2 m_MetalHighlightTint = new Vector2(0.2f, 0.5f);
		[SerializeField, MinMaxNumber(0)] Vector2Int m_MetalHighlight = new Vector2Int(1, 6);
		[SerializeField, MinMaxNumber(0)] Vector2Int m_MetalNailCountX = new Vector2Int(3, 5);
		[SerializeField, MinMaxNumber(0)] Vector2Int m_MetalNailCountY = new Vector2Int(3, 5);
		[SerializeField, MinMaxNumber(0)] Vector2Int m_MetalSunkenSizeX = new Vector2Int(3, 8);
		[SerializeField, MinMaxNumber(0)] Vector2Int m_MetalSunkenSizeY = new Vector2Int(3, 8);
		[SerializeField, MinMaxNumber(0f, 1f, 0.1f)] Vector2 m_MetalRustAmount = new Vector2(0f, 0.3f);

		[SerializeField] bool m_IronEdge = true;
		[SerializeField, MinMaxNumber(0f, 1f, 0.1f)] Vector2 m_IronHighlightTint = new Vector2(0.2f, 0.5f);
		[SerializeField, MinMaxNumber(0)] Vector2Int m_IronHighlight = new Vector2Int(1, 6);
		[SerializeField, MinMaxNumber(0)] Vector2Int m_IronPatternSize = new Vector2Int(2, 5);
		[SerializeField, MinMaxNumber(0f, 1f, 0.1f)] Vector2 m_IronRustAmount = new Vector2(0f, 0.3f);

		[SerializeField] bool m_FloorNail = true;
		[SerializeField, MinMaxNumber(1)] Vector2Int m_FloorRow = new Vector2Int(2, 4);

		[SerializeField, MinMaxNumber(1)] Vector2Int m_WindowGlassColumn = new Vector2Int(1, 3);
		[SerializeField, MinMaxNumber(1)] Vector2Int m_WindowGlassRow = new Vector2Int(1, 3);
		[SerializeField] bool m_WindowGlassDithering = true;

		[SerializeField, MinMaxNumber(0)] Vector2Int m_WallpaperStripeCount = new Vector2Int(2, 4);
		[SerializeField, MinMaxNumber(0)] Vector2Int m_WallpaperPatternCount = new Vector2Int(0, 6);
		[SerializeField, PixelEditor] PixelSprite m_WallpaperPattern = new PixelSprite(5, 5);

		[SerializeField, MinMaxNumber(0f, 1f, 0.1f)] Vector2 m_SandEdge = new Vector2(0f, 0.8f);
		[SerializeField, MinMaxNumber(0f, 1f, 0.1f)] Vector2 m_SandGravel = new Vector2(0.2f, 0.8f);
		[SerializeField, MinMaxNumber(1)] Vector2Int m_SandGravelGapX = new Vector2Int(2, 4);
		[SerializeField, MinMaxNumber(1)] Vector2Int m_SandGravelGapY = new Vector2Int(2, 4);
		[SerializeField, MinMaxNumber(0)] Vector2Int m_SandTraceCountX = new Vector2Int(2, 4);
		[SerializeField, MinMaxNumber(0)] Vector2Int m_SandTraceCountY = new Vector2Int(2, 4);
		[SerializeField, MinMaxNumber(0)] Vector2Int m_SandTraceLength = new Vector2Int(4, 8);


	}
}
