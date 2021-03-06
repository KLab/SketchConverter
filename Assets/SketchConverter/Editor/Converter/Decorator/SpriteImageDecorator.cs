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
using System.IO;
using System.Linq;
using SketchConverter.FileFormat;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Color = UnityEngine.Color;

namespace SketchConverter
{
    /// <summary>
    /// Image コンポーネントをアタッチしスプライトを設定する責務を持つ
    /// </summary>
    public class SpriteImageDecorator : Decorator
    {
        /// <summary>レイヤー名が Key と一致したら、Value のテクスチャパスを利用する</summary>
        protected virtual Dictionary<string, string> Dictionary { get; } = new Dictionary<string, string>();

        /// <summary>コンストラクタ</summary>
        public SpriteImageDecorator() : this(CreateDefaultDictionary)
        {
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="createDictionary">Key となるレイヤー名に紐づくテクスチャパスを Value に持つ辞書型を作成する処理</param>
        public SpriteImageDecorator(Func<Dictionary<string, string>> createDictionary) => Dictionary = createDictionary();

        /// <inheritdoc/>
        public override bool ShouldDecorate(IDecoratorEntry entry) => GetSpritePath(entry.Adapter) != null &&
                                                                      !entry.Adapter.Layer.IsClass(ClassText.Text) &&
                                                                      entry.GameObject.GetComponent<Graphic>() == null;

        /// <inheritdoc/>
        public override bool ShouldBreakingDescendants(IDecoratorEntry entry) => ShouldDecorate(entry);

        /// <inheritdoc/>
        public override void Decorate(IDecoratorEntry entry)
        {
            var image = entry.GameObject.AddComponent<Image>();
            var path = GetSpritePath(entry.Adapter);
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite != null)
            {
                image.sprite = sprite;
                image.type = sprite.border == Vector4.zero ? Image.Type.Simple : Image.Type.Sliced;
            }

            var color = LayerToFillColor(entry.Adapter);
            color.a *= (float) (entry.Adapter.LayerStyle?.ContextSettings.Opacity ?? 1);
            image.color = color;
        }

        /// <summary>
        /// 適用するテクスチャのパスを返す
        /// </summary>
        protected virtual string GetSpritePath(ILayerAdapter adapter)
        {
            if (adapter.SymbolLayer != null && Dictionary.TryGetValue(adapter.SymbolLayer.Name, out var symbolName))
            {
                return symbolName;
            }
            if (Dictionary.TryGetValue(adapter.Layer.Name, out var symbolInstanceName))
            {
                return symbolInstanceName;
            }
            return null;
        }

        /// <summary>
        /// カラー情報を返す
        /// </summary>
        protected virtual Color LayerToFillColor(ILayerAdapter adapter)
        {
            var fills = adapter.AvailableFillColors;
            return fills.Length > 0 ? fills.Last().ToUnityColor() : Color.white;
        }

        /// <summary>
        /// 設定したディレクトリからの相対パス名もしくはパス関係なくファイル名を Key とした時に該当のファイルを Value に持つ辞書型を作成する
        /// </summary>
        /// <remarks>補足。このメソッドが virtual になっていない設計理由は、コンストラクタ内の仮想メソッド呼び出しを避ける為である</remarks>
        protected static Dictionary<string, string> CreateDefaultDictionary()
        {
            const string filter = "t:texture";
            const string separator = "/";

            string WithoutExtension(string path) => Path.GetDirectoryName(path).Replace(@"\", separator) + separator + Path.GetFileNameWithoutExtension(path);

            void IfNeededAdd<TKey, TValue>(Dictionary<TKey, TValue> dic, TKey key, TValue value)
            {
                if (!dic.ContainsKey(key))
                {
                    dic.Add(key, value);
                }
            }

            var dictionary = new Dictionary<string, string>();
            var rootPaths = SketchConverterSettings.GetTextureDirectoryPaths();
            if (rootPaths.Length == 0)
            {
                rootPaths = new[] {"Assets"};
            }
            foreach (var rootPath in rootPaths)
            {
                var assetPaths = AssetDatabase.FindAssets(filter, new[] {rootPath})
                    .Select(AssetDatabase.GUIDToAssetPath)
                    .ToArray();
                foreach (var assetPath in assetPaths)
                {
                    var filePath = WithoutExtension(assetPath).Remove(0, rootPath.Length + separator.Length);
                    var fileName = Path.GetFileNameWithoutExtension(assetPath);
                    IfNeededAdd(dictionary, filePath, assetPath);
                    IfNeededAdd(dictionary, fileName, assetPath);
                }
            }
            return dictionary;
        }
    }
}
