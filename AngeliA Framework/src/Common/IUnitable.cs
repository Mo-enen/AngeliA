namespace AngeliA;

public interface IUnitable {

	public enum UniteMode { Horizontal, Vertical, FourDirection, EightDirection, }

	public static object UniteTempParam { get; private set; } = null;
	public static int GlobalUniteStamp = int.MinValue;
	public static readonly Pipe<Int2> UnitedPositions = new(1024);
	public int LocalUniteStamp { get; set; }

	public static void ForAllPartializedEntity<E> (int physicsMask, int entityID, IRect rect, OperationMode mode, UniteMode partialMode, System.Action<E> results, object param = null) where E : IUnitable {
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

			if (Physics.GetEntity(entityID, rect, physicsMask, mode: mode) is not E entity) continue;
			if (entity.LocalUniteStamp == stamp) continue;
			entity.LocalUniteStamp = stamp;
			results.Invoke(entity);

			// Link Connected
			switch (partialMode) {
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
