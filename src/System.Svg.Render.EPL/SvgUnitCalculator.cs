﻿using System.Drawing;
using System.Drawing.Drawing2D;
using Anotar.LibLog;

namespace System.Svg.Render.EPL
{
  public class SvgUnitCalculator
  {
    // TODO add reading for different origin

    public int SourceDpi { get; set; } = 72;
    public SvgUnitType UserUnitTypeSubstitution { get; set; } = SvgUnitType.Pixel;

    /// <exception cref="ArgumentException">If <see cref="SvgUnitType" /> of <paramref name="svgUnit1" /> and <paramref name="svgUnit2" /> are not matching.</exception>
    public virtual SvgUnit Add(SvgUnit svgUnit1,
                               SvgUnit svgUnit2)
    {
      var svgUnitType = this.CheckSvgUnitType(svgUnit1,
                                              svgUnit2);

      var val1 = this.GetValue(svgUnit1);
      var val2 = this.GetValue(svgUnit2);
      var value = val1 + val2;

      var result = new SvgUnit(svgUnitType,
                               value);

      return result;
    }

    /// <exception cref="ArgumentException">If <see cref="SvgUnitType" /> of <paramref name="svgUnit1" /> and <paramref name="svgUnit2" /> are not matching.</exception>
    public virtual SvgUnit Substract(SvgUnit svgUnit1,
                                     SvgUnit svgUnit2)
    {
      var svgUnitType = this.CheckSvgUnitType(svgUnit1,
                                              svgUnit2);

      var val1 = this.GetValue(svgUnit1);
      var val2 = this.GetValue(svgUnit2);
      var value = val1 - val2;

      var result = new SvgUnit(svgUnitType,
                               value);

      return result;
    }

    /// <exception cref="ArgumentException">If <see cref="SvgUnitType" /> of <paramref name="svgUnit1" /> and <paramref name="svgUnit2" /> are not matching.</exception>
    public virtual SvgUnitType CheckSvgUnitType(SvgUnit svgUnit1,
                                                SvgUnit svgUnit2)
    {
      if (svgUnit1.Type != svgUnit2.Type)
      {
        throw new ArgumentException($"{nameof(svgUnit2)}'s {nameof(SvgUnit.Type)} ({svgUnit2.Type}) does not equal {nameof(svgUnit1)}'s {nameof(SvgUnit.Type)} ({svgUnit1.Type})");
      }

      return svgUnit1.Type;
    }

    public bool IsValueZero(SvgUnit svgUnit)
    {
      // TODO find a good TOLERANCE
      return Math.Abs(svgUnit.Value) < 0.5f;
    }

    public float GetValue(SvgUnit svgUnit)
    {
      var result = svgUnit.Value;

      return result;
    }

    public bool TryGetDevicePoints(SvgUnit svgUnit,
                                   int targetDpi,
                                   out int devicePoints)
    {
      var value = this.GetValue(svgUnit);
      var svgUnitType = svgUnit.Type;

      var result = this.TryGetDevicePoints(value,
                                           svgUnitType,
                                           targetDpi,
                                           out devicePoints);

      return result;
    }

    public virtual bool TryGetDevicePoints(float value,
                                           SvgUnitType svgUnitType,
                                           int targetDpi,
                                           out int devicePoints)
    {
      if (svgUnitType == SvgUnitType.User)
      {
        svgUnitType = this.UserUnitTypeSubstitution;
      }

      float? inches;
      if (svgUnitType == SvgUnitType.Inch)
      {
        inches = value;
      }
      else if (svgUnitType == SvgUnitType.Centimeter)
      {
        inches = value / 2.54f;
      }
      else if (svgUnitType == SvgUnitType.Millimeter)
      {
        inches = value / 10f / 2.54f;
      }
      else if (svgUnitType == SvgUnitType.Point)
      {
        inches = value / 72f;
      }
      else if (svgUnitType == SvgUnitType.Pica)
      {
        inches = value / 10f / 72f;
      }
      else
      {
        inches = null;
      }

      float pixels;
      if (svgUnitType == SvgUnitType.Pixel)
      {
        pixels = value;
      }
      else if (inches.HasValue)
      {
        pixels = inches.Value * this.SourceDpi;
      }
      else
      {
        devicePoints = 0;
        return false;
      }

      devicePoints = (int) (pixels / this.SourceDpi * targetDpi);

      return true;
    }

    public enum Rotation
    {
      None,
      Rotate90,
      Rotate180,
      Rotate270
    }

    public bool TryApplyMatrixTransformation(Matrix matrix,
                                             ref PointF startPoint,
                                             out object rotationTranslation)
    {
      if (matrix == null)
      {
        LogTo.Error($"{nameof(matrix)} is null");
        rotationTranslation = null;
        return false;
      }

      var endPoint = new PointF
                     {
                       X = startPoint.X + 10,
                       Y = startPoint.Y
                     };

      var points = new[]
                   {
                     startPoint,
                     endPoint
                   };
      matrix.TransformPoints(points);

      startPoint = points[0];
      endPoint = points[1];

      Rotation rotation;

      // TODO find a good tolerance
      if (Math.Abs(startPoint.Y - endPoint.Y) < 0.5f)
      {
        if (startPoint.X < endPoint.X)
        {
          rotation = Rotation.None;
        }
        else if (startPoint.X > endPoint.X)
        {
          rotation = Rotation.Rotate180;
        }
        else
        {
          LogTo.Error($"HAMMER TIME - singularity detected ({startPoint.X}/{startPoint.Y}, {endPoint.X}/{endPoint.Y})");
          rotationTranslation = null;
          return false;
        }
      }
      else if (startPoint.Y > endPoint.Y)
      {
        rotation = Rotation.Rotate90;
      }
      else if (startPoint.Y < endPoint.Y)
      {
        rotation = Rotation.Rotate270;
      }
      else
      {
        LogTo.Error($"HAMMER TIME - singularity detected ({startPoint.X}/{startPoint.Y}, {endPoint.X}/{endPoint.Y})");
        rotationTranslation = null;
        return false;
      }

      try
      {
        rotationTranslation = this.GetRotationTranslation(rotation);
      }
      catch (ArgumentOutOfRangeException argumentOutOfRangeException)
      {
        LogTo.ErrorException($"could not get rotation translation for {rotation}",
                             argumentOutOfRangeException);
        rotationTranslation = null;
        return false;
      }

      return true;
    }

    /// <exception cref="ArgumentOutOfRangeException">If <paramref name="rotation" /> cannot be translated.</exception>
    protected internal virtual object GetRotationTranslation(Rotation rotation)
    {
      switch (rotation)
      {
        case Rotation.None:
          return 0;
        case Rotation.Rotate90:
          return 1;
        case Rotation.Rotate180:
          return 2;
        case Rotation.Rotate270:
          return 3;
        default:
          throw new ArgumentOutOfRangeException(nameof(rotation),
                                                rotation,
                                                null);
      }
    }
  }
}