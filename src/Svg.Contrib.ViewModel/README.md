![Icon](assets/icon.png)

# dotnet-Svg.Contrib.ViewModel

This project creates *ViewModel*-classes for your included [SVG](https://en.wikipedia.org/wiki/Scalable_Vector_Graphics) files.

*Clarification: Actually the ViewModel is a Controller, but ViewModel sounds more appealing.*

## Installing

[![NuGet Status](http://img.shields.io/nuget/v/Svg.Contrib.ViewModel.svg?style=flat-square)](https://www.nuget.org/packages/Svg.Contrib.ViewModel/) https://www.nuget.org/packages/Svg.Contrib.ViewModel/

    PM> Install-Package Svg.Contrib.ViewModel

## Example

![](assets/screenshot1.PNG)

    PM> Install-Package Svg.Contrib.ViewModel

![](assets/screenshot2.PNG)

    var label = new label();
    label.LabelInfo = "some text";
    label.Visible_LabelInfo = false;
    label.Barcode_RouteBc = "1234567890";
    var svgDocument = label.SvgDocument;

## Features

- `SvgVisualElement.Visible`
- `SvgTextBase.Text`
- `SvgElement.CustomAttributes("data-barcode")`

## License

dotnet-Svg.Contrib.ViewModel is published under [WTFNMFPLv3](https://github.com/dittodhole/WTFNMFPLv3)

## Icon

[Generator](https://thenounproject.com/term/generator/7266/) by [john trillana](https://thenounproject.com/claxxmoldii) from the Noun Project
