namespace AngeliA;

public sealed class BuffIndex<B> where B : Buff {
	public int Index = -1;
	public static implicit operator int (BuffIndex<B> buffI) {
		if (buffI.Index < 0) {
			int len = Buff.AllBuffCount;
			for (int i = 0; i < len; i++) {
				if (Buff.GetBuffAt(i) is B) {
					buffI.Index = i;
					return i;
				}
			}
			return 0;
		}
		return buffI.Index;
	}
}
