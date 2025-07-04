namespace AngeliA;

/// <summary>
/// Scope that draw shadows for rendering cells inside
/// </summary>
/// <example><code>
/// using AngeliA;
/// 
/// namespace AngeliaGame;
/// 
/// public class Example {
/// 
/// 	[OnGameUpdate]
/// 	internal static void OnGameUpdate () {
/// 
/// 		using (new EnvironmentShadowScope()) {
/// 
/// 			// Rendering cell created inside will have a shadow
/// 			Renderer.Draw(BuiltInSprite.ICON_ENTITY, Renderer.CameraRect.Shrink(Const.CEL * 8));
/// 
/// 		}
/// 
/// 	}
/// 
/// }
/// </code></example>	
public readonly struct EnvironmentShadowScope : System.IDisposable {

	/// <summary>
	/// Shadow distance X in global space
	/// </summary>
	public readonly int OffsetX;
	/// <summary>
	/// Shadow distance Y in global space
	/// </summary>
	public readonly int OffsetY;
	/// <summary>
	/// Transparency value of the shadow. (0 means transparent, 255 means not tramsparent)
	/// </summary>
	public readonly byte Alpha;
	/// <summary>
	/// Z value for sort rendering cells
	/// </summary>
	public readonly int Z;
	private readonly int StartIndex = Renderer.GetUsedCellCount();
	private readonly int LayerIndex = Renderer.CurrentLayerIndex;

	/// <summary>
	/// Scope that draw shadows for rendering cells inside
	/// </summary>
	/// <param name="offsetX">Shadow distance X in global space</param>
	/// <param name="offsetY">Shadow distance Y in global space</param>
	/// <param name="alpha">Transparency value of the shadow. (0 means transparent, 255 means not tramsparent)</param>
	/// <param name="z">Z value for sort rendering cells</param>
	public EnvironmentShadowScope (int offsetX = -64, int offsetY = 0, byte alpha = 64, int z = -65520) {
		OffsetX = offsetX;
		OffsetY = offsetY;
		Alpha = alpha;
		Z = z;
	}
	
	/// <summary>
	/// Scope that draw shadows for rendering cells inside
	/// </summary>
	public EnvironmentShadowScope () : this(-64, 0, 64, -65520) { }
	
	public readonly void Dispose () {
		if (Renderer.GetCells(LayerIndex, out var cells, out int count)) {
			for (int i = StartIndex; i < count; i++) {
				FrameworkUtil.DrawEnvironmentShadow(cells[i], OffsetX, OffsetY, Alpha, Z);
			}
		}
	}

}
