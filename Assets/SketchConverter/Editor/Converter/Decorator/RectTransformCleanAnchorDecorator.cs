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
using UnityEngine;

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
            RoundRectTransform(transform);
        }

        protected static bool Approximately(float a, float b) => Mathf.Abs(a - b) < 0.001f;

        protected virtual void RoundRectTransform(RectTransform transform)
        {
            // 1度別の値にしないと端数がでてしまう
            var swapVector2 = Vector2.zero;
            var swapVector3 = Vector3.zero;

            var anchoredPosition = transform.anchoredPosition;
            transform.anchoredPosition = swapVector2;
            transform.anchoredPosition = Round(anchoredPosition, 0);

            var anchorMin = transform.anchorMin;
            transform.anchorMin = swapVector2;
            transform.anchorMin = Round(anchorMin, 2);

            var anchorMax = transform.anchorMax;
            transform.anchorMax = swapVector2;
            transform.anchorMax = Round(anchorMax, 2);

            var sizeDelta = transform.sizeDelta;
            transform.sizeDelta = swapVector2;
            transform.sizeDelta = Round(sizeDelta, 0);

            var localScale = transform.localScale;
            transform.localScale = swapVector3;
            transform.localScale = Round(localScale, 2);

            transform.anchoredPosition = GetRoundedAnchoredPositionForInspector(transform);
        }

        protected virtual float Round(float value, int digits) => (float) Math.Round(value, digits, MidpointRounding.ToEven);

        protected virtual Vector2 Round(Vector2 value, int digits)
        {
            value.x = Round(value.x, digits);
            value.y = Round(value.y, digits);
            return value;
        }

        protected virtual Vector3 Round(Vector3 value, int digits)
        {
            value.x = Round(value.x, digits);
            value.y = Round(value.y, digits);
            value.z = Round(value.z, digits);
            return value;
        }

        protected virtual Vector2 GetRoundedAnchoredPositionForInspector(RectTransform transform)
        {
            var sizeDelta = transform.sizeDelta;
            var anchoredPosition = transform.anchoredPosition;
            var anchorMin = transform.anchorMin;
            var anchorMax = transform.anchorMax;

            var x = CleanPosition(sizeDelta.x, anchoredPosition.x, anchorMin.x, anchorMax.x);
            var y = CleanPosition(sizeDelta.y, anchoredPosition.y, anchorMin.y, anchorMax.y);
            return new Vector2(x, y);

            static float CleanPosition(float size, float pos, float anchorMin, float anchorMax)
            {
                if (!Approximately(size % 2.0f, 0.0f) &&
                    Approximately(anchorMin, 0.0f) &&
                    Approximately(anchorMax, 1.0f))
                {
                    return pos + (pos < 0 ? 0.5f : -0.5f);
                }
                return pos;
            }
        }
    }
}
