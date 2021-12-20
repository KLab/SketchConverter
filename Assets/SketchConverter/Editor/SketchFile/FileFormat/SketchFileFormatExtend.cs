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

using System;
using System.Collections.Generic;
using System.Linq;

namespace SketchConverter.FileFormat
{
    /// <summary>
    /// 通常のレイヤー情報
    /// </summary>
    public partial class OriginalMasterLayer : ILayer
    {
    }

    /// <summary>
    /// Page の根となるレイヤー情報
    /// </summary>
    public partial class PageLayer : ILayer
    {
    }

    /// <summary>
    /// SymbolMaster の根となるレイヤー
    /// </summary>
    public partial class SymbolSourceLayer : ILayer
    {
    }

    /// <summary>
    /// レイヤーの共通情報インターフェース
    /// </summary>
    public interface ILayer
    {
        long BooleanOperation { get; set; }
        dynamic Class { get; set; }
        long? ClippingMaskMode { get; set; }
        string DoObjectId { get; set; }
        ExportOptions ExportOptions { get; set; }
        FlowConnection Flow { get; set; }
        Rect Frame { get; set; }
        GroupLayout GroupLayout { get; set; }
        bool? HasClippingMask { get; set; }
        bool IsFixedToViewport { get; set; }
        bool IsFlippedHorizontal { get; set; }
        bool IsFlippedVertical { get; set; }
        bool IsLocked { get; set; }
        bool IsVisible { get; set; }
        long LayerListExpandedType { get; set; }
        OriginalMasterLayer[] Layers { get; set; }
        bool? MaintainScrollPosition { get; set; }
        string Name { get; set; }
        bool NameIsFixed { get; set; }
        long ResizingConstraint { get; set; }
        long ResizingType { get; set; }
        double Rotation { get; set; }
        string SharedStyleId { get; set; }
        bool ShouldBreakMaskChain { get; set; }
        Style Style { get; set; }
        string SymbolId { get; set; }
        Dictionary<string, dynamic> UserInfo { get; set; }
    }

    /// <summary>
    /// Parse 済みの override 情報
    /// </summary>
    public class ParsedOverrideValue
    {
        public static string PropertyKeywordText = "stringValue";
        public static string PropertyKeywordSymbol = "symbolID";
        public static string PropertyKeywordImage = "image";
        public static string PropertyKeywordLayerStyle = "layerStyle";
        public static string PropertyKeywordFillColor = "fillColor";

        readonly OverrideValue overrideValue;
        public string OverrideObjectId { get; }
        public string OverrideProperty { get; }
        public Value Value => overrideValue.Value;

        public ParsedOverrideValue(OverrideValue value)
        {
            overrideValue = value;
            var array1 = value.OverrideName.Split('/');
            var array2 = array1.Last().Split('_');
            OverrideObjectId = array2[0];
            OverrideProperty = array2[1];
        }
    }

    /// <summary>
    /// Sketch のカラー情報
    /// </summary>
    public partial class Color
    {
        /// <summary>
        /// 同一の色か判定する
        /// </summary>
        public static bool Equality(Color a, Color b)
        {
            if (ReferenceEquals(a, b))
            {
                return true;
            }
            if (a == null || b == null)
            {
                return false;
            }
            const double equalsPrecision = 0.0000001;
            return Math.Abs(a.Red - b.Red) < equalsPrecision &&
                   Math.Abs(a.Green - b.Green) < equalsPrecision &&
                   Math.Abs(a.Blue - b.Blue) < equalsPrecision &&
                   Math.Abs(a.Alpha - b.Alpha) < equalsPrecision;
        }
    }

    /// <summary>
    /// Sketch の ParagraphStyle 情報
    /// </summary>
    public partial class ParagraphStyle
    {
        public TextHorizontalAlignment AlignmentEnum => (TextHorizontalAlignment) (Alignment ?? 0);
    }

    /// <summary>
    /// Sketch の TextStyle 情報
    /// </summary>
    public partial class TextStyle
    {
        public TextVerticalAlignment VerticalAlignmentEnum => (TextVerticalAlignment) VerticalAlignment;
    }

    /// <summary>
    /// Sketch のスキーマ情報から生成されたクラスに対する拡張メソッド群
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// アートボードを持っている Page か判定
        /// </summary>
        public static bool HasArtboard(this PageLayer pageLayer)
        {
            foreach (var layer in pageLayer.Layers)
            {
                if (layer.IsClass(ClassText.Artboard) || layer.IsClass(ClassText.SymbolMaster))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// レイヤーが引数のクラスかどうか判定
        /// </summary>
        /// <param name="className">クラス名。ClassText クラスに名前群が定義されています</param>
        public static bool IsClass(this ILayer layer, string className) => layer.Class == className;

        /// <summary>
        /// 再帰的に全てのレイヤーを得る
        /// </summary>
        public static IEnumerable<OriginalMasterLayer> DescendantsAndSelf(this OriginalMasterLayer layer)
        {
            if (layer == null)
            {
                yield break;
            }
            yield return layer;
            if (layer.Layers == null)
            {
                yield break;
            }
            foreach (var child in layer.Layers)
            {
                yield return child;
                foreach (var descendant in child.DescendantsAndSelf())
                {
                    yield return descendant;
                }
            }
        }

        /// <summary>
        /// Sketch の Color を UnityEngine の Color に変換
        /// </summary>
        public static UnityEngine.Color ToUnityColor(this Color color) =>
            new UnityEngine.Color((float) color.Red, (float) color.Green, (float) color.Blue, (float) color.Alpha);

        /// <summary>
        /// Resizing 情報となるビットフラグを返す
        /// </summary>
        public static ResizingConstraint ResizingConstraintEnum(this ILayer layer)
        {
            // memo: https://github.com/html-sketchapp/html-sketchapp/pull/90
            var resizingConstraint = layer.ResizingConstraint ^ long.MaxValue;
            return (ResizingConstraint) resizingConstraint;
        }

        /// <summary>
        /// 引数の Resizing 設定がされているかどうかを返す 
        /// </summary>
        public static bool HasResizingConstraint(this ILayer layer, ResizingConstraint resizing) => (layer.ResizingConstraintEnum() & resizing) == resizing;

        /// <summary>
        /// パース済みの override 情報群を返す
        /// </summary>
        public static ParsedOverrideValue[] GetParsedOverrideValues(this OriginalMasterLayer layer)
        {
            return layer?.OverrideValues.Select(x => new ParsedOverrideValue(x)).ToArray() ?? Enumerable.Empty<ParsedOverrideValue>().ToArray();
        }
    }
}
