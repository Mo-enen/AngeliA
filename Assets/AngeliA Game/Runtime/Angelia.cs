using System.Collections;
using System.Collections.Generic;
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
			string[] FACE = {
				"{0}Face.Face.Normal, 1, {1}, 7, 5, 3, 3, 0, 4",
				"{0}Face.Face.Blink, 9, {1}, 7, 5, 3, 3, 0, 4",
				"{0}Face.Face.Damage 0, 28, {1}, 10, 8, 3, 3, 3, 4",
				"{0}Face.Face.Damage 1, 39, {1}, 10, 8, 3, 3, 3, 4",
				"{0}Face.Face.PassOut 0, 50, {1}, 11, 7, 3, 3, 2, 4",
				"{0}Face.Face.PassOut 1, 62, {1}, 11, 7, 3, 3, 2, 4",
				"{0}Face.Face.PassOut 2, 74, {1}, 11, 7, 3, 3, 2, 4",
				"{0}Face.Face.PassOut 3, 86, {1}, 11, 7, 3, 3, 2, 4",
				"{0}Face.Face.PassOut 4, 98, {1}, 11, 7, 3, 3, 2, 4",
				"{0}Face.Face.PassOut 5, 110, {1}, 11, 7, 3, 3, 2, 4",
				"{0}Face.Face.PassOut 6, 122, {1}, 11, 7, 3, 3, 2, 4",
				"{0}Face.Face.PassOut 7, 134, {1}, 11, 7, 3, 3, 2, 4",
				"{0}Face.Face.Sleep, 17, {1}, 10, 8, 3, 3, 3, 4",
			};

			string result = "";
			int currentY = 47;
			foreach (string name in ALL_NAMES) {
				foreach (var face in FACE) {
					result += string.Format(face, name, currentY) + "\n";
				}
				currentY += 9;
				result += "\n";
			}

			Util.TextToFile(result, "Assets/Test.txt");
		}



	}
}
