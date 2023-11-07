using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using AngeliaFramework;

namespace System.Runtime.CompilerServices { internal static class IsExternalInit { } }


namespace HololiaGame {
	public static class Hololia {



		static readonly (string id, string en, string cn)[] NAMES = {
			("Sora",        "Sora",        "时乃空"),
			("Roboco",      "Roboco",      "萝卜子"),
			("Miko",        "Miko",        "樱巫女"),
			("Suisei",      "Suisei",      "彗星"),
			("AZKi",        "AZKi",        "AZKi"),
			("Mel",         "Mel",         "梅露"),
			("Fubuki",      "Fubuki",      "吹雪"),
			("Matsuri",     "Matsuri",     "夏色祭"),
			("Aki",         "Aki",         "亚绮"),
			("Akai",        "Akai",        "赤井心"),
			("Aqua",        "Aqua",        "阿库娅"),
			("Shion",       "Shion",       "诗音"),
			("Ayame",       "Ayame",       "百鬼绫目"),
			("Choco",       "Choco",       "巧可"),
			("Subaru",      "Subaru",      "大空昴"),
			("Mio",         "Mio",         "大神澪"),
			("Okayu",       "Okayu",       "小粥"),
			("Korone",      "Korone",      "沁音"),
			("Pekora",      "Pekora",      "佩克拉"),
			("Flare",       "Flare",       "芙蕾雅"),
			("Noel",        "Noel",        "诺艾尔"),
			("Marine",      "Marine",      "玛琳"),
			("Kanata",      "Kanata",      "彼方"),
			("Watame",      "Watame",      "绵芽"),
			("Towa",        "Towa",        "常暗"),
			("Luna",        "Luna",        "璐娜"),
			("Coco",        "Coco",        "可可"),
			("Lamy",        "Lamy",        "菈米"),
			("Nene",        "Nene",        "桃铃音音"),
			("Botan",       "Botan",       "狮白"),
			("Polka",       "Polka",       "尾丸"),
			("Risu",        "Risu",        "栗鼠"),
			("Moona",       "Moona",       "穆娜"),
			("Iofifteen",   "iofifteen",   "iofifteen"),
			("Ollie",       "Ollie",       "奥莉"),
			("Melfissa",    "Melfissa",    "梅尔菲莎"),
			("Reine",       "Reine",       "蕾内"),
			("Calliope",    "Calliope",    "森美声"),
			("Kiara",       "Kiara",       "琪亚拉"),
			("Ina",         "Ina'nis",     "一伊那尓栖"),
			("InaCasual",   "Casual Ina",  "日常伊娜"),
			("InaArtist",   "Artist Ina",  "画家伊娜"),
			("Gura",        "Gura",        "古拉"),
			("Ame",         "Amelia",      "华生"),
			("IRyS",        "IRyS",        "IRyS"),
			("Fauna",       "Fauna",       "法娜"),
			("Kronii",      "Kronii",      "克洛尼"),
			("Mumei",       "Mumei",       "七诗无名"),
			("Bae",         "Bae",         "贝尔丝"),
			("Sana",        "Sana",        "九十九佐命"),
			("Laplus",      "Laplus",      "拉普"),
			("Lui",         "Lui",         "琉衣"),
			("Koyori",      "Koyori",      "小夜璃"),
			("Sakamata",    "Sakamata",    "沙花叉"),
			("Iroha",       "Iroha",       "风真"),
			("Zeta",        "Zeta",        "泽塔"),
			("Kaela",       "Kaela",       "Kaela"),
			("Kobo",        "Kobo",        "Kobo"),
			("Shiori",      "Shiori",      "Shiori"),
			("Bijou",       "Bijou",       "Bijou"),
			("Nerissa",     "Nerissa",     "Nerissa"),
			("Fuwa",        "Fuwawa",      "Fuwawa"),
			("Moco",        "Mococo",      "Mococo"),
			("FriendA",     "Friend A",    "友人A"),
			("Nodoka",      "Nodoka",      "春先和花"),
			("Ui",          "Shigure Ui",  "羽衣"),
		};
		static readonly (string id, string en, string cn)[] MORE_NAMES = {
			("Chris","Chris", "酷丽丝"),
			("Rushia","Rushia", "露西娅"),
			("Aloe","Aloe", "阿萝耶"),
		};


		//[UnityEditor.InitializeOnLoadMethod]
		public static void Test () {
			string result = "";
			for (int i = 0; i < NAMES.Length; i++) {
				var (id, en, cn) = NAMES[i];


				result += $"Pat.{id}:{en}\n";

			}

			result += "\n\n\n\n";

			for (int i = 0; i < NAMES.Length; i++) {
				var (id, en, cn) = NAMES[i];

				result += $"Pat.{id}:{cn}\n";

			}

			GUIUtility.systemCopyBuffer = result;
			Debug.Log(Random.value + " Copy.");
		}



	}
}
