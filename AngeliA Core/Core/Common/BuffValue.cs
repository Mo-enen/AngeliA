using System.Collections;
using System.Collections.Generic;


namespace AngeliA; 


public class BuffInt {

	public int FinalValue => Override ?? BaseValue;
	public int BaseValue = 0;

	public int? Override = null;

	public BuffInt (int value = 0) {
		BaseValue = value;
		Override = null;
	}
	public static implicit operator int (BuffInt bInt) => bInt.FinalValue;

}


public class BuffBool {

	public bool FinalValue => Override ?? BaseValue;
	public bool BaseValue = true;

	public bool? Override = null;

	public BuffBool (bool value = true) {
		BaseValue = value;
		Override = null;
	}
	public static implicit operator bool (BuffBool bBool) => bBool.FinalValue;

}


public class BuffString {

	public string FinalValue => Override != null ? BaseValue : Override;
	public string BaseValue = "";

	public string Override = null;

	public BuffString (string value = "") {
		BaseValue = value;
		Override = null;
	}
	public static implicit operator string (BuffString bStr) => bStr.FinalValue;

}