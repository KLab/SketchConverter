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
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace SketchConverter
{
    /// <summary>
    /// 子に対する透明度の反映を担う
    /// </summary>
    public class OpacityDecorator : Decorator
    {
        /// <inheritdoc/>
        public override bool ShouldDecorate(IDecoratorEntry entry) => entry.Adapter.Layer?.Style?.ContextSettings.Opacity < 1;

        /// <inheritdoc/>
        public override void DecorateReverse(IDecoratorEntry entry)
        {
            var opacity = (float) entry.Adapter.Layer.Style.ContextSettings.Opacity;
            ApplyToChildrenComponents(entry.GameObject, opacity);
        }

        /// <summary>
        /// 引数の子要素全てに透明度を適用
        /// </summary>
        protected virtual void ApplyToChildrenComponents(GameObject gameObject, float opacity)
        {
            ApplyToChildrenComponents<Graphic>(gameObject, opacity, (component, value) =>
            {
                component.color = Blend(component.color, value);
            });
        }

        /// <summary>
        /// 引数の子要素全てに透明度を適用
        /// </summary>
        protected virtual void ApplyToChildrenComponents<T>(GameObject gameObject, float opacity, Action<T, float> apply) where T : Component
        {
            var withoutSelfComponents = gameObject.GetComponentsInChildren<T>(true).Where(x => x.gameObject != gameObject);
            foreach (var component in withoutSelfComponents)
            {
                apply(component, opacity);
            }
        }

        /// <summary>
        /// 透明度を反映したカラーの結果を返す
        /// </summary>
        protected virtual Color Blend(Color source, float setting)
        {
            source.a *= setting;
            return source;
        }
    }
}
