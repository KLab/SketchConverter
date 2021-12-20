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
using SketchConverter.FileFormat;
using UnityEngine;
using UnityEngine.UI;
using Color = UnityEngine.Color;

namespace SketchConverter
{
    /// <summary>
    /// 子に対するカラーの反映を担う
    /// </summary>
    public class FillColorDecorator : Decorator
    {
        /// <inheritdoc/>
        public override bool ShouldDecorate(IDecoratorEntry entry) => entry.Adapter.AvailableFillColors.Any();

        /// <inheritdoc/>
        public override void DecorateReverse(IDecoratorEntry entry)
        {
            var color = entry.Adapter.AvailableFillColors.Last().ToUnityColor();
            ApplyToChildrenComponents(entry.GameObject, color);
        }

        /// <summary>
        /// 引数の子要素全てにカラーを適用
        /// </summary>
        protected virtual void ApplyToChildrenComponents(GameObject gameObject, Color color)
        {
            ApplyToChildrenComponents<Graphic>(gameObject, color, (component, c) =>
            {
                component.color = Blend(component.color, c);
            });
        }

        /// <summary>
        /// 引数の子要素全てにカラーを適用
        /// </summary>
        protected virtual void ApplyToChildrenComponents<T>(GameObject gameObject, Color color, Action<T, Color> apply) where T : Component
        {
            var withoutSelfComponents = gameObject.GetComponentsInChildren<T>(true).Where(x => x.gameObject != gameObject);
            foreach (var component in withoutSelfComponents)
            {
                apply(component, color);
            }
        }

        /// <summary>
        /// 反映したカラーの結果を返す
        /// </summary>
        protected virtual Color Blend(Color source, Color setting)
        {
            source.r = setting.r;
            source.g = setting.g;
            source.b = setting.b;
            source.a *= setting.a;
            return source;
        }
    }
}
