using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {
	public class TextureSquad {




		#region --- SUB ---


		private class TextureUnit {
			public Texture2D Texture;
			public Vector3Int WorldPos;
		}


		#endregion




		#region --- VAR ---


		// Api
		public readonly int WorldSize = 13;
		public int GlobalScale = 1000;

		// Data
		private readonly TextureUnit[,] Textures = null;
		private readonly Texture2D[,] TextureBuffer = null;
		private readonly Texture2D[] TextureBufferAlt = null;
		private readonly Material[] Materials = null;
		private readonly World FillingWorld = null;
		private readonly Transform SquadRoot = null;
		private readonly Transform SquadContainer = null;
		private Vector3Int LoadedWorldPos = default;


		#endregion




		#region --- MSG ---


		public TextureSquad (int worldSize) {
			WorldSize = worldSize;
			LoadedWorldPos = new(int.MinValue, int.MinValue, int.MinValue);
			Textures = new TextureUnit[worldSize, worldSize];
			TextureBuffer = new Texture2D[worldSize, worldSize];
			TextureBufferAlt = new Texture2D[worldSize * worldSize];
			for (int i = 0; i < worldSize; i++) {
				for (int j = 0; j < worldSize; j++) {
					Textures[i, j] = new TextureUnit() {
						Texture = new Texture2D(Const.MAP, Const.MAP, TextureFormat.ARGB32, false) {
							name = "",
							filterMode = FilterMode.Point,
							wrapMode = TextureWrapMode.Clamp,
						},
						WorldPos = new Vector3Int(int.MaxValue, int.MaxValue, int.MaxValue),
					};
				}
			}
			FillingWorld = new World();
			// Material
			var shader = Shader.Find("Angelia/Cell");
			Materials = new Material[worldSize * worldSize];
			for (int i = 0; i < Materials.Length; i++) {
				Materials[i] = new Material(shader);
			}
			// Add GameObject
			SquadRoot = new GameObject("Texture Squad").transform;
			SquadRoot.SetParent(Game.GameCamera.transform);
			SquadRoot.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
			SquadRoot.localScale = Vector3.one;
			SquadContainer = new GameObject("Container").transform;
			SquadContainer.SetParent(SquadRoot);
			SquadContainer.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
			SquadContainer.localScale = Vector3.one;

			var mesh = new Mesh();
			var vertices = new Vector3[4] { new(0, 0), new(0, 1), new(1, 1), new(1, 0), };
			var uvs = new Vector2[4] { new(0, 0), new(0, 1), new(1, 1), new(1, 0), };
			var tris = new int[6] { 0, 1, 2, 0, 2, 3, };
			var colors = new Color32[4] { Const.WHITE, Const.WHITE, Const.WHITE, Const.WHITE, };
			mesh.SetVertices(vertices);
			mesh.SetTriangles(tris, 0);
			mesh.SetUVs(0, uvs);
			mesh.SetColors(colors);
			mesh.RecalculateBounds();
			mesh.RecalculateNormals();
			mesh.UploadMeshData(true);
			for (int j = 0; j < worldSize; j++) {
				for (int i = 0; i < worldSize; i++) {
					var renderer = new GameObject("", typeof(MeshFilter), typeof(MeshRenderer)).transform;
					renderer.SetParent(SquadContainer);
					renderer.SetLocalPositionAndRotation(
						new Vector3(i - worldSize / 2f - 0.5f, j - worldSize / 2f - 0.5f, 1f),
						Quaternion.identity
					);
					renderer.localScale = Vector3.one;
					renderer.SetAsLastSibling();
					var mr = renderer.GetComponent<MeshRenderer>();
					mr.receiveShadows = false;
					mr.staticShadowCaster = false;
					mr.allowOcclusionWhenDynamic = false;
					mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
					mr.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
					mr.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
					mr.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
					mr.sortingOrder = 0;
					mr.material = Materials[j * worldSize + i];
					var mf = renderer.GetComponent<MeshFilter>();
					mf.sharedMesh = mesh;
				}
			}
			// Final
			Disable();
		}


		public void Enable () {
			for (int i = 0; i < WorldSize; i++) {
				for (int j = 0; j < WorldSize; j++) {
					Textures[i, j].WorldPos.x = int.MaxValue;
					Textures[i, j].WorldPos.y = int.MaxValue;
					Textures[i, j].WorldPos.z = int.MaxValue;
				}
			}
			LoadedWorldPos.x = int.MinValue;
			SquadRoot.gameObject.SetActive(true);
			SquadRoot.localPosition = new Vector3(0f, 0f, 1f);
			SquadRoot.localScale = Vector3.one;
			SquadContainer.localPosition = Vector3.zero;
			SquadContainer.localScale = Vector3.one;
			int len = SquadContainer.childCount;
			for (int i = 0; i < len; i++) {
				SquadContainer.GetChild(i).gameObject.SetActive(false);
			}
		}


		public void Disable () {
			SquadRoot.gameObject.SetActive(false);
		}


		public void Dispose () {
			Object.DestroyImmediate(SquadRoot.gameObject, false);
		}


		public void FrameUpdate (Vector3Int centerPos) {

			int GLOBAL_MAP = Const.MAP * Const.CEL;
			int HALF_GLOBAL_MAP = Const.MAP * Const.HALF;

			var worldPos = new Vector3Int(
				(centerPos.x - WorldSize * GLOBAL_MAP / 2).UDivide(GLOBAL_MAP),
				(centerPos.y - WorldSize * GLOBAL_MAP / 2).UDivide(GLOBAL_MAP),
				centerPos.z
			);

			// Update Textures
			if (LoadedWorldPos != worldPos) {
				ReloadTextures(worldPos);
			}

			// Update Renderer
			float scale = Game.GameCamera.orthographicSize * 2f / (WorldSize - 1);
			float posShiftX = -((centerPos.x + HALF_GLOBAL_MAP).UMod(GLOBAL_MAP) - GLOBAL_MAP) / (float)GLOBAL_MAP * scale;
			float posShiftY = -((centerPos.y + HALF_GLOBAL_MAP).UMod(GLOBAL_MAP) - GLOBAL_MAP) / (float)GLOBAL_MAP * scale;
			SquadContainer.localScale = Vector3.one * scale;
			SquadContainer.localPosition = new Vector3(posShiftX, posShiftY, 1f);
			SquadRoot.localScale = Vector3.one * (GlobalScale / 1000f);
		}


		#endregion




		#region --- LGC ---


		private void ReloadTextures (Vector3Int newWorldPos) {

			// Clear Buffer
			for (int i = 0; i < WorldSize; i++) {
				for (int j = 0; j < WorldSize; j++) {
					TextureBuffer[i, j] = null;
					TextureBufferAlt[i * WorldSize + j] = null;
				}
			}

			if (newWorldPos.z == LoadedWorldPos.z) {

				// Data >> Buffer
				int alt = 0;
				for (int i = 0; i < WorldSize; i++) {
					for (int j = 0; j < WorldSize; j++) {
						var unit = Textures[i, j];
						int localX = unit.WorldPos.x - newWorldPos.x;
						int localY = unit.WorldPos.y - newWorldPos.y;
						if (localX >= 0 && localX < WorldSize && localY >= 0 && localY < WorldSize) {
							TextureBuffer[localX, localY] = unit.Texture;
						} else {
							TextureBufferAlt[alt] = unit.Texture;
							alt++;
						}
					}
				}

				// Buffer >> Data
				alt = 0;
				for (int i = 0; i < WorldSize; i++) {
					for (int j = 0; j < WorldSize; j++) {
						var unit = Textures[i, j];
						var buffer = TextureBuffer[i, j];
						if (buffer != null) {
							unit.Texture = buffer;
						} else {
							unit.Texture = TextureBufferAlt[alt];
							alt++;
						}
					}
				}

				// Clear Buffer
				for (int i = 0; i < WorldSize; i++) {
					for (int j = 0; j < WorldSize; j++) {
						TextureBuffer[i, j] = null;
						TextureBufferAlt[i * WorldSize + j] = null;
					}
				}

			}

			// Load
			var pos = newWorldPos;
			for (int i = 0; i < WorldSize; i++) {
				for (int j = 0; j < WorldSize; j++) {
					var unit = Textures[i, j];
					pos.x = newWorldPos.x + i;
					pos.y = newWorldPos.y + j;
					var renderer = SquadContainer.GetChild(j * WorldSize + i);
					renderer.gameObject.SetActive(true);
					if (unit.WorldPos == pos) continue;
					if (FillingWorld.LoadFromDisk(WorldSquad.MapRoot, pos.x, pos.y, pos.z)) {
						FillingWorld.FillIntoTexture(unit.Texture, ignoreItem: true);
					} else {
						renderer.gameObject.SetActive(false);
					}
					unit.WorldPos = pos;
				}
			}

			// Material
			for (int i = 0; i < Materials.Length; i++) {
				Materials[i].mainTexture = Textures[i % WorldSize, i / WorldSize].Texture;
			}
			// Final
			LoadedWorldPos = newWorldPos;
		}


		#endregion




	}
}