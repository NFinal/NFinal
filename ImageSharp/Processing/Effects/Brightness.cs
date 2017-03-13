﻿// <copyright file="Brightness.cs" company="James Jackson-South">
// Copyright (c) James Jackson-South and contributors.
// Licensed under the Apache License, Version 2.0.
// </copyright>

namespace ImageSharp
{
    using System;

    using Processing.Processors;

    /// <summary>
    /// Extension methods for the <see cref="Image{TColor}"/> type.
    /// </summary>
    public static partial class ImageExtensions
    {
        /// <summary>
        /// Alters the brightness component of the image.
        /// </summary>
        /// <typeparam name="TColor">The pixel format.</typeparam>
                /// <param name="source">The image this method extends.</param>
        /// <param name="amount">The new brightness of the image. Must be between -100 and 100.</param>
        /// <returns>The <see cref="Image{TColor}"/>.</returns>
        public static Image<TColor> Brightness<TColor>(this Image<TColor> source, int amount)
            where TColor : struct, IPixel<TColor>
                    {
            return Brightness(source, amount, source.Bounds);
        }

        /// <summary>
        /// Alters the brightness component of the image.
        /// </summary>
        /// <typeparam name="TColor">The pixel format.</typeparam>
                /// <param name="source">The image this method extends.</param>
        /// <param name="amount">The new brightness of the image. Must be between -100 and 100.</param>
        /// <param name="rectangle">
        /// The <see cref="Rectangle"/> structure that specifies the portion of the image object to alter.
        /// </param>
        /// <returns>The <see cref="Image{TColor}"/>.</returns>
        public static Image<TColor> Brightness<TColor>(this Image<TColor> source, int amount, Rectangle rectangle)
            where TColor : struct, IPixel<TColor>
        {
            source.ApplyProcessor(new BrightnessProcessor<TColor>(amount), rectangle);
            return source;
        }
    }
}
