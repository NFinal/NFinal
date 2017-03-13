﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SixLabors.Fonts.Tables.General.CMap;
using SixLabors.Fonts.Tables.General.Glyphs;
using SixLabors.Fonts.WellKnownIds;

namespace SixLabors.Fonts.Tables.General
{
    [TableName(TableName)]
    internal class GlyphTable : Table
    {
        private const string TableName = "glyf";
        private GlyphLoader[] loaders;

        public int GlyphCount => this.loaders.Length;

        public GlyphTable(GlyphLoader[] glyphLoaders)
        {
            this.loaders = glyphLoaders;
        }

        internal virtual Glyphs.GlyphVector GetGlyph(int index)
        {
            return this.loaders[index].CreateGlyph(this);
        }

        public static GlyphTable Load(FontReader reader)
        {
            var locations = reader.GetTable<IndexLocationTable>().GlyphOffsets;
            using (var binaryReader = reader.GetReaderAtTablePosition(TableName))
            {
                return Load(binaryReader, locations);
            }
        }

        public static GlyphTable Load(BinaryReader reader, uint[] locations)
        {
            var empty = new Glyphs.EmptyGlyphLoader();
            var entryCount = locations.Length;
            var glyphCount = entryCount - 1; // last entry is a placeholder to the end of the table
            Glyphs.GlyphLoader[] glyphs = new Glyphs.GlyphLoader[glyphCount];
            for (var i = 0; i < glyphCount; i++)
            {
                if (locations[i] == locations[i + 1])
                {
                    // this is an empty glyphs;
                    glyphs[i] = empty;
                }
                else
                {
                    // move to start of glyph
                    reader.Seek(locations[i], System.IO.SeekOrigin.Begin);

                    glyphs[i] = GlyphLoader.Load(reader);
                }
            }

            return new GlyphTable(glyphs);
        }
    }
}