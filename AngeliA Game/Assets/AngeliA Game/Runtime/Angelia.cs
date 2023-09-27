using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using AngeliaFramework;



namespace System.Runtime.CompilerServices { internal static class IsExternalInit { } }


namespace AngeliaGame {
	public static class AngeliA {





		//[UnityEditor.InitializeOnLoadMethod]
		public static void Test () {

			string[] ALL_NAMES = {
				"Sora",
				"Roboco",
				"Miko",
				"Suisei",
				"AZKi",
				"Mel",
				"Fubuki",
				"Matsuri",
				"Aki",
				"Akai",
				"Aqua",
				"Shion",
				"Ayame",
				"Choco",
				"Subaru",
				"Mio",
				"Okayu",
				"Korone",
				"Pekora",
				"Flare",
				"Noel",
				"Marine",
				"Kanata",
				"Watame",
				"Towa",
				"Luna",
				"Coco",
				"Lamy",
				"Nene",
				"Botan",
				"Polka",
				"Risu",
				"Moona",
				"Iofifteen",
				"Ollie",
				"Melfissa",
				"Reine",
				"Calliope",
				"Kiara",
				"Ina",
				"Gura",
				"Ame",
				"IRyS",
				"Fauna",
				"Kronii",
				"Mumei",
				"Bae",
				"Sana",
				"Laplus",
				"Lui",
				"Koyori",
				"Sakamata",
				"Iroha",
				"Zeta",
				"Kaela",
				"Kobo",
				"Shiori",
				"Bijou",
				"Nerissa",
				"Fuwa",
				"Moco",
			};
			string[] CN_NAMES = {
				"时乃空",
				"萝卜子",
				"樱巫女",
				"彗星",
				"AZKi",
				"梅露",
				"吹雪",
				"夏色祭",
				"亚绮",
				"赤井心",
				"阿库娅",
				"诗音",
				"百鬼绫目",
				"巧可",
				"大空昴",
				"大神澪",
				"小粥",
				"沁音",
				"佩克拉",
				"芙蕾雅",
				"诺艾尔",
				"玛琳",
				"彼方",
				"绵芽",
				"常暗",
				"璐娜",
				"可可",
				"菈米",
				"桃铃音音",
				"狮白",
				"尾丸",
				"Risu",
				"穆娜",
				"Iofifteen",
				"Ollie",
				"Melfissa",
				"Reine",
				"森美声",
				"琪亚拉",
				"一伊那尓栖",
				"古拉",
				"艾米莉亚",
				"IRyS",
				"Fauna",
				"Kronii",
				"Mumei",
				"Bae",
				"Sana",
				"拉普",
				"琉衣",
				"小夜璃",
				"沙花叉",
				"风真",
				"Zeta",
				"Kaela",
				"Kobo",
				"Shiori",
				"Bijou",
				"Nerissa",
				"Fuwa",
				"Moco",
			};
			string[] ALL_NAMES1 = { "Chris", "Rushia", "Aloe", };



			string result = "";
			int currentX = 20;
			int currentY = 26;
			var names = ALL_NAMES.Concat(ALL_NAMES1).ToArray();
			//var names = ALL_NAMES;
			for (int i = 0; i < names.Length; i++) {
				string name = names[i];

				result += $"{name}HeadSuit, {currentX + 3 - 20}, {currentY - 26 + 20}, 13, 7, 0, 0, 0, 0\n";
				result += $"{name}BodySuit, {currentX + 5 - 20}, {currentY - 26 + 36}, 10, 10, 0, 0, 0, 0\n";
				result += $"{name}HipSuit, {currentX + 5 - 20}, {currentY - 26 + 43}, 10, 10, 0, 0, 0, 0\n";
				result += $"{name}HandSuit, {currentX + 3 - 20}, {currentY - 26 + 46}, 2, 2, 0, 0, 0, 0\n";
				result += $"{name}FootSuit, {currentX + 5 - 20}, {currentY - 26 + 51}, 5, 5, 0, 0, 0, 0\n";

				currentX += 28;
				if (currentX > 450) {
					currentX = 20;
					currentY += 80;
				}
				result += "\n";
			}
			GUIUtility.systemCopyBuffer = result;
			Debug.Log(Random.value + " Copy.");
		}



	}
}
