using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using UnityEngine;
using AngeliaFramework;
using Riptide.Utils;
using Riptide;

namespace System.Runtime.CompilerServices { internal static class IsExternalInit { } }



namespace AngeliaGame {



	public static class Angelia {


#if UNITY_EDITOR



		//[UnityEditor.InitializeOnLoadMethod]
		public static void Test () {

			//Server server = new Server();
			//server.Start(7777, 10);
			//
			//var msg = Message.Create(MessageSendMode.Reliable, 0);
			////server.SendToAll();
			//
			//
			//var e = System.Net.Dns.GetHostEntry("publisher.assetstore.unity3d.com");
			//foreach (var address in e.AddressList) {
			//	Debug.Log(address.ToString());
			//}
		}



#endif

	}
}