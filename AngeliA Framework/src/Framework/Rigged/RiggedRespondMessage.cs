using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace AngeliA;

public class RiggedRespondMessage {


	int testA;
	int testB;

	// API
	public void SetDataToFramework () {

		// Rendering

		Debug.Log(testA + " " + testB);



		// Audio




	}


	public void ReadDataFromPipe (BinaryReader reader) {

		testA = reader.ReadInt32();
		testB = reader.ReadInt32();

	}


}