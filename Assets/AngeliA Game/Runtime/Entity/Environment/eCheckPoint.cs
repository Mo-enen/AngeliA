using AngeliaFramework;


namespace AngeliaGame {


	public class eCheckLalynnA : CheckPoint { }
	public class eCheckMage : CheckPoint { }
	public class eCheckElf : CheckPoint { }
	public class eCheckDragon : CheckPoint { }
	public class eCheckTorch : CheckPoint { }
	public class eCheckSlime : CheckPoint { }
	public class eCheckInsect : CheckPoint { }
	public class eCheckOrc : CheckPoint { }
	public class eCheckTako : CheckPoint { }
	public class eCheckShark : CheckPoint { }
	public class eCheckBone : CheckPoint { }
	public class eCheckFootman : CheckPoint { }
	public class eCheckKnight : CheckPoint { }
	public class eCheckJesus : CheckPoint { }
	public class eCheckShield : CheckPoint { }
	public class eCheckGamble : CheckPoint { }
	public class eCheckScience : CheckPoint { }
	public class eCheckSpider : CheckPoint { }
	public class eCheckStalactite : CheckPoint { }
	public class eCheckSword : CheckPoint { }
	public class eCheckSpace : CheckPoint { }
	public class eCheckMachineGun : CheckPoint { }
	public class eCheckKnowledge : CheckPoint { }
	public class eCheckCat : CheckPoint { }


	public class eAltarLalynnA : CheckAltar {
		protected override int GetLinkedCheckPointID () => typeof(eCheckLalynnA).AngeHash();
	}
	public class eAltarMage : CheckAltar {
		protected override int GetLinkedCheckPointID () => typeof(eCheckMage).AngeHash();
	}
	public class eAltarElf : CheckAltar {
		protected override int GetLinkedCheckPointID () => typeof(eCheckElf).AngeHash();
	}
	public class eAltarDragon : CheckAltar {
		protected override int GetLinkedCheckPointID () => typeof(eCheckDragon).AngeHash();
	}
	public class eAltarTorch : CheckAltar {
		protected override int GetLinkedCheckPointID () => typeof(eCheckTorch).AngeHash();
	}
	public class eAltarSlime : CheckAltar {
		protected override int GetLinkedCheckPointID () => typeof(eCheckSlime).AngeHash();
	}
	public class eAltarInsect : CheckAltar {
		protected override int GetLinkedCheckPointID () => typeof(eCheckInsect).AngeHash();
	}
	public class eAltarOrc : CheckAltar {
		protected override int GetLinkedCheckPointID () => typeof(eCheckOrc).AngeHash();
	}
	public class eAltarTako : CheckAltar {
		protected override int GetLinkedCheckPointID () => typeof(eCheckTako).AngeHash();
	}
	public class eAltarShark : CheckAltar {
		protected override int GetLinkedCheckPointID () => typeof(eCheckShark).AngeHash();
	}
	public class eAltarBone : CheckAltar {
		protected override int GetLinkedCheckPointID () => typeof(eCheckBone).AngeHash();
	}
	public class eAltarFootman : CheckAltar {
		protected override int GetLinkedCheckPointID () => typeof(eCheckFootman).AngeHash();
	}
	public class eAltarKnight : CheckAltar {
		protected override int GetLinkedCheckPointID () => typeof(eCheckKnight).AngeHash();
	}
	public class eAltarJesus : CheckAltar {
		protected override int GetLinkedCheckPointID () => typeof(eCheckJesus).AngeHash();
	}
	public class eAltarShield : CheckAltar {
		protected override int GetLinkedCheckPointID () => typeof(eCheckShield).AngeHash();
	}
	public class eAltarGamble : CheckAltar {
		protected override int GetLinkedCheckPointID () => typeof(eCheckGamble).AngeHash();
	}
	public class eAltarScience : CheckAltar {
		protected override int GetLinkedCheckPointID () => typeof(eCheckScience).AngeHash();
	}
	public class eAltarSpider : CheckAltar {
		protected override int GetLinkedCheckPointID () => typeof(eCheckSpider).AngeHash();
	}
	public class eAltarStalactite : CheckAltar {
		protected override int GetLinkedCheckPointID () => typeof(eCheckStalactite).AngeHash();
	}
	public class eAltarSword : CheckAltar {
		protected override int GetLinkedCheckPointID () => typeof(eCheckSword).AngeHash();
	}
	public class eAltarSpace : CheckAltar {
		protected override int GetLinkedCheckPointID () => typeof(eCheckSpace).AngeHash();
	}
	public class eAltarMachineGun : CheckAltar {
		protected override int GetLinkedCheckPointID () => typeof(eCheckMachineGun).AngeHash();
	}
	public class eAltarKnowledge : CheckAltar {
		protected override int GetLinkedCheckPointID () => typeof(eCheckKnowledge).AngeHash();
	}
	public class eAltarCat : CheckAltar {
		protected override int GetLinkedCheckPointID () => typeof(eCheckCat).AngeHash();
	}


}