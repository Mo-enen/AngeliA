using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework.World;
using AngeliaFramework.Renderer;


namespace AngeliaFramework.Demo {
	public class Tester : MonoBehaviour {



		// Ser
		[SerializeField] TextMesh State = null;
		[SerializeField] Material Material = null;
		[SerializeField] bool RandomRenderer = true;
		[SerializeField] bool KeepRandom = true;

		// Data
		private float LastFpsTime = float.MinValue;
		private float LastFpsTimeFixed = float.MinValue;
		private float UpdateFPS = 0f;
		private float FixedFPS = 0f;



		// MSG
		private void Awake () {
			Application.targetFrameRate = 10000;

			// Renderer
			CellRenderer.InitLayers(6);
			for (int i = 0; i < 6; i++) {
				CellRenderer.SetupLayer(i, 512, Material, new Rect[] {
					new Rect(Random.Range(0f, 0.95f), Random.Range(0f, 0.95f), 0.05f, 0.05f),
					new Rect(Random.Range(0f, 0.95f), Random.Range(0f, 0.95f), 0.05f, 0.05f),
					new Rect(Random.Range(0f, 0.95f), Random.Range(0f, 0.95f), 0.05f, 0.05f),
					new Rect(Random.Range(0f, 0.95f), Random.Range(0f, 0.95f), 0.05f, 0.05f),
				});
			}

		}


		private void Update () {
			// Update State
			if (Time.time > LastFpsTime + 0.5f) {
				UpdateFPS = 1f / Time.deltaTime;
				LastFpsTime = Time.time;
			}
		}


		private void FixedUpdate () {

			// State
			State.text =
				$"update: {UpdateFPS:0}\n" +
				$"fixed: {FixedFPS:0}\n" +
				$"res.width: {Screen.currentResolution.width}\n" +
				$"res.height: {Screen.currentResolution.height}\n" +
				$"scr.width: {Screen.width}\n" +
				$"scr.height: {Screen.height}\n";

			// FPS
			if (Time.time > LastFpsTimeFixed + 0.5f) {
				FixedFPS = 1f / Time.deltaTime;
				LastFpsTimeFixed = Time.time;
			}

			// Cell
			if (RandomRenderer) {
				for (int i = 0; i < 6; i++) {
					for (int j = 0; j < 512; j++) {
						CellRenderer.FocusLayer(i);
						CellRenderer.SetCell(
							j, Random.Range(0, 4),
							Random.Range(0, (CellRenderer.CellWidth - 1) * CellRenderer.UNIT_MULT),
							Random.Range(0, (CellRenderer.CellHeight - 1) * CellRenderer.UNIT_MULT),
							Random.Range(800, 1200),
							Random.Range(-15, 15),
							Random.ColorHSV(0, 1, 0.8f, 1f, 0.8f, 1f)
						);
					}
				}
				if (!KeepRandom) {
					RandomRenderer = false;
				}
			}

		}


	}
}
