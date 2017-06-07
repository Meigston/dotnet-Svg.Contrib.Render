﻿namespace Svg.Contrib.Render.FingerPrint
{
  public enum Direction
  {
    Direction1 = 1,
    Direction2 = 2,
    Direction3 = 3,
    Direction4 = 4,
    LeftToRight = Direction.Direction1,
    TopToBottom = Direction.Direction2,
    RightToLeft = Direction.Direction3,
    BottomToTop = Direction.Direction4
  }

  public enum Alignment
  {
    TopLeft = 7,
    TopMiddle = 8,
    TopRight = 9,
    BaseLineLeft = 4,
    BaseLineMiddle = 5,
    BaseLineRight = 6,
    BottomLeft = 1,
    BottomMiddle = 2,
    BottomRight = 3
  }

  public enum CharacterSet
  {
    Utf8 = 8
  }

  public enum BarCodeType
  {
    Code128,
    Code128A,
    Code128B,
    Code128C,
    Interleaved2Of5,
    Code39,
    Code39FullAscii,
    Code39WithChecksum
  }
}
