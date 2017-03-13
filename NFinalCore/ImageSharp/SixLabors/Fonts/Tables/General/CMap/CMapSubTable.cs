﻿using SixLabors.Fonts.WellKnownIds;

namespace SixLabors.Fonts.Tables.General.CMap
{
    internal abstract class CMapSubTable
    {
        public CMapSubTable()
        {
        }

        public CMapSubTable(PlatformIDs platform, ushort encoding, ushort format)
        {
            this.Platform = platform;
            this.Encoding = encoding;
            this.Format = format;
        }

        public ushort Format { get; }

        public PlatformIDs Platform { get; }

        public ushort Encoding { get; }

        public abstract ushort GetGlyphId(char character);
    }
}