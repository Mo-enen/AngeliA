using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace AngeliA;

public class RiggedRespondMessage {


	private int GlobalFrame;
	private int PauselessFrame;

	// API
	public void SetDataToFramework () {

		// Rendering





		// Audio




	}


	public void ReadDataFromPipe (BinaryReader reader) {

		GlobalFrame = reader.ReadInt32(); // End of stream excp
		PauselessFrame = reader.ReadInt32();

	}


}