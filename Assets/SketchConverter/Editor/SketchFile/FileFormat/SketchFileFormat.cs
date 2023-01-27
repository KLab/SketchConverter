/**
 * SketchConverter
 * Copyright(c) 2020 KLab, Inc. All Rights Reserved.
 * Proprietary and Confidential - This source code is not for redistribution
 *
 * Subject to the prior written consent of KLab, Inc(Licensor) and its terms and
 * conditions, Licensor grants to you, and you hereby accept nontransferable,
 * nonexclusive limited right to access, obtain, use, copy and/or download
 * a copy of this product only for requirement purposes. You may not rent,
 * lease, loan, time share, sublicense, transfer, make generally available,
 * license, disclose, disseminate, distribute or otherwise make accessible or
 * available this product to any third party without the prior written approval
 * of Licensor. Unauthorized copying of this product, including modifications 
 * of this product or programs in which this product has been merged or included
 * with other software products is expressly forbidden.
 */

//---------------------------------------------
// SketchFileFormat.designer.cs の自動生成の対象外となる型をこのファイルに定義する
// 単純に利便性を高めるような実装はこちらのファイルには定義しない 
//---------------------------------------------

using System;
using System.Runtime.InteropServices;

namespace SketchConverter.FileFormat
{
    /// <summary>
    /// Sketch 内で扱われている型を表現する文字列の一覧クラス
    /// </summary>
    /*
     * このクラスに定義されている文字列一覧を得る方法について。
     * 下記のコマンドを sketch-file-format リポジトリで実行すると型一覧が得られる。
     * git grep '_class: { const:' -- 'packages/file-format/schema' | awk '{sub(".*:", "");sub("^ ","");sub(" }$","");print $0;}' | sort --ignore-case | uniq
     */
    public static class ClassText
    {
        public static string Artboard = "artboard";
        public static string AssetCollection = "assetCollection";
        public static string AttributedString = "attributedString";
        public static string Bitmap = "bitmap";
        public static string Blur = "blur";
        public static string Border = "border";
        public static string BorderOptions = "borderOptions";
        public static string Color = "color";
        public static string ColorControls = "colorControls";
        public static string CurvePoint = "curvePoint";
        public static string Document = "document";
        public static string ExportFormat = "exportFormat";
        public static string ExportOptions = "exportOptions";
        public static string Fill = "fill";
        public static string FontDescriptor = "fontDescriptor";
        public static string FontReference = "fontReference";
        public static string Gradient = "gradient";
        public static string GradientStop = "gradientStop";
        public static string GraphicsContextSettings = "graphicsContextSettings";
        public static string Group = "group";
        public static string ImageCollection = "imageCollection";
        public static string InnerShadow = "innerShadow";
        public static string LayoutGrid = "layoutGrid";
        public static string MSImmutableColorAsset = "MSImmutableColorAsset";
        public static string MSImmutableFlowConnection = "MSImmutableFlowConnection";
        public static string MSImmutableForeignLayerStyle = "MSImmutableForeignLayerStyle";
        public static string MSImmutableForeignSwatch = "MSImmutableForeignSwatch";
        public static string MSImmutableForeignSymbol = "MSImmutableForeignSymbol";
        public static string MSImmutableForeignTextStyle = "MSImmutableForeignTextStyle";
        public static string MSImmutableFreeformGroupLayout = "MSImmutableFreeformGroupLayout";
        public static string MSImmutableGradientAsset = "MSImmutableGradientAsset";
        public static string MSImmutableHotspotLayer = "MSImmutableHotspotLayer";
        public static string MSImmutableInferredGroupLayout = "MSImmutableInferredGroupLayout";
        public static string MSImmutableOverrideProperty = "MSImmutableOverrideProperty";
        public static string MSImmutablePatchInfo = "MSImmutablePatchInfo";
        public static string MSJSONFileReference = "MSJSONFileReference";
        public static string MSJSONOriginalDataReference = "MSJSONOriginalDataReference";
        public static string Oval = "oval";
        public static string OverrideValue = "overrideValue";
        public static string Page = "page";
        public static string ParagraphStyle = "paragraphStyle";
        public static string Polygon = "polygon";
        public static string Rect = "rect";
        public static string Rectangle = "rectangle";
        public static string RulerData = "rulerData";
        public static string Shadow = "shadow";
        public static string ShapeGroup = "shapeGroup";
        public static string ShapePath = "shapePath";
        public static string SharedStyle = "sharedStyle";
        public static string SharedStyleContainer = "sharedStyleContainer";
        public static string SharedTextStyleContainer = "sharedTextStyleContainer";
        public static string SimpleGrid = "simpleGrid";
        public static string Slice = "slice";
        public static string Star = "star";
        public static string StringAttribute = "stringAttribute";
        public static string Style = "style";
        public static string Swatch = "swatch";
        public static string SwatchContainer = "swatchContainer";
        public static string SymbolContainer = "symbolContainer";
        public static string SymbolInstance = "symbolInstance";
        public static string SymbolMaster = "symbolMaster";
        public static string Text = "text";
        public static string TextStyle = "textStyle";
        public static string Triangle = "triangle";
    }

    /// <summary>
    /// Sketch の Alignment 設定
    /// </summary>
    public enum TextHorizontalAlignment
    {
        Left,
        Right,
        Centered,
        Justified,
        Natural,
    }

    /// <summary>
    /// Sketch の Alignment 設定
    /// </summary>
    public enum TextVerticalAlignment
    {
        Top,
        Middle,
        Bottom,
    }

    /// <summary>
    /// Sketch の FillType 設定
    /// </summary>
    public enum FillType
    {
        Color = 0,
        Gradient = 1,
        Pattern = 4,
    }

    /// <summary>
    /// Sketch の PatternFillType 設定
    /// </summary>
    public enum PatternFillType
    {
        Tile,
        Fill,
        Stretch,
        Fit,
    }

    /// <summary>
    /// Sketch の GradientType
    /// </summary>
    public enum GradientType
    {
        Linear,
        Radial,
        Angular,
    }

    /// <summary>
    /// Sketch の Resizing 設定
    /// </summary>
    public enum ResizeType
    {
        Stretch,
        PinToEdge,
        Resize,
        Float,
    }

    /// <summary>
    /// Sketch の Resizing 設定
    /// </summary>
    [Flags]
    public enum ResizingConstraint
    {
        None = 0,
        Right = 1 << 0,
        Width = 1 << 1,
        Left = 1 << 2,
        Bottom = 1 << 3,
        Height = 1 << 4,
        Top = 1 << 5,
    }

    /// <summary>
    /// 主に override の為の定義
    /// </summary>
    [StructLayout(LayoutKind.Auto)]
    public partial struct Value
    {
        public Color Color;
        public static implicit operator Value(Color Color) => new Value {Color = Color};
    }
}
