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
        public bool GetAllowsOverrides() => AllowsOverrides ?? true;
    }

    /// <summary>
    /// Page の根となるレイヤー情報
    /// </summary>
    public partial class PageLayer : ILayer
    {
        public bool GetAllowsOverrides() => AllowsOverrides ?? true;
    }

    /// <summary>
    /// SymbolMaster の根となるレイヤー
    /// </summary>
    public partial class SymbolSourceLayer : ILayer
    {
        public bool GetAllowsOverrides() => AllowsOverrides;
    }

    /// <summary>
    /// レイヤーの共通情報インターフェース
    /// </summary>
    public interface ILayer
    {
        long BooleanOperation { get; }
        dynamic Class { get; }
        long? ClippingMaskMode { get; }
        string DoObjectId { get; }
        ExportOptions ExportOptions { get; }
        FlowConnection Flow { get; }
        Rect Frame { get; }
        GroupLayout GroupLayout { get; }
        bool? HasClippingMask { get; }
        bool IsFixedToViewport { get; }
        bool IsFlippedHorizontal { get; }
        bool IsFlippedVertical { get; }
        bool IsLocked { get; }
        bool IsVisible { get; }
        long LayerListExpandedType { get; }
        OriginalMasterLayer[] Layers { get; }
        bool? MaintainScrollPosition { get; }
        string Name { get; }
        bool NameIsFixed { get; }
        long ResizingConstraint { get; }
        long ResizingType { get; }
        double Rotation { get; }
        string SharedStyleId { get; }
        bool ShouldBreakMaskChain { get; }
        Style Style { get; }
        string SymbolId { get; }
        Dictionary<string, dynamic> UserInfo { get; }
        OverrideProperty[] OverrideProperties { get; }
        bool GetAllowsOverrides();
    }

    /// <summary>
    /// Parse 済みの override 情報
    /// </summary>
    public class ParsedOverrideValue
    {
        public static readonly string PropertyKeywordText = "stringValue";
        public static readonly string PropertyKeywordSymbol = "symbolID";
        public static readonly string PropertyKeywordImage = "image";
        public static readonly string PropertyKeywordLayerStyle = "layerStyle";
        public static readonly string PropertyKeywordTextStyle = "textStyle";
        public static readonly string PropertyKeywordFillColor = "fillColor";

        readonly OverrideValue overrideValue;

        /// <summary>どのオブジェクトに対してオーバーライドするかを示すID</summary>
        public string OverrideObjectId { get; }

        /// <summary>オーバーライド先のレイヤーに到達するまでのシンボルインスタンスID</summary>
        public string[] OverrideSymbolIds { get; }

        /// <summary>何をオーバーライドするのか示す文字列（stringValue, symbolID, fillColor, etc.）</summary>
        public string OverrideProperty { get; }

        /// <summary>オーバーライド値</summary>
        public Value Value => overrideValue.Value;

        /// <summary>素のオーバーライド情報文字列</summary>
        public string OverrideName => overrideValue.OverrideName;

        public ParsedOverrideValue(OverrideValue value)
        {
            overrideValue = value;

            // OverrideName には「どのインスタンスシンボルの中の/どのレイヤーに対して_何をオーバーライドするのか」という形で入っている
            // インスタンスシンボル内のインスタンスシンボルを指定するために、三階層以上になることもある
            // 短い例: 4AB528C0-F897-4F6D-B591-EB4DE4B1D390_symbolID
            // 長い例: 4AB528C0-F897-4F6D-B591-EB4DE4B1D390/866ED934-7423-4717-9D02-7C31E89E6DC9/5E2E8126-7ACB-407C-A3C1-061855E5C09E_symbolID
            var array = overrideValue.OverrideName.Split('_');
            var ids = array.First().Split('/');
            OverrideSymbolIds = ids.Take(ids.Length - 1).ToArray(); // -1 = OverrideObjectId
            OverrideObjectId = ids.Last();
            OverrideProperty = array.Last();
        }
    }

    /// <summary>
    /// Parse 済みの Property 情報
    /// </summary>
    public class ParsedOverrideProperty
    {
        readonly OverrideProperty overrideProperty;

        public string OverrideObjectPath { get; }
        public bool CanOverride => overrideProperty.CanOverride;
        public string OverrideName => overrideProperty.OverrideName;

        public ParsedOverrideProperty(OverrideProperty property)
        {
            overrideProperty = property;
            OverrideObjectPath = property.OverrideName.Split('_').First();
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

    public partial class Gradient
    {
        public (float x, float y) GetParsedTo() => Parse(To);
        public (float x, float y) GetParsedFrom() => Parse(From);

        /// <summary>
        /// こんな形のテキストをパースする: "{0.49999999999999967, -2.4894981252573991e-17}"
        /// </summary>
        (float x, float y) Parse(string s)
        {
            var array = s.Substring(1, s.Length - 2).Split(',');
            return (x: float.Parse(array[0]), y: float.Parse(array[1]));
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
    /// Sketch の Fill 情報
    /// </summary>
    public partial class Fill
    {
        public FillType FillTypeEnum => (FillType) FillType;
        public PatternFillType PatternFillTypeEnum => (PatternFillType) PatternFillType;
    }

    /// <summary>
    /// Sketch の Border 情報
    /// </summary>
    public partial class Border
    {
        public FillType FillTypeEnum => (FillType) FillType;
    }

    /// <summary>
    /// Sketch の Gradient 情報
    /// </summary>
    public partial class Gradient
    {
        public GradientType GradientTypeEnum => (GradientType) GradientType;
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
        public static UnityEngine.Color ToUnityColor(this Color color) => new UnityEngine.Color((float) color.Red, (float) color.Green, (float) color.Blue, (float) color.Alpha);

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
            return layer?.OverrideValues.Select(x => new ParsedOverrideValue(x)).ToArray() ?? Array.Empty<ParsedOverrideValue>();
        }

        /// <summary>
        /// パース済みの override 情報群を返す
        /// </summary>
        public static ParsedOverrideProperty[] GetParsedOverrideProperties(this ILayer layer)
        {
            return layer?.OverrideProperties?.Select(x => new ParsedOverrideProperty(x)).ToArray() ?? Array.Empty<ParsedOverrideProperty>();
        }
    }
}
