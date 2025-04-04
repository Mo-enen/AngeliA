using AngeliA;

namespace AngeliA.Platformer;

/// <summary>
/// Map blocks that connect to each other and forms a group
/// </summary>
public interface IUnitable {

	/// <summary>
	/// Which direction will be connected
	/// </summary>
	public enum UniteMode {
		/// <summary>
		/// Left and right
		/// </summary>
		Horizontal,
		/// <summary>
		/// Up and down
		/// </summary>
		Vertical,
		/// <summary>
		/// Left right up and down
		/// </summary>
		FourDirection,
		/// <summary>
		/// Left right up down and diagonal
		/// </summary>
		EightDirection,
	}

	/// <summary>
	/// Custom object that comes from ForAllUnitedEntity call. Only use this inside the "System.Action{E} results" function.
	/// </summary>
	public static object UniteTempParam { get; private set; } = null;
	public int LocalUniteStamp { get; set; }

	private static readonly Pipe<Int2> UnitedPositions = new(1024);
	private static int GlobalUniteStamp = int.MinValue;

	/// <summary>
	/// Iterate through all connected IUnitable as a group
	/// </summary>
	/// <param name="physicsMask">Which physics layer is include for entity searching</param>
	/// <param name="entityID">Target entity type ID</param>
	/// <param name="rect">Rect position in global space</param>
	/// <param name="mode"></param>
	/// <param name="uniteMode">Which direction count as connected</param>
	/// <param name="results">Invoke this action for all founded result</param>
	/// <param name="param">Custom data for the "result" action. Use IUnitable.UniteTempParam inside result action to get this data.</param>
	public static void ForAllUnitedEntity (int physicsMask, int entityID, IRect rect, OperationMode mode, UniteMode uniteMode, System.Action<IUnitable> results, object param = null) {
		UniteTempParam = param;
		int eWidth = rect.width;
		int eHeight = rect.height;
		int stamp = GlobalUniteStamp++;
		UnitedPositions.Reset();
		UnitedPositions.LinkToTail(rect.position);
		rect = new IRect(0, 0, 1, 1);
		for (int safe = 0; UnitedPositions.TryPopHead(out var pos) && safe < 4096; safe++) {

			rect.x = pos.x + eWidth / 2;
			rect.y = pos.y + eHeight / 2;

			if (Physics.GetEntity(entityID, rect, physicsMask, mode: mode) is not IUnitable entity) continue;
			if (entity.LocalUniteStamp == stamp) continue;
			entity.LocalUniteStamp = stamp;
			results.Invoke(entity);

			// Link Connected
			switch (uniteMode) {
				case UniteMode.Horizontal:
					UnitedPositions.LinkToTail(pos.Shift(-eWidth, 0));
					UnitedPositions.LinkToTail(pos.Shift(eWidth, 0));
					break;
				case UniteMode.Vertical:
					UnitedPositions.LinkToTail(pos.Shift(0, -eHeight));
					UnitedPositions.LinkToTail(pos.Shift(0, eHeight));
					break;
				case UniteMode.FourDirection:
					UnitedPositions.LinkToTail(pos.Shift(-eWidth, 0));
					UnitedPositions.LinkToTail(pos.Shift(eWidth, 0));
					UnitedPositions.LinkToTail(pos.Shift(0, -eHeight));
					UnitedPositions.LinkToTail(pos.Shift(0, eHeight));
					break;
				case UniteMode.EightDirection:
					UnitedPositions.LinkToTail(pos.Shift(-eWidth, -eHeight));
					UnitedPositions.LinkToTail(pos.Shift(-eWidth, eHeight));
					UnitedPositions.LinkToTail(pos.Shift(eWidth, -eHeight));
					UnitedPositions.LinkToTail(pos.Shift(eWidth, eHeight));
					UnitedPositions.LinkToTail(pos.Shift(0, -eHeight));
					UnitedPositions.LinkToTail(pos.Shift(0, eHeight));
					UnitedPositions.LinkToTail(pos.Shift(-eWidth, 0));
					UnitedPositions.LinkToTail(pos.Shift(eWidth, 0));
					break;
			}
		}
		UnitedPositions.Reset();
		UniteTempParam = null;
	}

}
