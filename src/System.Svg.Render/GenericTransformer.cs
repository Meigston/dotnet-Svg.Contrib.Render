﻿using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using JetBrains.Annotations;

namespace System.Svg.Render
{
  [PublicAPI]
  public class GenericTransformer
  {
    public GenericTransformer([NotNull] SvgUnitReader svgUnitReader)
    {
      this.SvgUnitReader = svgUnitReader;
    }

    [NotNull]
    protected SvgUnitReader SvgUnitReader { get; }

    [Pure]
    [MustUseReturnValue]
    // ReSharper disable UnusedParameter.Global
    protected virtual float GetLineHeightFactor([NotNull] SvgTextBase svgTextBase) => 1.25f;
    // ReSharper restore UnusedParameter.Global

    [Pure]
    protected virtual void ApplyMatrixOnPoint(float x,
                                              float y,
                                              [NotNull] Matrix matrix,
                                              out float newX,
                                              out float newY)
    {
      var originalPoint = new PointF(x,
                                     y);

      var points = new[]
                   {
                     originalPoint
                   };
      matrix.TransformPoints(points);

      var transformedPoint = points[0];
      newX = transformedPoint.X;
      newY = transformedPoint.Y;
    }

    [Pure]
    [MustUseReturnValue]
    protected virtual float ApplyMatrixOnLength(float length,
                                                [NotNull] Matrix matrix)
    {
      var vector = new PointF(length,
                              0f);

      vector = this.ApplyMatrixOnVector(vector,
                                        matrix);

      var result = this.GetLengthOfVector(vector);

      return result;
    }

    [Pure]
    [MustUseReturnValue]
    protected virtual PointF ApplyMatrixOnVector(PointF vector,
                                                 [NotNull] Matrix matrix)
    {
      var vectors = new[]
                    {
                      vector
                    };

      matrix.TransformVectors(vectors);

      var result = vectors[0];

      return result;
    }

    [Pure]
    [MustUseReturnValue]
    protected virtual float GetLengthOfVector(PointF vector)
    {
      var result = Math.Sqrt(Math.Pow(vector.X,
                                      2) + Math.Pow(vector.Y,
                                                    2));

      return (int) result;
    }

    [Pure]
    public virtual void Transform([NotNull] SvgLine svgLine,
                                  [NotNull] Matrix matrix,
                                  out float startX,
                                  out float startY,
                                  out float endX,
                                  out float endY,
                                  out float strokeWidth)
    {
      startX = this.SvgUnitReader.GetValue(svgLine,
                                           svgLine.StartX);
      startY = this.SvgUnitReader.GetValue(svgLine,
                                           svgLine.StartY);
      endX = this.SvgUnitReader.GetValue(svgLine,
                                         svgLine.EndX);
      endY = this.SvgUnitReader.GetValue(svgLine,
                                         svgLine.EndY);
      strokeWidth = this.SvgUnitReader.GetValue(svgLine,
                                                svgLine.StrokeWidth);

      this.ApplyMatrixOnPoint(startX,
                              startY,
                              matrix,
                              out startX,
                              out startY);

      this.ApplyMatrixOnPoint(endX,
                              endY,
                              matrix,
                              out endX,
                              out endY);

      strokeWidth = this.ApplyMatrixOnLength(strokeWidth,
                                             matrix);
    }

    [Pure]
    public virtual void Transform([NotNull] SvgImage svgImage,
                                  [NotNull] Matrix matrix,
                                  out float startX,
                                  out float startY,
                                  out float endX,
                                  out float endY,
                                  out float sourceAlignmentWidth,
                                  out float sourceAlignmentHeight)
    {
      startX = this.SvgUnitReader.GetValue(svgImage,
                                           svgImage.X);
      startY = this.SvgUnitReader.GetValue(svgImage,
                                           svgImage.Y);
      sourceAlignmentWidth = this.SvgUnitReader.GetValue(svgImage,
                                                         svgImage.Width);
      sourceAlignmentHeight = this.SvgUnitReader.GetValue(svgImage,
                                                          svgImage.Height);
      endX = startX + sourceAlignmentWidth;
      endY = startY + sourceAlignmentHeight;

      this.ApplyMatrixOnPoint(startX,
                              startY,
                              matrix,
                              out startX,
                              out startY);

      sourceAlignmentWidth = this.ApplyMatrixOnLength(sourceAlignmentWidth,
                                                      matrix);
      sourceAlignmentHeight = this.ApplyMatrixOnLength(sourceAlignmentHeight,
                                                       matrix);

      this.ApplyMatrixOnPoint(endX,
                              endY,
                              matrix,
                              out endX,
                              out endY);
    }

    [Pure]
    public virtual void Transform([NotNull] SvgRectangle svgRectangle,
                                  [NotNull] Matrix matrix,
                                  out float startX,
                                  out float startY,
                                  out float endX,
                                  out float endY,
                                  out float strokeWidth)
    {
      startX = this.SvgUnitReader.GetValue(svgRectangle,
                                           svgRectangle.X);
      endX = startX + this.SvgUnitReader.GetValue(svgRectangle,
                                                  svgRectangle.Width);
      startY = this.SvgUnitReader.GetValue(svgRectangle,
                                           svgRectangle.Y);
      endY = startY + this.SvgUnitReader.GetValue(svgRectangle,
                                                  svgRectangle.Height);
      strokeWidth = this.SvgUnitReader.GetValue(svgRectangle,
                                                svgRectangle.StrokeWidth);

      this.ApplyMatrixOnPoint(startX,
                              startY,
                              matrix,
                              out startX,
                              out startY);

      this.ApplyMatrixOnPoint(endX,
                              endY,
                              matrix,
                              out endX,
                              out endY);

      strokeWidth = this.ApplyMatrixOnLength(strokeWidth,
                                             matrix);
    }

    [Pure]
    public virtual void Transform([NotNull] SvgTextBase svgTextBase,
                                  [NotNull] Matrix matrix,
                                  out float startX,
                                  out float startY,
                                  out float fontSize)
    {
      // ReSharper disable ExceptionNotDocumentedOptional
      startX = this.SvgUnitReader.GetValue(svgTextBase,
                                           (svgTextBase.X ?? Enumerable.Empty<SvgUnit>()).FirstOrDefault());
      startY = this.SvgUnitReader.GetValue(svgTextBase,
                                           (svgTextBase.Y ?? Enumerable.Empty<SvgUnit>()).FirstOrDefault());
      // ReSharper restore ExceptionNotDocumentedOptional
      fontSize = this.SvgUnitReader.GetValue(svgTextBase,
                                             svgTextBase.FontSize);

      startY -= fontSize / this.GetLineHeightFactor(svgTextBase);

      this.ApplyMatrixOnPoint(startX,
                              startY,
                              matrix,
                              out startX,
                              out startY);

      fontSize = this.ApplyMatrixOnLength(fontSize,
                                          matrix);
    }
  }
}