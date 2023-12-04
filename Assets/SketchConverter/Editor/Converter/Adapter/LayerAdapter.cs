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
using SketchConverter.FileFormat;

namespace SketchConverter
{
    /// <summary>
    /// 階層構造における1階層あたりの情報
    /// </summary>
    public interface ILayerAdapter
    {
        /// <summary>ルート情報</summary>
        Adapter Adapter { get; }

        /// <summary>素のレイヤー情報。基本的には OriginalMasterLayer 型だが PageLayer 型の場合もある。</summary>
        ILayer Layer { get; }

        /// <summary>子のレイヤー情報</summary>
        OriginalMasterLayer[] Layers { get; }

        /// <summary>素のオリジナルのシンボルレイヤー情報。対象のレイヤーがシンボルインスタンスでない場合は null です</summary>
        ILayer SymbolLayer { get; }

        /// <summary>子の LayerAdapter 情報</summary>
        List<ILayerAdapter> LayerAdapters { get; }

        /// <summary>オーバーライド情報を解決した Layer.AttributedString.String</summary>
        string LayerAttributedStringString { get; }

        /// <summary>オーバーライド情報を解決した Layer.SymbolId</summary>
        string LayerSymbolId { get; }

        /// <summary>オーバーライド情報を解決した Layer.Style</summary>
        Style LayerStyle { get; }

        /// <summary>オーバーライド情報を解決した Layer.Style.TextStyle</summary>
        TextStyle LayerStyleTextStyle { get; }

        /// <summary>オーバーライド情報を解決した Layer.SharedStyleId</summary>
        string SharedStyleId { get; }

        /// <summary>オーバーライド情報を解決した Layer.Style の Fill Color で有効な色の配列</summary>
        Color[] AvailableFillColors { get; }
    }

    /// <inheritdoc/>
    public class LayerAdapter : ILayerAdapter
    {
        /// <inheritdoc/>
        public Adapter Adapter { get; }

        /// <inheritdoc/>
        public ILayer Layer { get; }

        /// <inheritdoc/>
        public ILayer SymbolLayer { get; }

        /// <inheritdoc/>
        public List<ILayerAdapter> LayerAdapters { get; } = new List<ILayerAdapter>();

        /// <inheritdoc/>
        public OriginalMasterLayer[] Layers => (SymbolLayer ?? Layer).Layers;

        /// <inheritdoc/>
        public string LayerAttributedStringString => GetOverrideValue(ParsedOverrideValue.PropertyKeywordText)?.Value.String ??
                                                     (Layer as OriginalMasterLayer)?.AttributedString.String ?? "";

        /// <inheritdoc/>
        public string LayerSymbolId
        {
            get
            {
                var overrideValue = GetOverrideValue(ParsedOverrideValue.PropertyKeywordSymbol);
                return overrideValue != null ? overrideValue.Value.String : Layer.SymbolId;
            }
        }

        /// <inheritdoc/>
        public Color[] AvailableFillColors
        {
            get
            {
                var overrideValue = GetOverrideValue(ParsedOverrideValue.PropertyKeywordFillColor);
                if (overrideValue == null)
                {
                    return LayerStyle?.Fills?
                        .Where(x => x.Class == ClassText.Fill)
                        .Where(x => x.IsEnabled)
                        .Select(x => x.Color)
                        .ToArray() ?? Array.Empty<Color>();
                }
                // レイヤーに Fills 設定が存在しない場合はオーバーライド情報を適用するべきではない
                var hasFills = LayerStyle?.Fills?.Where(x => x.Class == ClassText.Fill).Any(x => x.IsEnabled) ?? false;
                return hasFills ? new[] { overrideValue.Value.Color } : Array.Empty<Color>();
            }
        }

        /// <inheritdoc/>
        public Style LayerStyle
        {
            get
            {
                var overrideValue = GetOverrideValue(ParsedOverrideValue.PropertyKeywordLayerStyle);
                var shouldAvoidOverride = string.IsNullOrEmpty(Layer.SharedStyleId);
                if (overrideValue == null || shouldAvoidOverride)
                {
                    return Layer.Style;
                }
                var id = overrideValue.Value.String;
                var style = Adapter.Document.ForeignLayerStyles.Select(x => x.LocalSharedStyle).FirstOrDefault(x => x.DoObjectId == id) ??
                            Adapter.Document.LayerStyles.Objects.FirstOrDefault(x => x.DoObjectId == id);
                return style?.Value;
            }
        }

        /// <inheritdoc/>
        public TextStyle LayerStyleTextStyle
        {
            get
            {
                var overrideValue = GetOverrideValue(ParsedOverrideValue.PropertyKeywordTextStyle);
                var shouldAvoidOverride = string.IsNullOrEmpty(Layer.SharedStyleId);
                if (overrideValue == null || shouldAvoidOverride)
                {
                    return LayerStyle?.TextStyle;
                }
                var id = overrideValue.Value.String;
                var style = Adapter.Document.ForeignTextStyles.Select(x => x.LocalSharedStyle).FirstOrDefault(x => x.DoObjectId == id) ??
                            Adapter.Document.LayerTextStyles.Objects.FirstOrDefault(x => x.DoObjectId == id);
                return style?.Value.TextStyle;
            }
        }

        /// <inheritdoc/>
        public string SharedStyleId
        {
            get
            {
                var shouldAvoidOverride = string.IsNullOrEmpty(Layer.SharedStyleId);
                if (!shouldAvoidOverride)
                {
                    if (GetOverrideValue(ParsedOverrideValue.PropertyKeywordLayerStyle) is { } overrideValue1)
                    {
                        return overrideValue1.Value.String;
                    }
                    if (GetOverrideValue(ParsedOverrideValue.PropertyKeywordTextStyle) is { } overrideValue2)
                    {
                        return overrideValue2.Value.String;
                    }
                }
                return Layer.SharedStyleId;
            }
        }

        /// <summary>このクラスが管理するレイヤーに有効なオーバーライド情報</summary>
        ParsedOverrideValue[] OverrideValues { get; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public LayerAdapter(Adapter adapter, ILayer layer, IOverrideValueAdapter[] overrideValues)
        {
            Adapter = adapter;
            Layer = layer;
            SymbolLayer = adapter.GetSymbol(layer, overrideValues);
            OverrideValues = overrideValues.Where(x => x.CanOverride(layer)).Select(x => x.OverrideValue).ToArray();
        }

        /// <summary>
        /// OverrideProperty のキーワードからオーバーライド情報を得る
        /// </summary>
        protected ParsedOverrideValue GetOverrideValue(string keyword)
        {
            foreach (var value in OverrideValues)
            {
                if (value.OverrideProperty == keyword)
                {
                    return value;
                }
            }
            return null;
        }
    }
}
