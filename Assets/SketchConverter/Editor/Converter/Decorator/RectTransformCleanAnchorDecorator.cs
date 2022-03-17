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
using static UnityEngine.Mathf;

namespace SketchConverter
{
    /// <summary>
    /// 中途半端な値となるアンカー設定の場合大きさ固定中央基準として出力する RectTransformDecorator
    /// </summary>
    /// <remarks>
    /// 標準の RectTransformDecorator は Sketch のアンカーを再現しているが
    /// インスペクタ上では中途半端な値のアンカー設定となり扱いにくいため、
    /// 再現を優先しない場合はこの Decorator を利用した方がレイアウト作業が行いやすい
    /// </remarks>
    public class RectTransformCleanAnchorDecorator : RectTransformDecorator
    {
        public override void DecorateReverseAfter(IDecoratorEntry entry)
        {
            base.DecorateReverseAfter(entry);
            var transform = entry.GameObject.GetComponent<RectTransform>();
            var min = transform.anchorMin;
            var max = transform.anchorMax;
            if (!(Approximately(min.x, 0.5f) && Approximately(max.x, 0.5f)) &&
                !(Approximately(min.x, 0.0f) && Approximately(max.x, 0.0f)) &&
                !(Approximately(min.x, 1.0f) && Approximately(max.x, 1.0f)) &&
                !(Approximately(min.x, 0.0f) && Approximately(max.x, 1.0f)))
            {
                min.x = 0.5f;
                max.x = 0.5f;
            }
            if (!(Approximately(min.y, 0.5f) && Approximately(max.y, 0.5f)) &&
                !(Approximately(min.y, 0.0f) && Approximately(max.y, 0.0f)) &&
                !(Approximately(min.y, 1.0f) && Approximately(max.y, 1.0f)) &&
                !(Approximately(min.y, 0.0f) && Approximately(max.y, 1.0f)))
            {
                min.y = 0.5f;
                max.y = 0.5f;
            }
            SetAnchorLikeEditor(transform, min, max);
            var pos = transform.anchoredPosition;
            pos.x = Round(pos.x);
            pos.y = Round(pos.y);
            transform.anchoredPosition = pos;
        }
    }
}
