﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using JetBrains.Annotations;

// ReSharper disable NonLocalizedString
// ReSharper disable VirtualMemberNeverOverriden.Global

namespace Svg.Contrib.Render
{
  [PublicAPI]
  public abstract class SvgImageTranslatorBase<TContainer> : SvgElementTranslatorBase<TContainer, SvgImage>
    where TContainer : Container
  {
    /// <exception cref="ArgumentNullException"><paramref name="genericTransformer" /> is <see langword="null" />.</exception>
    protected SvgImageTranslatorBase([NotNull] GenericTransformer genericTransformer)
    {
      if (genericTransformer == null)
      {
        throw new ArgumentNullException(nameof(genericTransformer));
      }
      this.GenericTransformer = genericTransformer;
    }

    [NotNull]
    protected GenericTransformer GenericTransformer { get; }

    [NotNull]
    [ItemNotNull]
    private IDictionary<string, string> ImageIdentifierToVariableNameMap { get; } = new Dictionary<string, string>();

    /// <exception cref="ArgumentNullException"><paramref name="svgImage" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="sourceMatrix" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="viewMatrix" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="container" /> is <see langword="null" />.</exception>
    protected virtual void StoreGraphics([NotNull] SvgImage svgImage,
                                         [NotNull] Matrix sourceMatrix,
                                         [NotNull] Matrix viewMatrix,
                                         float sourceAlignmentWidth,
                                         float sourceAlignmentHeight,
                                         int horizontalStart,
                                         int verticalStart,
                                         [NotNull] TContainer container,
                                         [CanBeNull] out string variableName)
    {
      if (svgImage == null)
      {
        throw new ArgumentNullException(nameof(svgImage));
      }
      if (sourceMatrix == null)
      {
        throw new ArgumentNullException(nameof(sourceMatrix));
      }
      if (viewMatrix == null)
      {
        throw new ArgumentNullException(nameof(viewMatrix));
      }
      if (container == null)
      {
        throw new ArgumentNullException(nameof(container));
      }

      var imageIdentifier = string.Concat(svgImage.OwnerDocument.ID,
                                          "::",
                                          svgImage.ID);

      if (!this.ImageIdentifierToVariableNameMap.TryGetValue(imageIdentifier,
                                                             out variableName))
      {
        variableName = this.CalculateVariableName(imageIdentifier);
        this.ImageIdentifierToVariableNameMap[imageIdentifier] = variableName;

        using (var bitmap = this.GenericTransformer.ConvertToBitmap(svgImage,
                                                                    sourceMatrix,
                                                                    viewMatrix,
                                                                    (int) sourceAlignmentWidth,
                                                                    (int) sourceAlignmentHeight))
        {
          if (bitmap == null)
          {
            variableName = null;
            return;
          }

          this.StoreGraphics(variableName,
                             bitmap,
                             container);
        }
      }
    }

    /// <exception cref="ArgumentNullException"><paramref name="variableName" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="bitmap" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="container" /> is <see langword="null" />.</exception>
    protected abstract void StoreGraphics([NotNull] string variableName,
                                          [NotNull] Bitmap bitmap,
                                          [NotNull] TContainer container);

    /// <exception cref="ArgumentNullException"><paramref name="svgImage" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="sourceMatrix" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="viewMatrix" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="container" /> is <see langword="null" />.</exception>
    public override void Translate([NotNull] SvgImage svgImage,
                                   [NotNull] Matrix sourceMatrix,
                                   [NotNull] Matrix viewMatrix,
                                   [NotNull] TContainer container)

    {
      if (svgImage == null)
      {
        throw new ArgumentNullException(nameof(svgImage));
      }
      if (sourceMatrix == null)
      {
        throw new ArgumentNullException(nameof(sourceMatrix));
      }
      if (viewMatrix == null)
      {
        throw new ArgumentNullException(nameof(viewMatrix));
      }
      if (container == null)
      {
        throw new ArgumentNullException(nameof(container));
      }

      float sourceAlignmentWidth;
      float sourceAlignmentHeight;
      int horizontalStart;
      int verticalStart;
      int sector;
      this.GetPosition(svgImage,
                       sourceMatrix,
                       viewMatrix,
                       out sourceAlignmentWidth,
                       out sourceAlignmentHeight,
                       out horizontalStart,
                       out verticalStart,
                       out sector);

      this.AddTranslationToContainer(svgImage,
                                     sourceMatrix,
                                     viewMatrix,
                                     sourceAlignmentWidth,
                                     sourceAlignmentHeight,
                                     horizontalStart,
                                     verticalStart,
                                     sector,
                                     container);
    }

    /// <exception cref="ArgumentNullException"><paramref name="svgImage" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="sourceMatrix" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="viewMatrix" /> is <see langword="null" />.</exception>
    [Pure]
    protected virtual void GetPosition([NotNull] SvgImage svgImage,
                                       [NotNull] Matrix sourceMatrix,
                                       [NotNull] Matrix viewMatrix,
                                       out float sourceAlignmentWidth,
                                       out float sourceAlignmentHeight,
                                       out int horizontalStart,
                                       out int verticalStart,
                                       out int sector)
    {
      if (svgImage == null)
      {
        throw new ArgumentNullException(nameof(svgImage));
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
      this.GenericTransformer.Transform(svgImage,
                                        sourceMatrix,
                                        viewMatrix,
                                        out startX,
                                        out startY,
                                        out endX,
                                        out endY,
                                        out sourceAlignmentWidth,
                                        out sourceAlignmentHeight);

      horizontalStart = (int) startX;
      verticalStart = (int) startY;
      sector = this.GenericTransformer.GetRotationSector(sourceMatrix,
                                                         viewMatrix);
    }

    /// <exception cref="ArgumentNullException"><paramref name="svgImage" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="sourceMatrix" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="viewMatrix" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="container" /> is <see langword="null" />.</exception>
    protected virtual void AddTranslationToContainer([NotNull] SvgImage svgImage,
                                                     [NotNull] Matrix sourceMatrix,
                                                     [NotNull] Matrix viewMatrix,
                                                     float sourceAlignmentWidth,
                                                     float sourceAlignmentHeight,
                                                     int horizontalStart,
                                                     int verticalStart,
                                                     int sector,
                                                     [NotNull] TContainer container)
    {
      if (svgImage == null)
      {
        throw new ArgumentNullException(nameof(svgImage));
      }
      if (sourceMatrix == null)
      {
        throw new ArgumentNullException(nameof(sourceMatrix));
      }
      if (viewMatrix == null)
      {
        throw new ArgumentNullException(nameof(viewMatrix));
      }
      if (container == null)
      {
        throw new ArgumentNullException(nameof(container));
      }

      var forceDirectWrite = this.ForceDirectWrite(svgImage);
      if (forceDirectWrite)
      {
        this.GraphicDirectWrite(svgImage,
                                sourceMatrix,
                                viewMatrix,
                                sourceAlignmentWidth,
                                sourceAlignmentHeight,
                                horizontalStart,
                                verticalStart,
                                container);
      }
      else
      {
        string variableName;
        this.StoreGraphics(svgImage,
                           sourceMatrix,
                           viewMatrix,
                           sourceAlignmentWidth,
                           sourceAlignmentHeight,
                           horizontalStart,
                           verticalStart,
                           container,
                           out variableName);
        if (variableName != null)
        {
          this.PrintGraphics(svgImage,
                             sourceMatrix,
                             viewMatrix,
                             horizontalStart,
                             verticalStart,
                             sector,
                             variableName,
                             container);
        }
      }
    }

    /// <exception cref="ArgumentNullException"><paramref name="svgImage" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="sourceMatrix" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="viewMatrix" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="container" /> is <see langword="null" />.</exception>
    protected abstract void GraphicDirectWrite([NotNull] SvgImage svgImage,
                                               [NotNull] Matrix sourceMatrix,
                                               [NotNull] Matrix viewMatrix,
                                               float sourceAlignmentWidth,
                                               float sourceAlignmentHeight,
                                               int horizontalStart,
                                               int verticalStart,
                                               [NotNull] TContainer container);

    /// <exception cref="ArgumentNullException"><paramref name="imageIdentifier" /> is <see langword="null" />.</exception>
    [NotNull]
    [Pure]
    protected virtual string CalculateVariableName([NotNull] string imageIdentifier)
    {
      if (imageIdentifier == null)
      {
        throw new ArgumentNullException(nameof(imageIdentifier));
      }

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

    /// <exception cref="ArgumentNullException"><paramref name="svgImage" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="sourceMatrix" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="viewMatrix" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="variableName" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="container" /> is <see langword="null" />.</exception>
    protected abstract void PrintGraphics([NotNull] SvgImage svgImage,
                                          [NotNull] Matrix sourceMatrix,
                                          [NotNull] Matrix viewMatrix,
                                          int horizontalStart,
                                          int verticalStart,
                                          int sector,
                                          [NotNull] string variableName,
                                          [NotNull] TContainer container);

    /// <exception cref="ArgumentNullException"><paramref name="svgImage" /> is <see langword="null" />.</exception>
    [Pure]
    protected virtual bool ForceDirectWrite([NotNull] SvgImage svgImage)
    {
      if (svgImage == null)
      {
        throw new ArgumentNullException(nameof(svgImage));
      }

      return false;
    }
  }
}
