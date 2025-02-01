namespace AngeliA;


public interface IWithCharacterMovement {
	CharacterMovement CurrentMovement { get; }
}


public interface IWithCharacterAttackness {
	CharacterAttackness CurrentAttackness { get; }
}


public interface IWithCharacterBuff {
	CharacterBuff CurrentBuff { get; }
	public static void GiveBuffFromMap (IWithCharacterBuff wBuff, int unitX = int.MinValue, int unitY = int.MinValue, int unitZ = int.MinValue, int duration = -1) {
		if (wBuff is not Entity entity) return;
		unitX = unitX == int.MinValue ? (entity.X + 1).ToUnit() : unitX;
		unitY = unitY == int.MinValue ? (entity.Y + 1).ToUnit() : unitY;
		unitZ = unitZ == int.MinValue ? Stage.ViewZ : unitZ;
		int id = WorldSquad.Front.GetBlockAt(unitX, unitY, unitZ, BlockType.Element);
		if (id == 0) return;
		wBuff.CurrentBuff.GiveBuff(id, duration);
	}
}


public interface IWithCharacterHealth {
	CharacterHealth CurrentHealth { get; }
}


public interface IWithCharacterRenderer {
	CharacterRenderer CurrentRenderer { get; }
}


