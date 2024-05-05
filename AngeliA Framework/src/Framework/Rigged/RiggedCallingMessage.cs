using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AngeliA;

public class RiggedCallingMessage {



	// Data
	private int GlobalFrame;
	private int PauselessFrame;


	// API
	public void LoadDataFromFramework () {

		// Frame
		GlobalFrame = Game.GlobalFrame;
		PauselessFrame = Game.PauselessFrame;


		// Input



	}


	public void WriteDataToPipe (BinaryWriter writer) {

		writer.Write(GlobalFrame);
		writer.Write(PauselessFrame);



	}


}