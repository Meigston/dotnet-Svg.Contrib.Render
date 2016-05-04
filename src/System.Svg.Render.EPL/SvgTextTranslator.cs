﻿using System.Drawing;
using System.Linq;
using System.Svg.Transforms;

namespace System.Svg.Render.EPL
{
  public class SvgTextTranslator : SvgElementTranslator<SvgText>
  {
    public SvgTextTranslator(SvgUnitCalculator svgUnitCalculator)
    {
      if (svgUnitCalculator == null)
      {
        // TODO add documentation
        throw new ArgumentNullException(nameof(svgUnitCalculator));
      }

      this.SvgUnitCalculator = svgUnitCalculator;
    }

    private SvgUnitCalculator SvgUnitCalculator { get; }

    private bool IsTransformationAllowed(Type type)
    {
      if (type == typeof(SvgMatrix))
      {
        return true;
      }
      if (type == typeof(SvgRotate))
      {
        return true;
      }
      if (type == typeof(SvgScale))
      {
        return true;
      }
      if (type == typeof(SvgTranslate))
      {
        return true;
      }
      return false;
    }

    public override object Translate(SvgText instance,
                                     int targetDpi)
    {
      // TODO add multiline translation
      // TODO add lineHeight translation

      var x = instance.X.First();
      var y = instance.Y.First();

      var svgUnitType = this.SvgUnitCalculator.CheckSvgUnitType(x,
                                                                y);
      var startPoint = new PointF(this.SvgUnitCalculator.GetValue(x),
                                  this.SvgUnitCalculator.GetValue(y));

      var rotationTranslation = default(object);
      foreach (var transformation in instance.Transforms)
      {
        var transformationType = transformation.GetType();
        if (!this.IsTransformationAllowed(transformationType))
        {
          // TODO add logging
          return null;
        }

        // TODO fix rotationTranslation for multiple transformations
        var matrix = transformation.Matrix;
        if (!this.SvgUnitCalculator.TryApplyMatrixTransformation(matrix,
                                                                 ref startPoint,
                                                                 out rotationTranslation))
        {
          // TODO add logging
          return null;
        }
      }

      if (rotationTranslation == null)
      {
        rotationTranslation = this.SvgUnitCalculator.GetRotationTranslation(SvgUnitCalculator.Rotation.None);
      }

      var horizontalStart = this.SvgUnitCalculator.GetDevicePoints(startPoint.X,
                                                                   svgUnitType,
                                                                   targetDpi);
      var verticalStart = this.SvgUnitCalculator.GetDevicePoints(startPoint.Y,
                                                                 svgUnitType,
                                                                 targetDpi);

      // TODO here comes the magic!
      var fontSelection = 1;
      // VALUE    203dpi        300dpi
      // ==================================
      //  1       20.3cpi       25cpi
      //          6pts          4pts
      //          8x12 dots     12x20 dots
      // ==================================
      //  2       16.9cpi       18.75cpi
      //          7pts          6pts
      //          10x16 dots    16x28 dots
      // ==================================
      //  3       14.5cpi       15cpi
      //          10pts         8pts
      //          12x20 dots    20x36 dots
      // ==================================
      //  4       12.7cpi       12.5cpi
      //          12pts         10pts
      //          14x24 dots    24x44 dots
      // ==================================
      //  5       5.6cpi        6.25cpi
      //          24pts         21pts
      //          32x48 dots    48x80 dots
      // ==================================

      var horizontalMultiplier = 1; // Accepted Values: 1–6, 8
      var verticalMultiplier = 1; // Accepted Values: 1–9

      string reverseImage;
      if ((instance.Fill as SvgColourServer)?.Colour == Color.White)
      {
        reverseImage = "R";
      }
      else
      {
        reverseImage = "N";
      }

      var text = instance.Text;

      var translation = $@"A{horizontalStart},{verticalStart},{rotationTranslation},{fontSelection},{horizontalMultiplier},{verticalMultiplier},{reverseImage},""{text}""";

      return translation;
    }
  }
}