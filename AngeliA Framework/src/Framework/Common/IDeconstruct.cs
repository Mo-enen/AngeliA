namespace AngeliA;

public interface IDeconstruct<A, B> {
	public void Deconstruct (out A a, out B b);
}


public interface IDeconstruct<A, B, C> {
	public void Deconstruct (out A a, out B b, out C c);
}


