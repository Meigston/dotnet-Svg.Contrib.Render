﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using JetBrains.Annotations;

// ReSharper disable NonLocalizedString

namespace Svg.Contrib.Render
{
  [PublicAPI]
  public abstract class SvgImageTranslatorBase<TContainer> : SvgElementTranslatorBase<TContainer, SvgImage>
    where TContainer : Container
  {
    protected SvgImageTranslatorBase([NotNull] GenericTransformer genericTransformer)
    {
      this.GenericTransformer = genericTransformer;
    }

    [NotNull]
    protected GenericTransformer GenericTransformer { get; }

    [NotNull]
    [ItemNotNull]
    private IDictionary<string, string> ImageIdentifierToVariableNameMap { get; } = new Dictionary<string, string>();

    protected virtual void StoreGraphics([NotNull] SvgImage svgElement,
                                         [NotNull] Matrix matrix,
                                         float sourceAlignmentWidth,
                                         float sourceAlignmentHeight,
                                         int horizontalStart,
                                         int verticalStart,
                                         [NotNull] TContainer container,
                                         [CanBeNull] out string variableName)
    {
      var imageIdentifier = string.Concat(svgElement.OwnerDocument.ID,
                                          "::",
                                          svgElement.ID);

      if (!this.ImageIdentifierToVariableNameMap.TryGetValue(imageIdentifier,
                                                             out variableName))
      {
        variableName = this.CalculateVariableName(imageIdentifier);
        this.ImageIdentifierToVariableNameMap[imageIdentifier] = variableName;

        using (var bitmap = this.GenericTransformer.ConvertToBitmap(svgElement,
                                                                    matrix,
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

    protected abstract void StoreGraphics([NotNull] string variableName,
                                          [NotNull] Bitmap bitmap,
                                          [NotNull] TContainer container);

    public override void Translate([NotNull] SvgImage svgElement,
                                   [NotNull] Matrix matrix,
                                   [NotNull] TContainer container)

    {
      float sourceAlignmentWidth;
      float sourceAlignmentHeight;
      int horizontalStart;
      int verticalStart;
      int sector;
      this.GetPosition(svgElement,
                       matrix,
                       out sourceAlignmentWidth,
                       out sourceAlignmentHeight,
                       out horizontalStart,
                       out verticalStart,
                       out sector);

      this.AddTranslationToContainer(svgElement,
                                     matrix,
                                     sourceAlignmentWidth,
                                     sourceAlignmentHeight,
                                     horizontalStart,
                                     verticalStart,
                                     sector,
                                     container);
    }

    [Pure]
    protected virtual void GetPosition([NotNull] SvgImage svgElement,
                                       [NotNull] Matrix matrix,
                                       out float sourceAlignmentWidth,
                                       out float sourceAlignmentHeight,
                                       out int horizontalStart,
                                       out int verticalStart,
                                       out int sector)
    {
      float startX;
      float startY;
      float endX;
      float endY;
      this.GenericTransformer.Transform(svgElement,
                                        matrix,
                                        out startX,
                                        out startY,
                                        out endX,
                                        out endY,
                                        out sourceAlignmentWidth,
                                        out sourceAlignmentHeight);

      horizontalStart = (int) startX;
      verticalStart = (int) startY;
      sector = this.GenericTransformer.GetRotationSector(matrix);
    }

    protected virtual void AddTranslationToContainer([NotNull] SvgImage svgElement,
                                                     [NotNull] Matrix matrix,
                                                     float sourceAlignmentWidth,
                                                     float sourceAlignmentHeight,
                                                     int horizontalStart,
                                                     int verticalStart,
                                                     int sector,
                                                     [NotNull] TContainer container)
    {
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
        string variableName;
        this.StoreGraphics(svgElement,
                           matrix,
                           sourceAlignmentWidth,
                           sourceAlignmentHeight,
                           horizontalStart,
                           verticalStart,
                           container,
                           out variableName);
        if (variableName != null)
        {
          this.PrintGraphics(horizontalStart,
                             verticalStart,
                             variableName,
                             container);
        }
      }
    }

    protected abstract void GraphicDirectWrite([NotNull] SvgImage svgElement,
                                               [NotNull] Matrix matrix,
                                               float sourceAlignmentWidth,
                                               float sourceAlignmentHeight,
                                               int horizontalStart,
                                               int verticalStart,
                                               [NotNull] TContainer container);

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

    protected abstract void PrintGraphics(int horizontalStart,
                                          int verticalStart,
                                          [NotNull] string variableName,
                                          [NotNull] TContainer container);

    [Pure]
    [MustUseReturnValue]
    protected virtual bool ForceDirectWrite([NotNull] SvgImage svgImage) => false;
  }
}