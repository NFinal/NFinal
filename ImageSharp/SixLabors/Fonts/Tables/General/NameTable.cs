﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SixLabors.Fonts.Tables.General.Name;
using SixLabors.Fonts.Utilities;
using SixLabors.Fonts.WellKnownIds;

namespace SixLabors.Fonts.Tables.General
{
    [TableName(TableName)]
    internal class NameTable : Table
    {
        private const string TableName = "name";
        private NameRecord[] names;

        public static NameTable Load(FontReader reader)
        {
            using (var r = reader.GetReaderAtTablePosition(TableName))
            {
                // move to start of table
                return Load(r);
            }
        }

        public static NameTable Load(BinaryReader reader)
        {
            List<StringLoader> strings = new List<StringLoader>();
            ushort format = reader.ReadUInt16();
            ushort nameCount = reader.ReadUInt16();
            ushort stringOffset = reader.ReadUInt16();

            NameRecord[] names = new NameRecord[nameCount];

            for (var i = 0; i < nameCount; i++)
            {
                names[i] = NameRecord.Read(reader);
                strings.Add(names[i].StringReader);
            }

            StringLoader[] langs = null;
            if (format == 1)
            {
                // format 1 adds language data
                ushort langCount = reader.ReadUInt16();
                langs = new StringLoader[langCount];

                for (var i = 0; i < langCount; i++)
                {
                    langs[i] = StringLoader.Create(reader);
                    strings.Add(langs[i]);
                }
            }

            foreach (var readable in strings.OrderBy(x => x.Offset))
            {
                var diff = stringOffset + readable.Offset;

                // only seek forward, if we find issues with this we will consume forwards as the idea is we will never need to backtrack
                reader.Seek(diff, SeekOrigin.Begin);

                readable.LoadValue(reader);
            }

            var langNames = langs?.Select(x => x.Value).ToArray() ?? new string[0];

            return new NameTable(names, langNames);
        }

        internal NameTable(NameRecord[] names, string[] languages)
        {
            this.names = names;
            this.FontFamilyName = GetNameById(NameIds.FontFamilyName);
            this.FontSubFamilyName = GetNameById(NameIds.FontSubfamilyName);
            this.Id = GetNameById(NameIds.UniqueFontID);
            this.FontName = GetNameById(NameIds.FullFontName);
        }

        /// <summary>
        /// Gets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public string Id { get; private set; }

        /// <summary>
        /// Gets the name of the font.
        /// </summary>
        /// <value>
        /// The name of the font.
        /// </value>
        public string FontName { get; private set; }

        /// <summary>
        /// Gets the name of the font family.
        /// </summary>
        /// <value>
        /// The name of the font family.
        /// </value>
        public string FontFamilyName { get; private set; }

        /// <summary>
        /// Gets the name of the font sub family.
        /// </summary>
        /// <value>
        /// The name of the font sub family.
        /// </value>
        public string FontSubFamilyName { get; private set; }

        public string GetNameById(NameIds nameId)
        {
            foreach (var name in this.names)
            {
                if (name.Platform == PlatformIDs.Windows && name.LanguageID == 0409)
                {
                    if (name.NameID == nameId)
                    {
                        return name.Value;
                    }
                }
            }

            foreach (var name in this.names)
            {
                if (name.Platform == PlatformIDs.Windows)
                {
                    if (name.NameID == nameId)
                    {
                        return name.Value;
                    }
                }
            }

            foreach (var name in this.names)
            {
                if (name.NameID == nameId)
                {
                    return name.Value;
                }
            }

            return null;
        }

        public string GetNameById(ushort nameId)
        {
            return this.GetNameById((NameIds)nameId);
        }
    }
}
