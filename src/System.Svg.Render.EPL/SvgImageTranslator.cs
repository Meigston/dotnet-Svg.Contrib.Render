﻿using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using JetBrains.Annotations;

// ReSharper disable NonLocalizedString

namespace System.Svg.Render.EPL
{
  [PublicAPI]
  public class SvgImageTranslator : SvgElementTranslatorBase<SvgImage>
  {
    public SvgImageTranslator([NotNull] EplTransformer eplTransformer,
                              [NotNull] EplCommands eplCommands)
    {
      this.EplTransformer = eplTransformer;
      this.EplCommands = eplCommands;
    }

    [NotNull]
    protected EplTransformer EplTransformer { get; }

    [NotNull]
    protected EplCommands EplCommands { get; }

    [NotNull]
    [ItemNotNull]
    private IDictionary<string, string> ImageIdentifierToVariableNameMap { get; } = new Dictionary<string, string>();

    [NotNull]
    [Pure]
    [MustUseReturnValue]
    protected virtual string CalculateImageIdentifier([NotNull] SvgImage svgImage)
    {
      var result = string.Concat(svgImage.OwnerDocument.ID,
                                 "::",
                                 svgImage.ID);

      return result;
    }

    protected virtual void StoreVariableNameForImageIdentifier([NotNull] string imageIdentifier,
                                                               [NotNull] string variableName)
    {
      // ReSharper disable ExceptionNotDocumentedOptional
      this.ImageIdentifierToVariableNameMap[imageIdentifier] = variableName;
      // ReSharper restore ExceptionNotDocumentedOptional
    }

    public override void Translate([NotNull] SvgImage svgElement,
                                   [NotNull] Matrix matrix,
                                   [NotNull] Container<EplStream> container)

    {
      float startX;
      float startY;
      float endX;
      float endY;
      float sourceAlignmentWidth;
      float sourceAlignmentHeight;
      this.EplTransformer.Transform(svgElement,
                                    matrix,
                                    out startX,
                                    out startY,
                                    out endX,
                                    out endY,
                                    out sourceAlignmentWidth,
                                    out sourceAlignmentHeight);

      var horizontalStart = (int) startX;
      var verticalStart = (int) startY;

      var forceDirectWrite = this.ForceDirectWrite(svgElement);
      if (forceDirectWrite)
      {
        this.GraphicDirectWrite(svgElement,
                                matrix,
                                sourceAlignmentWidth,
                                sourceAlignmentHeight,
                                horizontalStart,
                                verticalStart,
                                container);
      }
      else
      {
        var variableName = this.StoreGraphics(svgElement,
                                              matrix,
                                              sourceAlignmentWidth,
                                              sourceAlignmentHeight,
                                              horizontalStart,
                                              verticalStart,
                                              container);
        if (variableName != null)
        {
          this.PrintGraphics(horizontalStart,
                             verticalStart,
                             variableName,
                             container);
        }
      }
    }

    protected virtual void GraphicDirectWrite([NotNull] SvgImage svgElement,
                                              [NotNull] Matrix matrix,
                                              float sourceAlignmentWidth,
                                              float sourceAlignmentHeight,
                                              int horizontalStart,
                                              int verticalStart,
                                              [NotNull] Container<EplStream> container)
    {
      using (var bitmap = this.ConvertToBitmap(svgElement,
                                               matrix,
                                               (int) sourceAlignmentWidth,
                                               (int) sourceAlignmentHeight))
      {
        if (bitmap == null)
        {
          return;
        }

        container.Body.Add(this.EplCommands.GraphicDirectWrite(bitmap,
                                                               horizontalStart,
                                                               verticalStart));
      }
    }

    [NotNull]
    [Pure]
    [MustUseReturnValue]
    protected virtual string CalculateVariableName([NotNull] string imageIdentifier)
    {
      // TODO this is magic
      // on purpose: the imageIdentifier should be hashed to 8 chars
      // long, and should always be the same for the same imageIdentifier
      // thus going for this pile of shit ...
      var variableName = Math.Abs(imageIdentifier.GetHashCode())
                             .ToString();
      if (variableName.Length > 8)
      {
        // ReSharper disable ExceptionNotDocumentedOptional
        variableName = variableName.Substring(0,
                                              8);
        // ReSharper restore ExceptionNotDocumentedOptional
      }

      return variableName;
    }

    [CanBeNull]
    [Pure]
    [MustUseReturnValue]
    protected virtual string StoreGraphics([NotNull] SvgImage svgElement,
                                           [NotNull] Matrix matrix,
                                           float sourceAlignmentWidth,
                                           float sourceAlignmentHeight,
                                           int horizontalStart,
                                           int verticalStart,
                                           [NotNull] Container<EplStream> container)
    {
      string variableName;
      var imageIdentifier = this.CalculateImageIdentifier(svgElement);
      if (!this.ImageIdentifierToVariableNameMap.TryGetValue(imageIdentifier,
                                                             out variableName))
      {
        variableName = this.CalculateVariableName(imageIdentifier);
        this.StoreVariableNameForImageIdentifier(imageIdentifier,
                                                 variableName);

        using (var bitmap = this.ConvertToBitmap(svgElement,
                                                 matrix,
                                                 (int) sourceAlignmentWidth,
                                                 (int) sourceAlignmentHeight))
        {
          if (bitmap == null)
          {
            return null;
          }

          container.Header.Add(this.EplCommands.DeleteGraphics(variableName));
          container.Header.Add(this.EplCommands.DeleteGraphics(variableName));
          container.Header.Add(this.EplCommands.StoreGraphics(bitmap,
                                                              variableName));
        }
      }

      return variableName;
    }

    [CanBeNull]
    [Pure]
    [MustUseReturnValue]
    protected virtual Bitmap ConvertToBitmap([NotNull] SvgImage svgElement,
                                             [NotNull] Matrix matrix,
                                             int sourceAlignmentWidth,
                                             int sourceAlignmentHeight)
    {
      var stretchImage = this.StretchImage(svgElement);

      using (var image = svgElement.GetImage() as Image)
      {
        if (image == null)
        {
          return null;
        }

        Bitmap bitmap;
        if (stretchImage)
        {
          bitmap = new Bitmap(image,
                              sourceAlignmentWidth,
                              sourceAlignmentHeight);
        }
        else
        {
          var sourceRatio = (float) sourceAlignmentWidth / sourceAlignmentHeight;
          var destinationRatio = (float) image.Width / image.Height;

          // TODO find a good TOLERANCE
          if (Math.Abs(sourceRatio - destinationRatio) < 0.5f)
          {
            bitmap = new Bitmap(image,
                                sourceAlignmentWidth,
                                sourceAlignmentHeight);
          }
          else
          {
            int destinationWidth;
            int destinationHeight;

            if (sourceRatio < destinationRatio)
            {
              destinationWidth = sourceAlignmentWidth;
              destinationHeight = (int) (sourceAlignmentWidth / destinationRatio);
            }
            else
            {
              destinationWidth = (int) (sourceAlignmentHeight * destinationRatio);
              destinationHeight = sourceAlignmentHeight;
            }

            var x = (sourceAlignmentWidth - destinationWidth) / 2;
            var y = (sourceAlignmentHeight - destinationHeight) / 2;

            bitmap = new Bitmap(sourceAlignmentWidth,
                                sourceAlignmentHeight);
            using (var graphics = Graphics.FromImage(bitmap))
            {
              var rect = new Rectangle(x,
                                       y,
                                       destinationWidth,
                                       destinationHeight);
              graphics.DrawImage(image,
                                 rect);
            }
          }
        }

        var rotateFlipType = (RotateFlipType) this.EplTransformer.GetRotationSector(matrix);
        bitmap.RotateFlip(rotateFlipType);

        return bitmap;
      }
    }

    protected virtual void PrintGraphics(int horizontalStart,
                                         int verticalStart,
                                         [NotNull] string variableName,
                                         [NotNull] Container<EplStream> container)
    {
      container.Body.Add(this.EplCommands.PrintGraphics(horizontalStart,
                                                        verticalStart,
                                                        variableName));
    }

    [Pure]
    [MustUseReturnValue]
    protected virtual bool ForceDirectWrite([NotNull] SvgImage svgImage)
    {
      return false;
    }

    [Pure]
    [MustUseReturnValue]
    protected virtual bool StretchImage([NotNull] SvgImage svgImage)
    {
      return false;
    }
  }
}