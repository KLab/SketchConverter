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
using SketchConverter.Newtonsoft.Json;

namespace SketchConverter
{
    /// <summary>
    /// SketchFile クラスからの情報取得処理を担う
    /// </summary>
    public class Adapter
    {
        public virtual SketchFile SketchFile { get; }
        public virtual DocumentClass Document => SketchFile.Document;
        public virtual PageLayer[] Pages => SketchFile.Pages;
        protected readonly Dictionary<string, ILayer> symbols;

        public Adapter(SketchFile sketchFile)
        {
            SketchFile = sketchFile;
            symbols = CreateSymbolsDictionary(sketchFile);
        }

        /// <summary>
        /// シンボル検索用の辞書型を作成する
        /// </summary>
        protected Dictionary<string, ILayer> CreateSymbolsDictionary(SketchFile sketchFile)
        {
            var symbols1 = sketchFile.Pages
                .SelectMany(x => x.Layers)
                .SelectMany(x => x.DescendantsAndSelf())
                .Where(x => x.IsClass(ClassText.SymbolMaster))
                .Select(x => (id: x.SymbolId, layer: (ILayer) x));
            var symbols2 = sketchFile.Document.ForeignSymbols
                .Select(x => x.SymbolMaster)
                .Select(x => (id: x.SymbolId, layer: (ILayer) x));
            var symbols3 = sketchFile.Document.ForeignSymbols
                .SelectMany(x => x.SymbolMaster.Layers)
                .SelectMany(x => x.DescendantsAndSelf())
                .Where(x => x.IsClass(ClassText.SymbolMaster))
                .Select(x => (id: x.SymbolId, layer: (ILayer) x));
            return symbols1
                .Concat(symbols2)
                .Concat(symbols3)
                .GroupBy(x => x.id, x => x.layer)
                .ToDictionary(x => x.Key, x => x.First());
        }

        /// <summary>
        /// ディープクローンを行う
        /// </summary>
        public virtual T DeepClone<T>(T source)
        {
            var deserializeSettings = new JsonSerializerSettings {ObjectCreationHandling = ObjectCreationHandling.Replace};
            return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(source), deserializeSettings);
        }

        /// <summary>
        /// シンボルを返す
        /// </summary>
        /// <param name="symbolId">検索するシンボルID</param>
        /// <returns>シンボル情報。存在しない場合はnullを返します。</returns>
        public virtual ILayer GetSymbol(string symbolId)
        {
            if (!string.IsNullOrEmpty(symbolId) && symbols.TryGetValue(symbolId, out var symbol))
            {
                return symbol;
            }
            return null;
        }

        /// <summary>
        /// 引数のレイヤーの元シンボルを返す。オーバーライドを解決する
        /// </summary>
        public virtual ILayer GetSymbol(ILayer layer, IOverrideValueAdapter[] overrideValues)
        {
            var overrideSymbolValue = overrideValues
                .FirstOrDefault(x => x.OverrideProperty == ParsedOverrideValue.PropertyKeywordSymbol && x.CanOverride(layer));
            return GetSymbol(overrideSymbolValue != null ? overrideSymbolValue.Value.String : layer.SymbolId);
        }

        /// <summary>
        /// 素のレイヤー情報から取り回しやすい ILayerAdapter 型に変換する
        /// </summary>
        public virtual ILayerAdapter CreateLayerAdapter(ILayer rootLayer)
        {
            return Create(rootLayer, Array.Empty<IOverrideValueAdapter>());

            ILayerAdapter Create(ILayer targetLayer, IOverrideValueAdapter[] overrideValues)
            {
                overrideValues = UpdateOverrideValues(overrideValues, targetLayer);
                var adapter = new LayerAdapter(this, targetLayer, overrideValues);
                var isEmptySymbol = !string.IsNullOrEmpty(adapter.Layer.SymbolId) && string.IsNullOrEmpty(adapter.LayerSymbolId);
                if (isEmptySymbol)
                {
                    return null;
                }
                foreach (var layer in adapter.Layers ?? Enumerable.Empty<OriginalMasterLayer>())
                {
                    if (Create(layer, overrideValues) is { } childAdapter)
                    {
                        adapter.LayerAdapters.Add(childAdapter);
                    }
                }
                return adapter;
            }
        }

        /// <summary>
        /// 子孫に引き回すオーバーライド値を最新化する
        /// </summary>
        IOverrideValueAdapter[] UpdateOverrideValues(IOverrideValueAdapter[] overrideValues, ILayer layer)
        {
            if (layer.IsClass(ClassText.SymbolInstance) && layer is OriginalMasterLayer symbolInstance)
            {
                overrideValues = overrideValues.Where(x => !x.IsInvalid).Select(x => x.Clone()).ToArray();
                foreach (var overrideValue in overrideValues)
                {
                    overrideValue.Trace(symbolInstance);
                }
                if (GetSymbol(symbolInstance, overrideValues) is { } symbol)
                {
                    var properties = symbol.GetParsedOverrideProperties();
                    overrideValues = symbol.GetAllowsOverrides()
                        ? overrideValues.Concat(symbolInstance.GetParsedOverrideValues().Select(x => new OverrideValueAdapter(x))).ToArray()
                        : overrideValues.Where(x => x.CanOverride(symbolInstance)).ToArray();
                    foreach (var overrideValue in overrideValues)
                    {
                        overrideValue.ApplyProperties(properties);
                    }
                }
            }
            return overrideValues;
        }
    }
}
