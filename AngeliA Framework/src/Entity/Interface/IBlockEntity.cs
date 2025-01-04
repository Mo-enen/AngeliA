namespace AngeliA;

public interface IBlockEntity {
	public int MaxStackCount => 256;
	public bool ContainEntityAsElement => false;
	public void OnEntityPicked () { }
	public void OnEntityPut () { }
	public void OnEntityRefresh () { }
}
