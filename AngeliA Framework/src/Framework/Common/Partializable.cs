namespace AngeliA;


public enum PartializedMode { Horizontal, Vertical, FourDirection, EightDirection, }


public interface IPartializable {

	public static object PartializeTempParam { get; private set; } = null;
	public static int GlobalPartializeStamp = int.MinValue;
	public static readonly Pipe<Int2> PartializedPositions = new(1024);
	public int PartializeStamp { get; set; }

	public static void ForAllPartializedEntity<E> (int physicsMask, int entityID, IRect rect, OperationMode mode, PartializedMode partialMode, System.Action<E> results, object param = null) where E : IPartializable {
		PartializeTempParam = param;
		int eWidth = rect.width;
		int eHeight = rect.height;
		int stamp = GlobalPartializeStamp++;
		PartializedPositions.Reset();
		PartializedPositions.LinkToTail(rect.position);
		rect = new IRect(0, 0, 1, 1);
		for (int safe = 0; PartializedPositions.TryPopHead(out var pos) && safe < 4096; safe++) {

			rect.x = pos.x + eWidth / 2;
			rect.y = pos.y + eHeight / 2;

			if (Physics.GetEntity(entityID, rect, physicsMask, mode: mode) is not E entity) continue;
			if (entity.PartializeStamp == stamp) continue;
			entity.PartializeStamp = stamp;
			results.Invoke(entity);

			// Link Connected
			switch (partialMode) {
				case PartializedMode.Horizontal:
					PartializedPositions.LinkToTail(pos.Shift(-eWidth, 0));
					PartializedPositions.LinkToTail(pos.Shift(eWidth, 0));
					break;
				case PartializedMode.Vertical:
					PartializedPositions.LinkToTail(pos.Shift(0, -eHeight));
					PartializedPositions.LinkToTail(pos.Shift(0, eHeight));
					break;
				case PartializedMode.FourDirection:
					PartializedPositions.LinkToTail(pos.Shift(-eWidth, 0));
					PartializedPositions.LinkToTail(pos.Shift(eWidth, 0));
					PartializedPositions.LinkToTail(pos.Shift(0, -eHeight));
					PartializedPositions.LinkToTail(pos.Shift(0, eHeight));
					break;
				case PartializedMode.EightDirection:
					PartializedPositions.LinkToTail(pos.Shift(-eWidth, -eHeight));
					PartializedPositions.LinkToTail(pos.Shift(-eWidth, eHeight));
					PartializedPositions.LinkToTail(pos.Shift(eWidth, -eHeight));
					PartializedPositions.LinkToTail(pos.Shift(eWidth, eHeight));
					PartializedPositions.LinkToTail(pos.Shift(0, -eHeight));
					PartializedPositions.LinkToTail(pos.Shift(0, eHeight));
					PartializedPositions.LinkToTail(pos.Shift(-eWidth, 0));
					PartializedPositions.LinkToTail(pos.Shift(eWidth, 0));
					break;
			}
		}
		PartializedPositions.Reset();
		PartializeTempParam = null;
	}
}
