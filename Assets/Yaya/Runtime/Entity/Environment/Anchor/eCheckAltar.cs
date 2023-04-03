using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {


	public class AltarLalynnA : eCheckAltar { }
	public class AltarMage : eCheckAltar { }
	public class AltarElf : eCheckAltar { }
	public class AltarDragon : eCheckAltar { }
	public class AltarTorch : eCheckAltar { }
	public class AltarSlime : eCheckAltar { }
	public class AltarInsect : eCheckAltar { }
	public class AltarOrc : eCheckAltar { }
	public class AltarTako : eCheckAltar { }
	public class AltarShark : eCheckAltar { }
	public class AltarBone : eCheckAltar { }
	public class AltarFootman : eCheckAltar { }
	public class AltarKnight : eCheckAltar { }
	public class AltarJesus : eCheckAltar { }
	public class AltarShield : eCheckAltar { }
	public class AltarGamble : eCheckAltar { }
	public class AltarScience : eCheckAltar { }
	public class AltarSpider : eCheckAltar { }
	public class AltarStalactite : eCheckAltar { }
	public class AltarSword : eCheckAltar { }
	public class AltarSpace : eCheckAltar { }
	public class AltarMachineGun : eCheckAltar { }
	public class AltarKnowledge : eCheckAltar { }
	public class AltarCat : eCheckAltar { }





	[EntityAttribute.Bounds(0, 0, Const.CEL, Const.CEL * 2)]
	public abstract class eCheckAltar : eCheckPoint {





		#region --- SUB ---


		[System.Serializable]
		public class CheckPointMeta {
			public Int4[] UnlockedPositions;
		}


		#endregion




		#region --- VAR ---


		// Data
		private static readonly Dictionary<int, Vector3Int> UnlockedPositionPool = new();
		private static MapChannel LoadedChannel = MapChannel.BuiltIn;


		#endregion




		#region --- MSG ---


		[BeforeGameInitialize]
		public static void Initialize () {
			ReloadMetaPool(MapChannel.BuiltIn);
			MapEditor.OnPlayModeChanged += OnMapEditorPlayModeChanged;
		}


		public override void OnActivated () {
			base.OnActivated();
			Height = Const.CEL * 2;
		}


		protected override void OnPlayerTouched (Vector3Int unitPos) {
			base.OnPlayerTouched(unitPos);
			if (Game.Current.WorldSquad.Channel != LoadedChannel) {
				ReloadMetaPool(Game.Current.WorldSquad.Channel);
			}
			// Save Meta
			if (!UnlockedPositionPool.TryGetValue(TypeID, out var savedPos) || savedPos != unitPos) {
				UnlockedPositionPool[TypeID] = unitPos;
				var meta = new CheckPointMeta {
					UnlockedPositions = new Int4[UnlockedPositionPool.Count],
				};
				int index = 0;
				foreach (var (id, pos) in UnlockedPositionPool) {
					ref var pos4 = ref meta.UnlockedPositions[index];
					pos4.A = pos.x;
					pos4.B = pos.y;
					pos4.C = pos.z;
					pos4.D = id;
					index++;
				}
				string mapRoot = LoadedChannel switch {
					MapChannel.BuiltIn => Const.BuiltInMapRoot,
					MapChannel.User => Const.UserMapRoot,
					MapChannel.Download => Const.DownloadMapRoot,
					_ => throw new System.NotImplementedException(),
				};
				AngeUtil.SaveJson(meta, mapRoot);
			}
		}


		#endregion




		#region --- API ---


		public static bool TryGetUnlockedPosition (int id, out Vector3Int unitPosition) {
			if (Game.Current.WorldSquad.Channel != LoadedChannel) {
				ReloadMetaPool(Game.Current.WorldSquad.Channel);
			}
			return UnlockedPositionPool.TryGetValue(id, out unitPosition);
		}


		#endregion




		#region --- LGC ---


		private static void ReloadMetaPool (MapChannel channel) {
			LoadedChannel = channel;
			UnlockedPositionPool.Clear();
			var meta = AngeUtil.LoadOrCreateJson<CheckPointMeta>(channel.MapFolder());
			if (meta == null || meta.UnlockedPositions == null) return;
			foreach (var pos4 in meta.UnlockedPositions) {
				var pos3 = new Vector3Int(pos4.A, pos4.B, pos4.C);
				UnlockedPositionPool.TryAdd(pos4.D, pos3);
			}
		}


		private static void OnMapEditorPlayModeChanged (bool isPlaying) {
#if UNITY_EDITOR
			const MapChannel TARGET_CHANNEL = MapChannel.BuiltIn;
#else
			const MapChannel TARGET_CHANNEL = MapChannel.User;
#endif
			string path = AngeUtil.GetJsonMetaPath<CheckPointMeta>(TARGET_CHANNEL.MapFolder());
			UnlockedPositionPool.Clear();
			Util.DeleteFile(path);
		}


		#endregion




	}
}