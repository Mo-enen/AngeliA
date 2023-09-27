namespace AngeliaFramework.Editor {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEditor;


	public class AseCore {

		


		#region --- VAR ---


		// Data
		private readonly AseData Data = null;


		// Config
		public Vector2 UserPivot = Vector2.one * 0.5f;
		public string AseName = "";
		public string IgnoreLayerTag = "";


		#endregion




		#region --- API ---


		public AseCore (AseData data) => Data = data;


		public TaskResult[] CreateResults () {
			if (Data == null) { return null; }
			var result = CreateResultForTargetLayer(-1, 0);
			if (result == null) { return null; }
			result.TaskLayerIndex = 0;
			result.TaskLayerCount = 1;
			return new TaskResult[1] { result };
		}


		#endregion




		#region --- LGC ---


		private TaskResult CreateResultForTargetLayer (int targetLayerIndex, int targetFrame) {

			// Check
			if (Data == null) { return null; }

			var result = new TaskResult();

			// Layer Check
			int layerCount = Data.GetLayerCount(false);
			bool taskForAllLayers = targetLayerIndex < 0;
			var layers = Data.GetAllChunks<AseData.LayerChunk>();
			if (targetLayerIndex >= 0 && targetLayerIndex < layerCount) {
				var layerChunk = layers[targetLayerIndex];
				result.LayerName = layers != null ? layerChunk.Name : "";
				if (layerChunk.Type == 1) return null;
				if (layerChunk.CheckFlag(AseData.LayerChunk.LayerFlag.Background)) return null;
				if (!layerChunk.CheckFlag(AseData.LayerChunk.LayerFlag.Visible)) return null;
			}

			// Get Cells
			var cells = Data.GetCells(
				layers, layerCount, taskForAllLayers ? -1 : targetLayerIndex, IgnoreLayerTag
			);

			// Get Frame Results
			result.Frames = new TaskResult.FrameResult[targetFrame >= 0 ? 1 : Data.FrameDatas.Count];
			for (int i = 0; i < result.Frames.Length; i++) {
				var fData = Data.FrameDatas[i];
				if (!fData.AllCellsLinked()) {
					result.Frames[i] = GetFrameResult(cells, targetFrame < 0 ? i : targetFrame);
				} else {
					result.Frames[i] = TaskResult.FrameResult.GetEmpty(i);
				}
			}

			return result;
		}


		private TaskResult.FrameResult GetFrameResult (AseData.CelChunk[,] cells, int frameIndex) {

			int width = Data.Header.Width;
			int height = Data.Header.Height;
			if (width <= 0 || height <= 0) {
				return TaskResult.FrameResult.GetEmpty(frameIndex);
			}
			ushort colorDepth = Data.Header.ColorDepth;
			var palette = colorDepth == 8 ? Data.GetPalette32() : null;
			var layerChunks = Data.GetAllChunks<AseData.LayerChunk>();
			var pixels = Data.GetAllPixels(
				cells, frameIndex, true, true, palette, layerChunks
			);

			// Sprites
			var sprites = new List<SpriteMetaData>();
			Data.ForAllChunks<AseData.SliceChunk>((chunk, fIndex, cIndex) => {
				AseData.SliceChunk.SliceData sData = null;
				for (int i = 0; i < chunk.Slices.Length; i++) {
					var d = chunk.Slices[i];
					if (sData == null || frameIndex >= d.FrameIndex) {
						sData = d;
					} else if (frameIndex < d.FrameIndex) {
						break;
					}
				}
				if (sData != null) {
					// Rect
					var rect = new Rect(
						sData.X,
						height - sData.Y - sData.Height,
						sData.Width,
						sData.Height
					);
					// Add into Sprites
					sprites.Add(new SpriteMetaData() {
						name = chunk.Name,
						rect = rect,
						border = chunk.CheckFlag(AseData.SliceChunk.SliceFlag.NinePatches) ? new Vector4(
							sData.CenterX,
							sData.Height - sData.CenterY - sData.CenterHeight,
							sData.Width - sData.CenterX - sData.CenterWidth,
							sData.CenterY
						) : Vector4.zero,
						pivot = chunk.CheckFlag(AseData.SliceChunk.SliceFlag.HasPivot) ? new Vector2(
							(float)sData.PivotX / sData.Width,
							1f - (float)sData.PivotY / sData.Height
						) : UserPivot,
						alignment = 9,
					});
				}
			});

			// Final
			return new TaskResult.FrameResult() {
				FrameIndex = frameIndex,
				Pixels = pixels,
				Sprites = sprites.ToArray(),
				Width = width,
				Height = height,
			};
		}


		#endregion




	}
}