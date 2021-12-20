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

using UnityEngine;

namespace SketchConverter
{
    /// <summary>
    /// デコレータへ渡す引数
    /// </summary>
    public interface IDecoratorEntry
    {
        /// <summary>一階層あたりの Sketch の情報</summary>
        ILayerAdapter Adapter { get; }

        /// <summary>装飾先となるGameObject</summary>
        GameObject GameObject { get; }
    }

    /// <inheritdoc/>
    public class DecoratorEntry : IDecoratorEntry
    {
        /// <inheritdoc/>
        public GameObject GameObject { get; }

        /// <inheritdoc/>
        public ILayerAdapter Adapter { get; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public DecoratorEntry(GameObject gameObject, ILayerAdapter adapter)
        {
            GameObject = gameObject;
            Adapter = adapter;
        }
    }
}
