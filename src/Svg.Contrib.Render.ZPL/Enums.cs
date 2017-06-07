namespace Svg.Contrib.Render.ZPL
{
  public enum LineColor
  {
    Black = 'B',
    White = 'W'
  }

  public enum FieldOrientation
  {
    Normal = 'N',
    RotatedBy90Degrees = 'R',
    RotatedBy180Degrees = 'I',
    RotatedBy270Degrees = 'B'
  }

  public enum PrintOrientation
  {
    Normal = 'N',
    Invert = 'I'
  }

  public enum CharacterSet
  {
    ZebraCodePage850 = 13,
    ZebraCodePage1252 = 27
  }

  public enum PrintInterpretationLine
  {
    Yes = 'Y',
    No = 'N'
  }

  public enum PrintInterpretationLineAboveCode
  {
    Yes = 'Y',
    No = 'N'
  }

  public enum Mode
  {
    NoSelectedMode = 'N',
    UccCaseMode = 'U',
    AutomaticMode = 'A',
    UccEanMode = 'D'
  }

  public enum CalculateAndPrintMod10CheckDigit
  {
    Yes = 'Y',
    No = 'N'
  }

  public enum UccCheckDigit
  {
    Yes = 'Y',
    No = 'N'
  }

  public enum Mod43Check
  {
    Yes = 'Y',
    No = 'N'
  }
}
