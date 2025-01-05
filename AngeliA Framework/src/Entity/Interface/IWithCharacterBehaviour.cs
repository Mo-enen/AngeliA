namespace AngeliA;


public interface IWithCharacterMovement {
	CharacterMovement CurrentMovement { get; }
}


public interface IWithCharacterAttackness {
	CharacterAttackness CurrentAttackness { get; }
}


public interface IWithCharacterBuff {
	CharacterBuff CurrentBuff { get; }
}


public interface IWithCharacterHealth {
	CharacterHealth CurrentHealth { get; }
}


public interface IWithCharacterRenderer {
	CharacterRenderer CurrentRenderer { get; }
}


