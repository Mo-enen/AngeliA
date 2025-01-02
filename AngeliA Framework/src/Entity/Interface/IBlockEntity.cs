namespace AngeliA;

public interface IBlockEntity {
	public int MaxStackCount => 256;
	public void OnEntityPicked () { }
	public void OnEntityPut () { }
	public void OnEntityRefresh () { }
}
