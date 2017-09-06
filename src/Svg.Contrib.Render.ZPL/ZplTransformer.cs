using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using JetBrains.Annotations;

// ReSharper disable NonLocalizedString
// ReSharper disable VirtualMemberNeverOverriden.Global

namespace Svg.Contrib.Render.ZPL
{
  public class ZplTransformer : GenericTransformer
  {
    public const int DefaultOutputHeight = 1296;
    public const int DefaultOutputWidth = 816;

    /// <inheritdoc />
    public ZplTransformer([NotNull] SvgUnitReader svgUnitReader,
                          int outputWidth = ZplTransformer.DefaultOutputWidth,
                          int outputHeight = ZplTransformer.DefaultOutputHeight)
      : base(svgUnitReader,
             outputWidth,
             outputHeight) { }

    [NotNull]
    private IDictionary<int, FieldOrientation> SectorMappings { get; } = new Dictionary<int, FieldOrientation>
                                                                         {
                                                                           {
                                                                             0, FieldOrientation.Normal
                                                                           },
                                                                           {
                                                                             1, FieldOrientation.RotatedBy90Degrees
                                                                           },
                                                                           {
                                                                             2, FieldOrientation.RotatedBy180Degrees
                                                                           },
                                                                           {
                                                                             3, FieldOrientation.RotatedBy270Degrees
                                                                           }
                                                                         };

    /// <exception cref="ArgumentNullException"><paramref name="sourceMatrix" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="viewMatrix" /> is <see langword="null" />.</exception>
    [Pure]
    public virtual FieldOrientation GetFieldOrientation([NotNull] Matrix sourceMatrix,
                                                        [NotNull] Matrix viewMatrix)
    {
      if (sourceMatrix == null)
      {
        throw new ArgumentNullException(nameof(sourceMatrix));
      }
      if (viewMatrix == null)
      {
        throw new ArgumentNullException(nameof(viewMatrix));
      }

      var sector = this.GetRotationSector(sourceMatrix,
                                          viewMatrix);

      var fieldOrientation = this.SectorMappings[sector];

      return fieldOrientation;
    }

    /// <inheritdoc />
    public override void Transform(SvgImage svgImage,
                                   Matrix sourceMatrix,
                                   Matrix viewMatrix,
                                   out float startX,
                                   out float startY,
                                   out float endX,
                                   out float endY,
                                   out float sourceAlignmentWidth,
                                   out float sourceAlignmentHeight)
    {
      base.Transform(svgImage,
                     sourceMatrix,
                     viewMatrix,
                     out startX,
                     out startY,
                     out endX,
                     out endY,
                     out sourceAlignmentWidth,
                     out sourceAlignmentHeight);

      var height = endY - startY;

      startY += height;
      endY += height;
    }

    /// <exception cref="ArgumentNullException"><paramref name="svgTextBase" /> is <see langword="null" />.</exception>
    [Pure]
    public virtual void GetFontSelection([NotNull] SvgTextBase svgTextBase,
                                         float fontSize,
                                         [NotNull] out string fontName,
                                         out int characterHeight,
                                         out int width)
    {
      if (svgTextBase == null)
      {
        throw new ArgumentNullException(nameof(svgTextBase));
      }

      fontName = "0";
      characterHeight = (int) Math.Max(fontSize,
                                       10f);
      width = 0;
    }

    /// <inheritdoc />
    /// <exception cref="ArgumentNullException"><paramref name="svgTextBase" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="sourceMatrix" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="viewMatrix" /> is <see langword="null" />.</exception>
    public override void Transform(SvgTextBase svgTextBase,
                                   Matrix sourceMatrix,
                                   Matrix viewMatrix,
                                   out float startX,
                                   out float startY,
                                   out float fontSize)
    {
      if (svgTextBase == null)
      {
        throw new ArgumentNullException(nameof(svgTextBase));
      }
      if (sourceMatrix == null)
      {
        throw new ArgumentNullException(nameof(sourceMatrix));
      }
      if (viewMatrix == null)
      {
        throw new ArgumentNullException(nameof(viewMatrix));
      }

      base.Transform(svgTextBase,
                     sourceMatrix,
                     viewMatrix,
                     out startX,
                     out startY,
                     out fontSize);

      var lineHeightFactor = this.GetLineHeightFactor(svgTextBase);
      if (this.GetRotationSector(sourceMatrix,
                                 viewMatrix) % 2 == 0)
      {
        if (lineHeightFactor > 0f)
        {
          startY -= fontSize / lineHeightFactor;
        }
        else
        {
          startY -= fontSize;
        }
      }
      else
      {
        if (lineHeightFactor > 0f)
        {
          startX += fontSize / lineHeightFactor;
        }
        else
        {
          startX += fontSize;
        }
      }
    }

    /// <inheritdoc />
    public override void Transform(SvgRectangle svgRectangle,
                                   Matrix sourceMatrix,
                                   Matrix viewMatrix,
                                   out float startX,
                                   out float startY,
                                   out float endX,
                                   out float endY,
                                   out float strokeWidth)
    {
      base.Transform(svgRectangle,
                     sourceMatrix,
                     viewMatrix,
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
  }
}
