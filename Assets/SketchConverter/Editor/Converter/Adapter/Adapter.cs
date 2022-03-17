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

        public Adapter(SketchFile sketchFile) => SketchFile = sketchFile;

        /// <summary>
        /// ディープクローンを行う
        /// </summary>
        public virtual T DeepClone<T>(T source)
        {
            var deserializeSettings = new JsonSerializerSettings {ObjectCreationHandling = ObjectCreationHandling.Replace};
            return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(source), deserializeSettings);
        }

        /// <summary>
        /// ライブラリのシンボルを返す
        /// </summary>
        /// <param name="symbolId">検索するシンボルID</param>
        /// <returns>シンボル情報。存在しない場合はnullを返します。</returns>
        public virtual ILayer GetSymbol(string symbolId)
        {
            if (string.IsNullOrEmpty(symbolId))
            {
                return null;
            }
            var layer = Pages
                .SelectMany(x => x.Layers)
                .SelectMany(x => x.DescendantsAndSelf())
                .Where(x => x.IsClass(ClassText.SymbolMaster))
                .FirstOrDefault(x => x.SymbolId == symbolId);
            if (layer != null)
            {
                return layer;
            }
            var foreignSymbol = Document.ForeignSymbols
                .Where(x => x.SymbolMaster.SymbolId == symbolId)
                .Select(x => x.SymbolMaster)
                .FirstOrDefault();
            if (foreignSymbol != null)
            {
                return foreignSymbol;
            }
            var foreignSymbolLayer = Document.ForeignSymbols
                .SelectMany(x => x.SymbolMaster.Layers)
                .SelectMany(x => x.DescendantsAndSelf())
                .Where(x => x.IsClass(ClassText.SymbolMaster))
                .FirstOrDefault(x => x.SymbolId == symbolId);
            if (foreignSymbolLayer != null)
            {
                return foreignSymbolLayer;
            }
            return null;
        }

        /// <summary>
        /// 素のレイヤー情報から取り回しやすい ILayerAdapter 型に変換する
        /// </summary>
        public virtual ILayerAdapter CreateLayerAdapter(ILayer rootLayer)
        {
            return Create(rootLayer, Enumerable.Empty<ParsedOverrideValue>().ToArray());

            ILayerAdapter Create(ILayer targetLayer, ParsedOverrideValue[] parsedOverrideValues)
            {
                // ILayer to entity
                if (targetLayer.IsClass(ClassText.SymbolInstance) && targetLayer is OriginalMasterLayer originalMasterLayer)
                {
                    parsedOverrideValues = originalMasterLayer.GetParsedOverrideValues().Concat(parsedOverrideValues).ToArray();
                }
                var entity = new LayerAdapter(this, targetLayer, parsedOverrideValues, GetSymbol);
                var overrideSymbolToEmpty = !string.IsNullOrEmpty(entity.Layer.SymbolId) && string.IsNullOrEmpty(entity.LayerSymbolId);
                if (overrideSymbolToEmpty)
                {
                    return null;
                }

                // create entity.Layers
                foreach (var layer in entity.Layers ?? Enumerable.Empty<OriginalMasterLayer>())
                {
                    var childEntity = Create(layer, parsedOverrideValues);
                    if (childEntity != null)
                    {
                        entity.LayerAdapters.Add(childEntity);
                    }
                }
                return entity;
            }
        }
    }
}
