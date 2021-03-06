﻿using System;
using System.Drawing.Drawing2D;
using JetBrains.Annotations;

// ReSharper disable ClassWithVirtualMembersNeverInherited.Global

namespace Svg.Contrib.Render.FingerPrint
{
  [PublicAPI]
  public class SvgLineTranslator : SvgElementTranslatorBase<FingerPrintContainer, SvgLine>
  {
    /// <exception cref="ArgumentNullException"><paramref name="fingerPrintTransformer" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="fingerPrintCommands" /> is <see langword="null" />.</exception>
    public SvgLineTranslator([NotNull] FingerPrintTransformer fingerPrintTransformer,
                             [NotNull] FingerPrintCommands fingerPrintCommands)
    {
      if (fingerPrintTransformer == null)
      {
        throw new ArgumentNullException(nameof(fingerPrintTransformer));
      }
      if (fingerPrintCommands == null)
      {
        throw new ArgumentNullException(nameof(fingerPrintCommands));
      }
      this.FingerPrintTransformer = fingerPrintTransformer;
      this.FingerPrintCommands = fingerPrintCommands;
    }

    [NotNull]
    protected FingerPrintCommands FingerPrintCommands { get; }

    [NotNull]
    protected FingerPrintTransformer FingerPrintTransformer { get; }

    /// <exception cref="ArgumentNullException"><paramref name="svgLine" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="sourceMatrix" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="viewMatrix" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="fingerPrintContainer" /> is <see langword="null" />.</exception>
    public override void Translate([NotNull] SvgLine svgLine,
                                   [NotNull] Matrix sourceMatrix,
                                   [NotNull] Matrix viewMatrix,
                                   [NotNull] FingerPrintContainer fingerPrintContainer)
    {
      if (svgLine == null)
      {
        throw new ArgumentNullException(nameof(svgLine));
      }
      if (sourceMatrix == null)
      {
        throw new ArgumentNullException(nameof(sourceMatrix));
      }
      if (viewMatrix == null)
      {
        throw new ArgumentNullException(nameof(viewMatrix));
      }
      if (fingerPrintContainer == null)
      {
        throw new ArgumentNullException(nameof(fingerPrintContainer));
      }

      int horizontalStart;
      int verticalStart;
      int length;
      int lineWeight;
      int verticalEnd;
      float strokeWidth;
      this.GetPosition(svgLine,
                       sourceMatrix,
                       viewMatrix,
                       out horizontalStart,
                       out verticalStart,
                       out length,
                       out lineWeight,
                       out verticalEnd,
                       out strokeWidth);

      this.AddTranslationToContainer(svgLine,
                                     horizontalStart,
                                     verticalStart,
                                     verticalEnd,
                                     length,
                                     lineWeight,
                                     strokeWidth,
                                     fingerPrintContainer);
    }

    /// <exception cref="ArgumentNullException"><paramref name="svgLine" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="sourceMatrix" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="viewMatrix" /> is <see langword="null" />.</exception>
    [Pure]
    protected virtual void GetPosition([NotNull] SvgLine svgLine,
                                       [NotNull] Matrix sourceMatrix,
                                       [NotNull] Matrix viewMatrix,
                                       out int horizontalStart,
                                       out int verticalStart,
                                       out int horizontalLength,
                                       out int verticalLength,
                                       out int verticalEnd,
                                       out float strokeWidth)
    {
      if (svgLine == null)
      {
        throw new ArgumentNullException(nameof(svgLine));
      }
      if (sourceMatrix == null)
      {
        throw new ArgumentNullException(nameof(sourceMatrix));
      }
      if (viewMatrix == null)
      {
        throw new ArgumentNullException(nameof(viewMatrix));
      }

      float startX;
      float startY;
      float endX;
      float endY;
      this.FingerPrintTransformer.Transform(svgLine,
                                            sourceMatrix,
                                            viewMatrix,
                                            out startX,
                                            out startY,
                                            out endX,
                                            out endY,
                                            out strokeWidth);

      // TODO find a good TOLERANCE
      if (Math.Abs(startY - endY) < 0.5f
          || Math.Abs(startX - endX) < 0.5f)
      {
        horizontalStart = (int) startX;
        verticalStart = (int) startY;
        horizontalLength = (int) (endX - startX);
        verticalLength = (int) (endY - startY);
        verticalEnd = (int) endY;
      }
      else
      {
        throw new NotImplementedException();
        horizontalStart = (int) startX;
        verticalStart = (int) startY;
        horizontalLength = (int) strokeWidth;
        verticalLength = (int) endX;
        verticalEnd = (int) endY;
      }
    }

    /// <exception cref="ArgumentNullException"><paramref name="svgLine" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="fingerPrintContainer" /> is <see langword="null" />.</exception>
    protected virtual void AddTranslationToContainer([NotNull] SvgLine svgLine,
                                                     int horizontalStart,
                                                     int verticalStart,
                                                     int verticalEnd,
                                                     int length,
                                                     int lineWeight,
                                                     float strokeWidth,
                                                     [NotNull] FingerPrintContainer fingerPrintContainer)
    {
      if (svgLine == null)
      {
        throw new ArgumentNullException(nameof(svgLine));
      }
      if (fingerPrintContainer == null)
      {
        throw new ArgumentNullException(nameof(fingerPrintContainer));
      }

      if (length == 0
          || lineWeight == 0)
      {
        if (length == 0)
        {
          length = (int) strokeWidth;
        }
        if (lineWeight == 0)
        {
          lineWeight = (int) strokeWidth;
        }

        fingerPrintContainer.Body.Add(this.FingerPrintCommands.Position(horizontalStart,
                                                                        verticalStart));
        fingerPrintContainer.Body.Add(this.FingerPrintCommands.Direction(Direction.LeftToRight));
        fingerPrintContainer.Body.Add(this.FingerPrintCommands.Align(Alignment.TopLeft));
        fingerPrintContainer.Body.Add(this.FingerPrintCommands.Line(length,
                                                                    lineWeight));
      }
      else
      {
        throw new NotImplementedException();
      }
    }
  }
}
