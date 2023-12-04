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
using UnityEditor;
using UnityEngine.UI;

namespace SketchConverter
{
    /// <summary>
    /// Sketch 上のマスクを変換する責務を持つ
    /// </summary>
    public class MaskDecorator : Decorator
    {
        /// <inheritdoc/>
        public override bool ShouldDecorate(IDecoratorEntry entry) => entry.Adapter.Layer.HasClippingMask ?? false;

        /// <inheritdoc/>
        public override void DecorateReverseAfter(IDecoratorEntry entry)
        {
            var mask = ObjectFactory.AddComponent<RectMask2D>(entry.GameObject);
            var targets = entry.Parent.Children
                .SkipWhile(x => x != entry)
                .Skip(1)
                .TakeWhile(x => !x.Adapter.Layer.ShouldBreakMaskChain)
                .ToArray();
            foreach (var target in targets)
            {
                if (target.GameObject != null)
                {
                    target.GameObject.transform.SetParent(mask.transform);
                }
            }

            if (entry.GameObject.TryGetComponent<Graphic>(out var graphic))
            {
                graphic.enabled = entry.Adapter.Layer.IsVisible;
            }
        }
    }
}
