using System.Collections.Generic;
using System.Collections;
using System.Reflection;

namespace AngeliA;

public interface IWire {
	public bool ConnectedLeft { get; }
	public bool ConnectedRight { get; }
	public bool ConnectedDown { get; }
	public bool ConnectedUp { get; }
}