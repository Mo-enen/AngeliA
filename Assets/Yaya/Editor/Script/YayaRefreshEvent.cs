using System.Collections;
using System.Collections.Generic;
using AngeliaFramework;
using AngeliaFramework.Editor;
using UnityEngine;


namespace Yaya.Editor {
	public class YayaArtworkExtension : IRefreshEvent {


		public string Message => "Yaya Refresh Events";


		public void Refresh () {
			YayaUtil.CreateCameraScrollMetaFile(Const.BuiltInMapRoot);
			YayaUtil.CreateCameraScrollMetaFile(Const.UserMapRoot);
		}



	}
}