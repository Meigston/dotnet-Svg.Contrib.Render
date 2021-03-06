﻿using System;
using System.Drawing.Drawing2D;
using System.Linq;
using JetBrains.Annotations;
using Svg.Pathing;

// ReSharper disable ClassWithVirtualMembersNeverInherited.Global

namespace Svg.Contrib.Render.ZPL
{
  [PublicAPI]
  public class SvgPathTranslator : SvgElementTranslatorBase<ZplContainer, SvgPath>
  {
    /// <exception cref="ArgumentNullException"><paramref name="zplTransformer" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="zplCommands" /> is <see langword="null" />.</exception>
    public SvgPathTranslator([NotNull] ZplTransformer zplTransformer,
                             [NotNull] ZplCommands zplCommands)
    {
      if (zplTransformer == null)
      {
        throw new ArgumentNullException(nameof(zplTransformer));
      }
      if (zplCommands == null)
      {
        throw new ArgumentNullException(nameof(zplCommands));
      }
      this.ZplTransformer = zplTransformer;
      this.ZplCommands = zplCommands;
    }

    [NotNull]
    protected ZplTransformer ZplTransformer { get; }

    [NotNull]
    protected ZplCommands ZplCommands { get; }

    /// <exception cref="ArgumentNullException"><paramref name="svgPath" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="sourceMatrix" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="viewMatrix" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="zplContainer" /> is <see langword="null" />.</exception>
    public override void Translate([NotNull] SvgPath svgPath,
                                   [NotNull] Matrix sourceMatrix,
                                   [NotNull] Matrix viewMatrix,
                                   [NotNull] ZplContainer zplContainer)
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
      if (zplContainer == null)
      {
        throw new ArgumentNullException(nameof(zplContainer));
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
                                     zplContainer);
      }
    }

    /// <exception cref="ArgumentNullException"><paramref name="svgPath" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="svgLineSegment" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="sourceMatrix" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="viewMatrix" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="zplContainer" /> is <see langword="null" />.</exception>
    protected virtual void TranslateSvgLineSegment([NotNull] SvgPath svgPath,
                                                   [NotNull] SvgLineSegment svgLineSegment,
                                                   [NotNull] Matrix sourceMatrix,
                                                   [NotNull] Matrix viewMatrix,
                                                   [NotNull] ZplContainer zplContainer)
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
      if (zplContainer == null)
      {
        throw new ArgumentNullException(nameof(zplContainer));
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
      this.ZplTransformer.Transform(svgLine,
                                    sourceMatrix,
                                    viewMatrix,
                                    out startX,
                                    out startY,
                                    out endX,
                                    out endY,
                                    out strokeWidth);

      var horizontalStart = (int) startX;
      var verticalStart = (int) endY;
      var width = (int) (endX - startX);
      var height = (int) (endY - startY);
      var thickness = (int) strokeWidth;

      zplContainer.Body.Add(this.ZplCommands.FieldTypeset(horizontalStart,
                                                          verticalStart));
      zplContainer.Body.Add(this.ZplCommands.GraphicBox(width,
                                                        height,
                                                        thickness,
                                                        LineColor.Black));
    }
  }
}
