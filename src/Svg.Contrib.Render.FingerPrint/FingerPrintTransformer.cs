﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using ImageMagick;
using JetBrains.Annotations;

// ReSharper disable NonLocalizedString
// ReSharper disable ClassWithVirtualMembersNeverInherited.Global

namespace Svg.Contrib.Render.FingerPrint
{
  [PublicAPI]
  public class FingerPrintTransformer : GenericTransformer
  {
    public const int DefaultOutputHeight = 1296;
    public const int DefaultOutputWidth = 816;

    public FingerPrintTransformer([NotNull] SvgUnitReader svgUnitReader)
      : base(svgUnitReader,
             FingerPrintTransformer.DefaultOutputWidth,
             FingerPrintTransformer.DefaultOutputHeight) {}

    public FingerPrintTransformer([NotNull] SvgUnitReader svgUnitReader,
                                  int outputWidth,
                                  int outputHeight)
      : base(svgUnitReader,
             outputWidth,
             outputHeight) {}

    [Pure]
    [NotNull]
    [MustUseReturnValue]
    protected override Matrix CreateDeviceMatrix()
    {
      var deviceMatrix = new Matrix(1,
                                    0,
                                    0,
                                    -1,
                                    0,
                                    0);
      return deviceMatrix;
    }

    [Pure]
    [NotNull]
    [MustUseReturnValue]
    protected override Matrix ApplyViewRotationOnDeviceMatrix([NotNull] Matrix deviceMatrix,
                                                              ViewRotation viewRotation = ViewRotation.Normal)
    {
      var viewMatrix = deviceMatrix.Clone();

      if (viewRotation == ViewRotation.Normal)
      {
        viewMatrix.Translate(0,
                             this.OutputHeight,
                             MatrixOrder.Append);
      }
      else if (viewRotation == ViewRotation.RotateBy90Degrees)
      {
        viewMatrix.Rotate(270f);
      }
      else if (viewRotation == ViewRotation.RotateBy180Degrees)
      {
        throw new NotImplementedException();
        viewMatrix.Rotate(180f);
        viewMatrix.Translate(-this.OutputWidth,
                             this.OutputHeight,
                             MatrixOrder.Append);
      }
      else if (viewRotation == ViewRotation.RotateBy270Degress)
      {
        throw new NotImplementedException();
        viewMatrix.Rotate(90f);
        viewMatrix.Translate(0,
                             this.OutputHeight,
                             MatrixOrder.Append);
      }

      return viewMatrix;
    }

    [Pure]
    public override void Transform([NotNull] SvgRectangle svgRectangle,
                                   [NotNull] Matrix matrix,
                                   out float startX,
                                   out float startY,
                                   out float endX,
                                   out float endY,
                                   out float strokeWidth)
    {
      base.Transform(svgRectangle,
                     matrix,
                     out startX,
                     out startY,
                     out endX,
                     out endY,
                     out strokeWidth);

      startX -= strokeWidth / 2f;
      endX += strokeWidth / 2f;
      startY -= strokeWidth / 2f;
      endY += strokeWidth / 2f;
    }

    [Pure]
    public void Transform([NotNull] SvgTextBase svgTextBase,
                          [NotNull] Matrix matrix,
                          out float startX,
                          out float startY,
                          out float fontSize,
                          out Direction direction)
    {
      startX = this.SvgUnitReader.GetValue(svgTextBase,
                                           svgTextBase.X.FirstOrDefault());
      startY = this.SvgUnitReader.GetValue(svgTextBase,
                                           svgTextBase.Y.FirstOrDefault());
      fontSize = this.SvgUnitReader.GetValue(svgTextBase,
                                             svgTextBase.FontSize);

      direction = this.GetDirection(matrix);

      if ((int) direction % 2 == 0)
      {
        //startY -= fontSize / this.GetLineHeightFactor(svgTextBase);
      }
      else
      {
        startX -= fontSize / this.GetLineHeightFactor(svgTextBase);
      }

      this.ApplyMatrixOnPoint(startX,
                              startY,
                              matrix,
                              out startX,
                              out startY);
    }

    [MustUseReturnValue]
    [Pure]
    public virtual Direction GetDirection([NotNull] Matrix matrix)
    {
      var sector = this.GetRotationSector(matrix);
      var direction = (Direction) ((4 - sector) % 4 + 1);

      return direction;
    }

    [Pure]
    public void GetFontSelection([NotNull] SvgTextBase svgTextBase,
                                 float fontSize,
                                 out string fontName,
                                 out int characterHeight,
                                 out int slant)
    {
      if (svgTextBase.FontWeight > SvgFontWeight.Normal)
      {
        fontName = "Swiss 721 Bold BT";
      }
      else
      {
        fontName = "Swiss 721 BT";
      }

      characterHeight = (int) fontSize;

      if ((svgTextBase.FontStyle & SvgFontStyle.Italic) != 0)
      {
        slant = 20;
      }
      else
      {
        slant = 0;
      }
    }

    [NotNull]
    [Pure]
    [MustUseReturnValue]
    public override IEnumerable<byte> GetRawBinaryData([NotNull] Bitmap bitmap,
                                                       bool invertBytes,
                                                       int numberOfBytesPerRow)
    {
      var result = new byte[]
                   {
                     0x40,
                     0x00
                   }.Concat(base.GetRawBinaryData(bitmap,
                                                  invertBytes,
                                                  numberOfBytesPerRow));

      return result;
    }

    [NotNull]
    [Pure]
    [MustUseReturnValue]
    public virtual byte[] ConvertToPcx([NotNull] Bitmap bitmap)
    {
      var width = bitmap.Width;
      var mod = width % 8;
      if (mod > 0)
      {
        width += 8 - mod;
      }
      var height = bitmap.Height;

      using (var magickImage = new MagickImage(bitmap))
      {
        if (mod > 0)
        {
          var magickGeometry = new MagickGeometry
          {
            Width = width,
            Height = height,
            IgnoreAspectRatio = true
          };
          magickImage.Resize(magickGeometry);
        }

        magickImage.ColorAlpha(MagickColors.White);

        var quantizeSettings = new QuantizeSettings
        {
          Colors = 2,
          DitherMethod = DitherMethod.No
        };
        magickImage.Quantize(quantizeSettings);

        magickImage.Format = MagickFormat.Pcx;
        magickImage.ColorType = ColorType.Palette;
        magickImage.ColorSpace = ColorSpace.Gray;

        var array = magickImage.ToByteArray();

        return array;
      }
    }
  }
}