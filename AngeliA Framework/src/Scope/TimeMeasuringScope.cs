using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public readonly struct TimeMeasuringScope : System.IDisposable {
	private readonly string Label = "";
	public TimeMeasuringScope (string label = "") {
		Label = label;
		Debug.BeginTimeMeasuring();
	}
	public readonly void Dispose () => Debug.LogTimeMeasuring(Label);
}
