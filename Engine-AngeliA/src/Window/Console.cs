using System.Collections.Generic;
using System.Collections;
using AngeliA;

namespace AngeliaEngine;

public class Console : WindowUI {




	#region --- VAR ---



	#endregion




	#region --- MSG ---


	public Console () {
		Debug.OnLog += OnLog;
		Debug.OnLogError += OnLogError;
		Debug.OnLogWarning += OnLogWarning;
		Debug.OnLogException += OnLogException;
	}


	~Console () {
		Debug.OnLog -= OnLog;
		Debug.OnLogError -= OnLogError;
		Debug.OnLogWarning -= OnLogWarning;
		Debug.OnLogException -= OnLogException;
	}


	public override void UpdateWindowUI () {

	}


	#endregion




	#region --- LGC ---


	private void OnLog (object obj) {

	}


	private void OnLogWarning (object obj) {

	}


	private void OnLogError (object obj) {

	}


	private void OnLogException (System.Exception ex) {

	}


	#endregion




}