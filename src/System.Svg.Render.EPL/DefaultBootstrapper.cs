﻿using JetBrains.Annotations;

namespace System.Svg.Render.EPL
{
  public static class DefaultBootstrapper
  {
    [NotNull]
    public static EplRenderer Create(float sourceDpi,
                                     float targetDpi,
                                     PrinterCodepage printerCodepage,
                                     int countryCode)
    {
      var svgUnitReader = new SvgUnitReader();
      var eplTransformer = new EplTransformer(svgUnitReader);
      var viewMatrix = eplTransformer.CreateViewMatrix(sourceDpi,
                                                       targetDpi);
      var eplRenderer = new EplRenderer(viewMatrix,
                                        printerCodepage,
                                        countryCode);

      var encoding = eplRenderer.GetEncoding();

      var eplCommands = new EplCommands(encoding);

      {
        var svgLineTranslator = new SvgLineTranslator(eplTransformer,
                                                      eplCommands);

        var svgRectangleTranslator = new SvgRectangleTranslator(eplTransformer,
                                                                eplCommands);

        var svgTextTranslator = new SvgTextBaseTranslator<SvgText>(eplTransformer,
                                                                   eplCommands);
        var svgTextSpanTranslator = new SvgTextBaseTranslator<SvgTextSpan>(eplTransformer,
                                                                           eplCommands);

        var svgPathTranslator = new SvgPathTranslator(eplTransformer,
                                                      eplCommands);

        var svgImageTranslator = new SvgImageTranslator(eplTransformer,
                                                        eplCommands);

        eplRenderer.RegisterTranslator(svgLineTranslator);
        eplRenderer.RegisterTranslator(svgRectangleTranslator);
        eplRenderer.RegisterTranslator(svgTextTranslator);
        eplRenderer.RegisterTranslator(svgTextSpanTranslator);
        eplRenderer.RegisterTranslator(svgPathTranslator);
        eplRenderer.RegisterTranslator(svgImageTranslator);
      }

      return eplRenderer;
    }
  }
}