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

using System.Collections.Generic;
using System.Linq;
using SketchConverter.FileFormat;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SketchConverter
{
    /// <summary>
    /// .sketch を GameObject に変換する処理を担うクラス
    /// </summary>
    public class Generator : IGenerator
    {
        /// <summary>.sketch ファイルの情報を取得するための仲介クラス</summary>
        protected virtual Adapter Adapter { get; }

        /// <inheritdoc/>
        public virtual DecoratorCollection Decorators { get; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public Generator(Adapter adapter, DecoratorCollection decorators)
        {
            Adapter = adapter;
            Decorators = decorators;
        }

        /// <inheritdoc/>
        public virtual GameObject Generate(ILayer layer)
        {
            var root = new GameObject("SketchConverterTemporaryRootObject");
            try
            {
                Decorators.Sort();
                var layerAdapter = Adapter.CreateLayerAdapter(layer);
                var entries = GenerateDescendants(layerAdapter, root.gameObject).ToArray();
                Decorate(entries);
                DecorateAfter(entries);
                DecorateReverse(entries);
                DecorateReverseAfter(entries);

                var generateRoot = entries.First().GameObject;
                Postprocess(generateRoot);
                return generateRoot;
            }
            finally
            {
                GameObject.DestroyImmediate(root);
            }
        }

        /// <summary>
        /// 引数のレイヤーを起点に全てのレイヤーを走査し GameObject を生成する
        /// </summary>
        protected virtual IEnumerable<IDecoratorEntry> GenerateDescendants(ILayerAdapter adapter, GameObject parent = null)
        {
            var gameObject = GenerateGameObject(parent, adapter);
            var currentEntry = new DecoratorEntry(gameObject, adapter);
            yield return currentEntry;
            if (!ShouldBreakingDescendants(currentEntry))
            {
                foreach (var childLayerAdapter in adapter.LayerAdapters)
                {
                    foreach (var entry in GenerateDescendants(childLayerAdapter, gameObject))
                    {
                        yield return entry;
                    }
                }
            }
        }

        /// <summary>
        /// レイヤー情報を元に GameObject に対して修飾する
        /// </summary>
        protected virtual void Decorate(IDecoratorEntry[] entries)
        {
            foreach (var entry in entries)
            {
                foreach (var decorator in Decorators.Where(x => x.ShouldDecorate(entry)))
                {
                    decorator.Decorate(entry);
                }
            }
        }

        /// <summary>
        /// レイヤー情報を元に GameObject に対して修飾する
        /// </summary>
        protected virtual void DecorateAfter(IDecoratorEntry[] entries)
        {
            foreach (var entry in entries)
            {
                foreach (var decorator in Decorators.Where(x => x.ShouldDecorate(entry)))
                {
                    decorator.DecorateAfter(entry);
                }
            }
        }

        /// <summary>
        /// レイヤー情報を元に GameObject に対して修飾する
        /// </summary>
        protected virtual void DecorateReverse(IDecoratorEntry[] entries)
        {
            foreach (var entry in entries.AsEnumerable().Reverse())
            {
                foreach (var decorator in Decorators.Where(x => x.ShouldDecorate(entry)))
                {
                    decorator.DecorateReverse(entry);
                }
            }
        }

        /// <summary>
        /// レイヤー情報を元に GameObject に対して修飾する
        /// </summary>
        protected virtual void DecorateReverseAfter(IDecoratorEntry[] entries)
        {
            foreach (var entry in entries.AsEnumerable().Reverse())
            {
                foreach (var decorator in Decorators.Where(x => x.ShouldDecorate(entry)))
                {
                    decorator.DecorateReverseAfter(entry);
                }
            }
        }

        /// <summary>
        /// 子孫レイヤーの GameObject 生成を打ち切るかどうか
        /// </summary>
        /// <param name="gameObject">レイヤー情報を元に生成された GameObject</param>
        /// <param name="layer">レイヤー情報</param>
        protected virtual bool ShouldBreakingDescendants(IDecoratorEntry entry)
        {
            foreach (var decorator in Decorators)
            {
                if (decorator.ShouldBreakingDescendants(entry))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// GameObject の生成をする
        /// </summary>
        protected virtual GameObject GenerateGameObject(GameObject parent, ILayerAdapter adapter)
        {
            var gameObject = new GameObject();
            gameObject.AddComponent<RectTransform>();
            if (parent != null)
            {
                gameObject.transform.SetParent(parent.transform);
            }
            return gameObject;
        }

        /// <summary>
        /// 生成されたゲームオブジェクトの後処理を行う
        /// </summary>
        protected virtual void Postprocess(GameObject gameObject)
        {
            SetParentToGeneratedGameObject(gameObject);
            SetLayerToGeneratedGameObject(gameObject);
        }

        /// <summary>
        /// 生成されたゲームオブジェクトの親設定を行う
        /// </summary>
        protected virtual void SetParentToGeneratedGameObject(GameObject gameObject)
        {
            var canvas = GetParentCanvas();
            var rectTransform = gameObject.GetComponent<RectTransform>();
            var transform = gameObject.transform;
            transform.SetParent(canvas != null ? canvas.transform : null);
            transform.localPosition = Vector3.zero;
            transform.localScale = Vector3.one;
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = Vector2.zero;
                rectTransform.SetAsLastSibling();
            }
        }

        /// <summary>
        /// 生成されたゲームオブジェクトのレイヤー設定を行う
        /// </summary>
        protected virtual void SetLayerToGeneratedGameObject(GameObject gameObject)
        {
            const int defaultLayer = 5; // 5: UI
            var parent = gameObject.transform.parent;
            var layer = parent != null ? parent.gameObject.layer : defaultLayer;
            SetLayerRecursively(gameObject.transform, layer);
        }

        /// <summary>
        /// 生成されたゲームオブジェクトの親となるCanvasを返します
        /// </summary>
        protected virtual Canvas GetParentCanvas()
        {
            if (Selection.activeGameObject != null)
            {
                var canvas = Selection.activeGameObject.GetComponent<Canvas>();
                if (canvas != null)
                {
                    return canvas;
                }
            }
            return Enumerable.Range(0, SceneManager.sceneCount)
                .Select(SceneManager.GetSceneAt)
                .Where(x => x.isLoaded)
                .SelectMany(x => x.GetRootGameObjects())
                .Select(x => x.GetComponentInChildren<Canvas>())
                .FirstOrDefault(x => x != null);
        }

        /// <summary>
        /// Transform を通してゲームオブジェクトのレイヤー設定を再帰的に行う
        /// </summary>
        protected virtual void SetLayerRecursively(Transform transform, int layer)
        {
            transform.gameObject.layer = layer;
            foreach (Transform child in transform)
            {
                SetLayerRecursively(child, layer);
            }
        }
    }
}
