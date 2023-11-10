using AngeliaFramework;


namespace AngeliaGame {


	public class CheckLalynnA : CheckPoint { }
	public class CheckMage : CheckPoint { }
	public class CheckElf : CheckPoint { }
	public class CheckDragon : CheckPoint { }
	public class CheckTorch : CheckPoint { }
	public class CheckSlime : CheckPoint { }
	public class CheckInsect : CheckPoint { }
	public class CheckOrc : CheckPoint { }
	public class CheckMelon : CheckPoint { }
	public class CheckCrow : CheckPoint { }
	public class CheckBone : CheckPoint { }
	public class CheckFootman : CheckPoint { }
	public class CheckKnight : CheckPoint { }
	public class CheckJesus : CheckPoint { }
	public class CheckShield : CheckPoint { }
	public class CheckGamble : CheckPoint { }
	public class CheckScience : CheckPoint { }
	public class CheckSpider : CheckPoint { }
	public class CheckStalactite : CheckPoint { }
	public class CheckSword : CheckPoint { }
	public class CheckSpace : CheckPoint { }
	public class CheckMachineGun : CheckPoint { }
	public class CheckKnowledge : CheckPoint { }
	public class CheckCat : CheckPoint { }



	[LinkedCheckPoint(typeof(CheckLalynnA))] public class AltarLalynnA : CheckAltar { }
	[LinkedCheckPoint(typeof(CheckMage))] public class AltarMage : CheckAltar { }
	[LinkedCheckPoint(typeof(CheckElf))] public class AltarElf : CheckAltar { }
	[LinkedCheckPoint(typeof(CheckDragon))] public class AltarDragon : CheckAltar { }
	[LinkedCheckPoint(typeof(CheckTorch))] public class AltarTorch : CheckAltar { }
	[LinkedCheckPoint(typeof(CheckSlime))] public class AltarSlime : CheckAltar { }
	[LinkedCheckPoint(typeof(CheckInsect))] public class AltarInsect : CheckAltar { }
	[LinkedCheckPoint(typeof(CheckOrc))] public class AltarOrc : CheckAltar { }
	[LinkedCheckPoint(typeof(CheckMelon))] public class AltarMelon : CheckAltar { }
	[LinkedCheckPoint(typeof(CheckCrow))] public class AltarCrow : CheckAltar { }
	[LinkedCheckPoint(typeof(CheckBone))] public class AltarBone : CheckAltar { }
	[LinkedCheckPoint(typeof(CheckFootman))] public class AltarFootman : CheckAltar { }
	[LinkedCheckPoint(typeof(CheckKnight))] public class AltarKnight : CheckAltar { }
	[LinkedCheckPoint(typeof(CheckJesus))] public class AltarJesus : CheckAltar { }
	[LinkedCheckPoint(typeof(CheckShield))] public class AltarShield : CheckAltar { }
	[LinkedCheckPoint(typeof(CheckGamble))] public class AltarGamble : CheckAltar { }
	[LinkedCheckPoint(typeof(CheckScience))] public class AltarScience : CheckAltar { }
	[LinkedCheckPoint(typeof(CheckSpider))] public class AltarSpider : CheckAltar { }
	[LinkedCheckPoint(typeof(CheckStalactite))] public class AltarStalactite : CheckAltar { }
	[LinkedCheckPoint(typeof(CheckSword))] public class AltarSword : CheckAltar { }
	[LinkedCheckPoint(typeof(CheckSpace))] public class AltarSpace : CheckAltar { }
	[LinkedCheckPoint(typeof(CheckMachineGun))] public class AltarMachineGun : CheckAltar { }
	[LinkedCheckPoint(typeof(CheckKnowledge))] public class AltarKnowledge : CheckAltar { }
	[LinkedCheckPoint(typeof(CheckCat))] public class AltarCat : CheckAltar { }


}