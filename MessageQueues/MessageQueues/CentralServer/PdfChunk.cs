using System.Collections.Generic;

namespace CentralServer
{
	public class PdfChunk
	{
		public int Pozition { get; set; }

		public int Size { get; set; }

		public List<byte> Buffer { get; set; }

		public int BufferSize { get; set; }
	}
}
