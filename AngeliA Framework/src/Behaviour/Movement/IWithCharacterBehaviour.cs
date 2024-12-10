namespace AngeliA;

public interface IWithCharacterMovement {
	CharacterMovement CurrentMovement { get; }
}

public interface IWithCharacterAttackness {
	CharacterAttackness CurrentAttackness { get; }
}