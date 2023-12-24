using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;


namespace AngeliaForUnity.Editor {
	public static class ZlibUtil {


		public static byte[] DeCompressZLib (byte[] sourceByte) {
			byte[] outputBytes = new byte[0];
			try {
				using var inputStream = new MemoryStream(sourceByte);
				using var outputStream = DeCompressStream(inputStream);
				outputBytes = new byte[outputStream.Length];
				outputStream.Position = 0;
				outputStream.Read(outputBytes, 0, outputBytes.Length);
			} catch { }
			return outputBytes;
		}


		public static byte[] CompressZLib (byte[] sourceByte) {
			byte[] outPutByteArray = new byte[0];
			try {
				using var inputStream = new MemoryStream(sourceByte);
				using var outStream = CompressStream(inputStream);
				outPutByteArray = new byte[outStream.Length];
				outStream.Position = 0;
				outStream.Read(outPutByteArray, 0, outPutByteArray.Length);
			} catch { }
			return outPutByteArray;
		}


		private static void CopyStream (Stream input, Stream output) {
			byte[] buffer = new byte[2000];
			int len;
			while ((len = input.Read(buffer, 0, 2000)) > 0) {
				output.Write(buffer, 0, len);
			}
			output.Flush();
		}


		private static Stream CompressStream (Stream sourceStream) {
			MemoryStream streamOut = new();
			var streamZOut = new zlib.ZOutputStream(streamOut, zlib.zlibConst.Z_DEFAULT_COMPRESSION);
			CopyStream(sourceStream, streamZOut);
			streamZOut.finish();
			return streamOut;
		}


		private static Stream DeCompressStream (Stream sourceStream) {
			MemoryStream outStream = new();
			var outZStream = new zlib.ZOutputStream(outStream);
			CopyStream(sourceStream, outZStream);
			outZStream.finish();
			return outStream;
		}



	}
}