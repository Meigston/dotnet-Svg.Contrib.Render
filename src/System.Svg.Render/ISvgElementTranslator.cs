﻿using System.Drawing.Drawing2D;
using JetBrains.Annotations;

namespace System.Svg.Render
{
  [PublicAPI]
  public interface ISvgElementTranslator<TContainer>
  {
    void Translate([NotNull] SvgElement svgElement,
                   [NotNull] Matrix matrix,
                   [NotNull] TContainer container);
  }

  [PublicAPI]
  public interface ISvgElementTranslator<TContainer, TSvgElement> : ISvgElementTranslator<TContainer>
    where TSvgElement : SvgElement
  {
    void Translate([NotNull] TSvgElement svgElement,
                   [NotNull] Matrix matrix,
                   [NotNull] TContainer container);
  }
}