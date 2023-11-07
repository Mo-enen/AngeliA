using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


public class ChromaticAberrationEffect : AngeliaScreenEffect {



	// Const
	private static readonly int RED_X = Shader.PropertyToID("_RX");
	private static readonly int RED_Y = Shader.PropertyToID("_RY");
	private static readonly int GREEN_X = Shader.PropertyToID("_GX");
	private static readonly int GREEN_Y = Shader.PropertyToID("_GY");
	private static readonly int BLUE_X = Shader.PropertyToID("_BX");
	private static readonly int BLUE_Y = Shader.PropertyToID("_BY");

	// Api
	public override int Order => 0;
	public static float PingPongTime { get; set; } = 0.618f;
	public static float PingPongMin { get; set; } = 0f;
	public static float PingPongMax { get; set; } = 0.015f;

	// Data
	private static readonly System.Random Ran = new(1928736456);
	private float EnableTime = 0f;


	// API
	public override Shader GetShader () => Shader.Find("Angelia/ChromaticAberration");


	// MSG
	private void OnEnable () => EnableTime = Time.time;


	private void Update () {
		if (Material == null) return;
		if ((Time.time - EnableTime) % PingPongTime > PingPongTime / 2f) {
			Material.SetFloat(RED_X, GetRandomAmount(0f));
			Material.SetFloat(RED_Y, GetRandomAmount(0.2f));
			Material.SetFloat(BLUE_X, GetRandomAmount(0.7f));
			Material.SetFloat(BLUE_Y, GetRandomAmount(0.4f));
		} else {
			Material.SetFloat(GREEN_X, GetRandomAmount(0f));
			Material.SetFloat(GREEN_Y, GetRandomAmount(0.8f));
			Material.SetFloat(BLUE_X, GetRandomAmount(0.4f));
			Material.SetFloat(BLUE_Y, GetRandomAmount(0.72f));
		}
	}


	private float GetRandomAmount (float timeOffset = 0f) {
		int range = (int)(Util.RemapUnclamped(
			0f, PingPongTime,
			PingPongMin, PingPongMax,
			Mathf.PingPong(Time.time - EnableTime + timeOffset * PingPongTime, PingPongTime)
		) * 100000f);
		return Ran.Next(-range, range) / 100000f;
	}


}