using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace HololiaGame {

	public class eCheckIna : CheckPoint { }
	[LinkedCheckPoint(typeof(eCheckIna))] public class eAltarIna : CheckAltar { }

	public class eCheckGura : CheckPoint { }
	[LinkedCheckPoint(typeof(eCheckGura))] public class eAltarGura : CheckAltar { }

}
