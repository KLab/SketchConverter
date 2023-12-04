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
    /// 非アクティブ設定を担う
    /// </summary>
    public class InactiveDecorator : Decorator
    {
        /// <inheritdoc/>
        public override bool ShouldDecorate(IDecoratorEntry entry) => true;

        /// <inheritdoc/>
        public override bool ShouldBreakingDescendants(IDecoratorEntry entry) => false;

        /// <inheritdoc/>
        public override void DecorateAfter(IDecoratorEntry entry)
        {
            if (!entry.Adapter.Layer.IsVisible)
            {
                if (entry.Adapter.Layer.HasClippingMask != true)
                {
                    entry.GameObject.SetActive(false);
                }
                else if (entry.GameObject.TryGetComponent<Graphic>(out var graphic))
                {
                    graphic.enabled = false;
                }
            }
            else if (entry.GameObject.GetComponentsInChildren<Component>().All(x => !(x is Graphic)))
            {
                entry.GameObject.SetActive(false);
            }
        }
    }
}
