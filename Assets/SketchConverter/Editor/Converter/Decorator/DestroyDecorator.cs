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
using UnityEngine;
using UnityEngine.UI;

namespace SketchConverter
{
    /// <summary>
    /// レイヤーの削除を担う
    /// </summary>
    public class DestroyDecorator : Decorator
    {
        /// <inheritdoc/>
        public override bool ShouldDecorate(IDecoratorEntry entry) => true;

        /// <inheritdoc/>
        public override bool ShouldBreakingDescendants(IDecoratorEntry entry) => false;

        /// <inheritdoc/>
        public override void Decorate(IDecoratorEntry entry)
        {
            if (!entry.Adapter.Layer.IsVisible && entry.Adapter.Layer.HasClippingMask != true)
            {
                Object.DestroyImmediate(entry.GameObject);
            }
        }

        /// <inheritdoc/>
        public override void DecorateReverseAfter(IDecoratorEntry entry)
        {
            if (entry.GameObject.GetComponentsInChildren<Component>().All(x => !(x is Graphic)))
            {
                Object.DestroyImmediate(entry.GameObject);
            }
            else if (IsInvisibleMaskLayerHasEmpty(entry))
            {
                Object.DestroyImmediate(entry.GameObject);
            }

            bool IsInvisibleMaskLayerHasEmpty(IDecoratorEntry entry) => !entry.Adapter.Layer.IsVisible &&
                                                                        entry.Adapter.Layer.HasClippingMask == true &&
                                                                        entry.GameObject.transform.childCount == 0;
        }
    }
}
