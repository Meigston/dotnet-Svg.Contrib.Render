﻿using System.Drawing;
using System.Drawing.Drawing2D;
using JetBrains.Annotations;

// ReSharper disable NonLocalizedString
// ReSharper disable VirtualMemberNeverOverriden.Global

namespace Svg.Contrib.Render.ZPL
{
  [PublicAPI]
  public class SvgImageTranslator : SvgImageTranslatorBase<ZplContainer>
  {
    public SvgImageTranslator([NotNull] ZplTransformer zplTransformer,
                              [NotNull] ZplCommands zplCommands)
      : base(zplTransformer)
    {
      this.ZplTransformer = zplTransformer;
      this.ZplCommands = zplCommands;
    }

    [NotNull]
    protected ZplTransformer ZplTransformer { get; }

    [NotNull]
    protected ZplCommands ZplCommands { get; }

    protected override void StoreGraphics([NotNull] string variableName,
                                          [NotNull] Bitmap bitmap,
                                          [NotNull] ZplContainer container)
    {
      int numberOfBytesPerRow;
      var rawBinaryData = this.ZplTransformer.GetRawBinaryData(bitmap,
                                                               false,
                                                               out numberOfBytesPerRow);

      container.Header.Add(this.ZplCommands.DownloadGraphics(variableName,
                                                             rawBinaryData,
                                                             numberOfBytesPerRow));
    }

    protected override void GraphicDirectWrite([NotNull] SvgImage svgElement,
                                               [NotNull] Matrix sourceMatrix,
                                               [NotNull] Matrix viewMatrix,
                                               float sourceAlignmentWidth,
                                               float sourceAlignmentHeight,
                                               int horizontalStart,
                                               int verticalStart,
                                               [NotNull] ZplContainer container)
    {
      using (var bitmap = this.ZplTransformer.ConvertToBitmap(svgElement,
                                                              sourceMatrix,
                                                              viewMatrix,
                                                              (int) sourceAlignmentWidth,
                                                              (int) sourceAlignmentHeight))
      {
        if (bitmap == null)
        {
          return;
        }

        int numberOfBytesPerRow;
        var rawBinaryData = this.ZplTransformer.GetRawBinaryData(bitmap,
                                                                 false,
                                                                 out numberOfBytesPerRow);

        container.Body.Add(this.ZplCommands.FieldTypeset(horizontalStart,
                                                         verticalStart));
        container.Body.Add(this.ZplCommands.GraphicField(rawBinaryData,
                                                         numberOfBytesPerRow));
      }
    }

    protected override void PrintGraphics([NotNull] SvgImage svgElement,
                                          [NotNull] Matrix sourceMatrix,
                                          [NotNull] Matrix viewMatrix,
                                          int horizontalStart,
                                          int verticalStart,
                                          int sector,
                                          [NotNull] string variableName,
                                          [NotNull] ZplContainer container)
    {
      container.Body.Add(this.ZplCommands.FieldTypeset(horizontalStart,
                                                       verticalStart));
      container.Body.Add(this.ZplCommands.RecallGraphic(variableName));
    }
  }
}