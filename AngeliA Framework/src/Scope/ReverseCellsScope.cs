using System;

namespace AngeliA;

/// <summary>
/// Scope that make rendering cells reverse in sorting order
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
/// 		using (new ReverseCellsScope()) {
/// 
/// 			// Rendering cells inside will be reversed in sorting order
/// 
/// 		}
/// 
/// 	}
/// 
/// }
/// </code></example>	
public readonly struct ReverseCellsScope : IDisposable {
	private readonly int LayerIndex;
	private readonly int UsedCount;
	/// <summary>
	/// Scope that make rendering cells reverse in sorting order
	/// </summary>
	public ReverseCellsScope () {
		LayerIndex = Renderer.CurrentLayerIndex;
		UsedCount = Renderer.GetUsedCellCount();
	}
	public readonly void Dispose () {
		int start = UsedCount;
		if (Renderer.GetCells(LayerIndex, out var cells, out int count) && start < count) {
			//System.Array.Reverse(cells, start, count - start);
			for (int i = start, j = count - 1; i < j; i++, j--) {
				(cells[i], cells[j]) = (cells[j], cells[i]);
			}
		}
	}
}
