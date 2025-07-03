namespace AngeliA;


/// <summary>
/// Interface that makes the entity contains a instance of character movement behaviour
/// </summary>
public interface IWithCharacterMovement {
	CharacterMovement CurrentMovement { get; }
}


/// <summary>
/// Interface that makes the entity contains a instance of character attackness behaviour
/// </summary>
public interface IWithCharacterAttackness {
	CharacterAttackness CurrentAttackness { get; }
}


/// <summary>
/// Interface that makes the entity contains a instance of character buff behaviour
/// </summary>
public interface IWithCharacterBuff {
	CharacterBuff CurrentBuff { get; }
}


/// <summary>
/// Interface that makes the entity contains a instance of character health behaviour
/// </summary>
public interface IWithCharacterHealth {
	CharacterHealth CurrentHealth { get; }
}


/// <summary>
/// Interface that makes the entity contains a instance of character rendering behaviour
/// </summary>
public interface IWithCharacterRenderer {
	CharacterRenderer CurrentRenderer { get; }
}


