﻿using System.Drawing;
using System.Drawing.Drawing2D;
using JetBrains.Annotations;

namespace System.Svg.Render.ZPL
{
  [PublicAPI]
  public class SvgRectangleTranslator : SvgElementTranslatorBase<SvgRectangle>
  {
    public SvgRectangleTranslator([NotNull] ZplTransformer zplTransformer,
                                  [NotNull] ZplCommands zplCommands,
                                  [NotNull] SvgUnitReader svgUnitReader)
    {
      this.ZplTransformer = zplTransformer;
      this.ZplCommands = zplCommands;
      this.SvgUnitReader = svgUnitReader;
    }

    [NotNull]
    protected ZplTransformer ZplTransformer { get; }

    [NotNull]
    protected ZplCommands ZplCommands { get; }

    [NotNull]
    protected SvgUnitReader SvgUnitReader { get; }

    public override void Translate([NotNull] SvgRectangle svgElement,
                                   [NotNull] Matrix matrix,
                                   [NotNull] ZplStream container)
    {
      if (svgElement.Fill != SvgPaintServer.None
          && (svgElement.Fill as SvgColourServer)?.Colour != Color.White)
      {
        this.TranslateFilledBox(svgElement,
                                matrix,
                                container);
      }
      else if (svgElement.Stroke != SvgPaintServer.None)
      {
        this.TranslateBox(svgElement,
                          matrix,
                          container);
      }
    }

    protected virtual void TranslateFilledBox([NotNull] SvgRectangle instance,
                                              [NotNull] Matrix matrix,
                                              [NotNull] ZplStream container)
    {
      // TODO fix this! square gets rendered ...

      var startX = this.SvgUnitReader.GetValue(instance,
                                               instance.X);
      var startY = this.SvgUnitReader.GetValue(instance,
                                               instance.Y);
      var endX = startX + this.SvgUnitReader.GetValue(instance,
                                                      instance.Width);
      var endY = startY + this.SvgUnitReader.GetValue(instance,
                                                      instance.Height);

      var svgLine = new SvgLine
                    {
                      Color = instance.Color,
                      Stroke = SvgPaintServer.None,
                      StrokeWidth = instance.StrokeWidth,
                      StartX = startX,
                      StartY = startY,
                      EndX = endX,
                      EndY = endY
                    };

      float strokeWidth;
      this.ZplTransformer.Transform(svgLine,
                                    matrix,
                                    out startX,
                                    out startY,
                                    out endX,
                                    out endY,
                                    out strokeWidth);

      var horizontalStart = (int) startX;
      var verticalStart = (int) endY;
      var width = (int) (endX - startX);
      var thickness = (int) (endY - startY);

      container.Add(this.ZplCommands.FieldTypeset(horizontalStart,
                                                  verticalStart));
      container.Add(this.ZplCommands.GraphicBox(width,
                                                0,
                                                thickness,
                                                LineColor.Black));
    }

    protected virtual void TranslateBox([NotNull] SvgRectangle instance,
                                        [NotNull] Matrix matrix,
                                        [NotNull] ZplStream container)
    {
      float startX;
      float endX;
      float startY;
      float endY;
      float strokeWidth;
      this.ZplTransformer.Transform(instance,
                                    matrix,
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

      container.Add(this.ZplCommands.FieldTypeset(horizontalStart,
                                                  verticalStart));
      container.Add(this.ZplCommands.GraphicBox(width,
                                                height,
                                                thickness,
                                                LineColor.Black));
    }
  }
}