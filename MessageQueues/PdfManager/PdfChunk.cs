using System.Collections.Generic;

namespace PdfManager
{
    public class PdfChunk
    {
        public int Position { get; set; }

        public int Size { get; set; }

        public List<byte> Buffer { get; set; }

        public int BufferSize { get; set; }
    }
}
