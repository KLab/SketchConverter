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
using UnityEditor;
using UnityEngine.UI;
using Color = UnityEngine.Color;

namespace SketchConverter
{
    /// <summary>
    /// Sketch 上の Rectangle を Image コンポーネントに変換する責務を持つ
    /// </summary>
    public class RectangleImageDecorator : Decorator
    {
        /// <inheritdoc/>
        public override bool ShouldDecorate(IDecoratorEntry entry) => entry.Adapter.Layer.IsClass(ClassText.Rectangle) &&
                                                                      entry.GameObject.GetComponent<Graphic>() == null &&
                                                                      entry.Adapter.AvailableFillColors.Any();

        /// <inheritdoc/>
        public override bool ShouldBreakingDescendants(IDecoratorEntry entry) => ShouldDecorate(entry);

        /// <inheritdoc/>
        public override void Decorate(IDecoratorEntry entry)
        {
            var image = ObjectFactory.AddComponent<Image>(entry.GameObject);
            var color = entry.Adapter.AvailableFillColors
                .Select(x => x.ToUnityColor())
                .Aggregate(Blend);
            color.a *= (float) (entry.Adapter.LayerStyle?.ContextSettings.Opacity ?? 1);
            image.color = color;

            Color Blend(Color c1, Color c2)
            {
                c1.r = c1.r * (1 - c2.a) + c2.r * c2.a;
                c1.g = c1.g * (1 - c2.a) + c2.g * c2.a;
                c1.b = c1.b * (1 - c2.a) + c2.b * c2.a;
                c1.a = 1 - (1 - c1.a) * (1 - c2.a);
                return c1;
            }
        }
    }
}
