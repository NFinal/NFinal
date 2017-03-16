﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace SixLabors.Fonts
{
    /// <summary>
    /// Encapulated logic for laying out and then rendering text to a <see cref="IGlyphRenderer"/> surface.
    /// </summary>
    public class TextRenderer
    {
        private TextLayout layoutEngine;
        private IGlyphRenderer renderer;

        internal TextRenderer(IGlyphRenderer renderer, TextLayout layoutEngine)
        {
            this.layoutEngine = layoutEngine;
            this.renderer = renderer;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TextRenderer"/> class.
        /// </summary>
        /// <param name="renderer">The renderer.</param>
        public TextRenderer(IGlyphRenderer renderer)
            : this(renderer, new TextLayout())
        {
        }

        /// <summary>
        /// Renders the text.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="style">The style.</param>
        /// <param name="dpi">The dpi.</param>
        public void RenderText(string text, FontSpan style, Vector2 dpi)
        {
            var glyphsToRender = this.layoutEngine.GenerateLayout(text, style);

            foreach (var g in glyphsToRender)
            {
                g.Glyph.RenderTo(this.renderer, g.Location, dpi);
            }
        }

        /// <summary>
        /// Renders the text.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="style">The style.</param>
        /// <param name="dpi">The dpi.</param>
        public void RenderText(string text, FontSpan style, float dpi)
        {
            this.RenderText(text, style, new Vector2(dpi));
        }
    }
}
