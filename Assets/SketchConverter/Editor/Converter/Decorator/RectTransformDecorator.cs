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

using SketchConverter.FileFormat;
using UnityEngine;

namespace SketchConverter
{
    /// <summary>
    /// RectTransform をアタッチする
    /// </summary>
    public class RectTransformDecorator : Decorator
    {
        protected readonly Vector2 CenterVector = new Vector2(0.5f, 0.5f);

        /// <inheritdoc/>
        public override bool ShouldDecorate(IDecoratorEntry entry) => true;

        /// <inheritdoc/>
        public override bool ShouldBreakingDescendants(IDecoratorEntry entry) => false;

        /// <inheritdoc/>
        public override void Decorate(IDecoratorEntry entry)
        {
            var layer = entry.Adapter.Layer;
            var rectTransform = entry.GameObject.GetComponent<RectTransform>();
            SetRectTransform(rectTransform, entry.Adapter.SymbolLayer ?? entry.Adapter.Layer);
            SetPivot(rectTransform, layer);
            SetAnchor(rectTransform, layer);
            SetRotation(rectTransform, layer);
            SetScale(rectTransform, layer);
        }

        /// <inheritdoc/>
        public override void DecorateReverse(IDecoratorEntry entry)
        {
            if (entry.Adapter.SymbolLayer == null)
            {
                return;
            }
            var layer = entry.Adapter.Layer;
            var rectTransform = entry.GameObject.GetComponent<RectTransform>();
            SetRectTransform(rectTransform, layer);
            SetPivot(rectTransform, layer);
            SetAnchor(rectTransform, layer);
            SetRotation(rectTransform, layer);
            SetScale(rectTransform, layer);
            SetSketchScale(rectTransform, layer);
        }

        /// <summary>
        /// Pivot の設定を行う
        /// </summary>
        protected virtual void SetPivot(RectTransform transform, ILayer layer)
        {
            SetPivotLikeEditor(transform, CenterVector);
        }

        /// <summary>
        /// Anchor の設定を行う
        /// </summary>
        protected virtual void SetAnchor(RectTransform transform, ILayer layer)
        {
            var (anchorMin, anchorMax) = GetSketchAnchor(transform, layer);
            SetAnchorLikeEditor(transform, anchorMin, anchorMax);
        }

        /// <summary>
        /// Rotation の設定を行う
        /// </summary>
        protected virtual void SetRotation(RectTransform transform, ILayer layer)
        {
            var flipped = layer.IsFlippedHorizontal ^ layer.IsFlippedVertical ? -1 : 1;
            transform.localRotation = Quaternion.Euler(0, 0, (float) layer.Rotation * flipped);
        }

        /// <summary>
        /// Scale の設定を行う
        /// </summary>
        protected virtual void SetScale(RectTransform transform, ILayer layer)
        {
            var x = layer.IsFlippedHorizontal ? -1 : 1;
            var y = layer.IsFlippedVertical ? -1 : 1;
            var scale = (float) ((layer as OriginalMasterLayer)?.Scale ?? 1.0f);
            transform.localScale = new Vector3(scale * x, scale * y, 1.0f);
        }

        /// <summary>
        /// RectTransform をアタッチする
        /// </summary>
        protected virtual void SetRectTransform(RectTransform transform, ILayer layer)
        {
            var frame = layer.Frame;
            transform.anchorMin = new Vector2(0, 1);
            transform.anchorMax = new Vector2(0, 1);
            transform.pivot = new Vector2(0, 1);
            transform.anchoredPosition = new Vector3((float) frame.X, (float) -frame.Y);
            transform.sizeDelta = new Vector2((float) frame.Width, (float) frame.Height);
        }

        /// <summary>
        /// Pivot を変更する。エディタ上で編集した時と同じように位置は保つ
        /// </summary>
        protected virtual void SetPivotLikeEditor(RectTransform transform, Vector2 pivot)
        {
            var deltaSize = transform.sizeDelta;
            var deltaPivot = transform.pivot - pivot;
            var deltaPosition = new Vector2(deltaPivot.x * deltaSize.x, deltaPivot.y * deltaSize.y);
            transform.pivot = pivot;
            transform.anchoredPosition -= deltaPosition;
        }

        /// <summary>
        /// Anchor を変更する。エディタ上で編集した時と同じように位置は保つ
        /// </summary>
        protected virtual void SetAnchorLikeEditor(RectTransform transform, Vector2 anchorMin, Vector2 anchorMax)
        {
            var parent = transform.parent as RectTransform;
            if (parent == null)
            {
                transform.anchorMin = anchorMin;
                transform.anchorMax = anchorMax;
                return;
            }

            var min = anchorMin - transform.anchorMin;
            var max = anchorMax - transform.anchorMax;
            var parentRect = parent.rect;
            var rect =
            (
                l: parentRect.width * min.x,
                r: parentRect.width * max.x,
                t: parentRect.height * max.y,
                b: parentRect.height * min.y
            );
            transform.anchorMin = anchorMin;
            transform.anchorMax = anchorMax;
            transform.sizeDelta += new Vector2(rect.l - rect.r, rect.b - rect.t);
            var pivot = transform.pivot;
            transform.anchoredPosition -= new Vector2
            (
                rect.l * (1 - pivot.x) + rect.r * pivot.x,
                rect.b * (1 - pivot.y) + rect.t * pivot.y
            );
        }

        /// <summary>
        /// Sketch 上での標準設定と同じような挙動をする Anchor を得る
        /// </summary>
        protected virtual (Vector2 min, Vector2 max) GetSketchAnchor(RectTransform target, ILayer layer)
        {
            var parent = target.parent as RectTransform;
            if (parent == null)
            {
                return (CenterVector, CenterVector);
            }
            if (Mathf.Approximately(parent.rect.width, 0) || Mathf.Approximately(parent.rect.height, 0))
            {
                return (CenterVector, CenterVector);
            }
            var rect = (parent: parent.rect, target: target.rect);
            var deltaMin = CenterVector - target.anchorMin;
            var deltaMax = CenterVector - target.anchorMax;
            var deltaRect =
            (
                l: rect.parent.width * deltaMin.x,
                r: rect.parent.width * deltaMax.x,
                t: rect.parent.height * deltaMax.y,
                b: rect.parent.height * deltaMin.y
            );
            var targetPivot = target.pivot;
            var deltaPosition = target.anchoredPosition - new Vector2
            (
                deltaRect.l * (1 - targetPivot.x) + deltaRect.r * targetPivot.x,
                deltaRect.b * (1 - targetPivot.y) + deltaRect.t * targetPivot.y
            );
            var anchor =
            (
                l: (rect.target.xMin - rect.parent.x + deltaPosition.x) / rect.parent.width,
                r: (rect.target.xMax - rect.parent.x + deltaPosition.x) / rect.parent.width,
                t: (rect.target.yMin - rect.parent.y + deltaPosition.y) / rect.parent.height,
                b: (rect.target.yMax - rect.parent.y + deltaPosition.y) / rect.parent.height
            );
            var min = new Vector2(anchor.l, anchor.t);
            var max = new Vector2(anchor.r, anchor.b);
            var has =
            (
                l: layer.HasResizingConstraint(ResizingConstraint.Left),
                r: layer.HasResizingConstraint(ResizingConstraint.Right),
                b: layer.HasResizingConstraint(ResizingConstraint.Bottom),
                t: layer.HasResizingConstraint(ResizingConstraint.Top),
                w: layer.HasResizingConstraint(ResizingConstraint.Width),
                h: layer.HasResizingConstraint(ResizingConstraint.Height)
            );
            if (has.w)
            {
                var x = has.l ? 0 : has.r ? 1 : 0.5f;
                min.x = x;
                max.x = x;
            }
            else
            {
                min.x = has.l ? 0 : min.x;
                max.x = has.r ? 1 : max.x;
            }
            if (has.h)
            {
                var y = has.b ? 0 : has.t ? 1 : 0.5f;
                min.y = y;
                max.y = y;
            }
            else
            {
                min.y = has.b ? 0 : min.y;
                max.y = has.t ? 1 : max.y;
            }
            return (min, max);
        }

        /// <summary>
        /// Sketchの Scale に合わせてRectTransformのサイズを変更する
        /// </summary>
        protected virtual void SetSketchScale(RectTransform transform, ILayer layer)
        {
            var scale = 1.0f / (float) ((layer as OriginalMasterLayer)?.Scale ?? 1.0f);
            if (Mathf.Approximately(scale, 1.0f))
            {
                return;
            }
            var (anchorMin, anchorMax) = GetSketchAnchor(transform, layer);
            var size = (anchorMax - anchorMin) * 0.5f;
            var position = (anchorMax + anchorMin) * 0.5f;
            transform.anchorMin = new Vector2(
                -size.x * scale + position.x,
                -size.y * scale + position.y);
            transform.anchorMax = new Vector2(
                size.x * scale + position.x,
                size.y * scale + position.y);
        }
    }
}
