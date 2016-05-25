﻿using JetBrains.Annotations;

namespace Svg.Contrib.Render.EPL.Tests
{
  public class EplTransformer : EPL.EplTransformer
  {
    public EplTransformer([NotNull] SvgUnitReader svgUnitReader)
      : base(svgUnitReader,
             EPL.EplTransformer.DefaultOutputWidth,
             EPL.EplTransformer.DefaultOutputHeight) {}

    protected override float GetLineHeightFactor([NotNull] SvgTextBase svgTextBase) => 1f;
  }
}