﻿using System;
using System.Drawing.Drawing2D;
using System.Linq;
using JetBrains.Annotations;
using Svg.Pathing;

// ReSharper disable ClassWithVirtualMembersNeverInherited.Global

namespace Svg.Contrib.Render.FingerPrint
{
  [PublicAPI]
  public class SvgPathTranslator : SvgElementTranslatorBase<FingerPrintContainer, SvgPath>
  {
    /// <exception cref="ArgumentNullException"><paramref name="fingerPrintTransformer" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="fingerPrintCommands" /> is <see langword="null" />.</exception>
    public SvgPathTranslator([NotNull] FingerPrintTransformer fingerPrintTransformer,
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
    protected FingerPrintTransformer FingerPrintTransformer { get; }

    [NotNull]
    protected FingerPrintCommands FingerPrintCommands { get; }

    /// <exception cref="ArgumentNullException"><paramref name="svgPath" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="sourceMatrix" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="viewMatrix" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="fingerPrintContainer" /> is <see langword="null" />.</exception>
    public override void Translate([NotNull] SvgPath svgPath,
                                   [NotNull] Matrix sourceMatrix,
                                   [NotNull] Matrix viewMatrix,
                                   [NotNull] FingerPrintContainer fingerPrintContainer)
    {
      if (svgPath == null)
      {
        throw new ArgumentNullException(nameof(svgPath));
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

      // TODO translate C (curveto)
      // TODO translate S (smooth curveto)
      // TODO translate Q (quadratic bézier curve)
      // TODO translate T (smooth bézier curve)
      // TODO translate A (elliptical arc)
      // TODO translate Z (closepath)
      // TODO add test cases

      if (svgPath.PathData == null)
      {
        return;
      }

      // ReSharper disable ExceptionNotDocumentedOptional
      foreach (var svgLineSegment in svgPath.PathData.OfType<SvgLineSegment>())
        // ReSharper restore ExceptionNotDocumentedOptional
      {
        this.TranslateSvgLineSegment(svgPath,
                                     svgLineSegment,
                                     sourceMatrix,
                                     viewMatrix,
                                     fingerPrintContainer);
      }
    }

    /// <exception cref="ArgumentNullException"><paramref name="svgPath" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="svgLineSegment" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="sourceMatrix" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="viewMatrix" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="fingerPrintContainer" /> is <see langword="null" />.</exception>
    protected virtual void TranslateSvgLineSegment([NotNull] SvgPath svgPath,
                                                   [NotNull] SvgLineSegment svgLineSegment,
                                                   [NotNull] Matrix sourceMatrix,
                                                   [NotNull] Matrix viewMatrix,
                                                   [NotNull] FingerPrintContainer fingerPrintContainer)
    {
      if (svgPath == null)
      {
        throw new ArgumentNullException(nameof(svgPath));
      }
      if (svgLineSegment == null)
      {
        throw new ArgumentNullException(nameof(svgLineSegment));
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

      var svgLine = new SvgLine
                    {
                      Color = svgPath.Color,
                      Stroke = svgPath.Stroke,
                      StrokeWidth = svgPath.StrokeWidth,
                      StartX = svgLineSegment.Start.X,
                      StartY = svgLineSegment.Start.Y,
                      EndX = svgLineSegment.End.X,
                      EndY = svgLineSegment.End.Y
                    };

      float startX;
      float startY;
      float endX;
      float endY;
      float strokeWidth;
      this.FingerPrintTransformer.Transform(svgLine,
                                            sourceMatrix,
                                            viewMatrix,
                                            out startX,
                                            out startY,
                                            out endX,
                                            out endY,
                                            out strokeWidth);

      var horizontalStart = (int) startX;
      var verticalStart = (int) startY;
      var length = (int) (endX - startX);
      if (length == 0)
      {
        length = (int) strokeWidth;
      }

      var lineWeight = (int) (endY - startY);
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
  }
}
