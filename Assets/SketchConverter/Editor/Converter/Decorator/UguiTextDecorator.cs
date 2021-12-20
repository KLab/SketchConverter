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
using SketchConverter.FileFormat;
using UnityEngine;
using UnityEngine.UI;

namespace SketchConverter
{
    /// <summary>
    /// uGUI 標準の Text コンポーネントをアタッチする責務を担う
    /// </summary>
    public class UguiTextDecorator : Decorator
    {
        /// <inheritdoc/>
        public override bool ShouldDecorate(IDecoratorEntry entry) =>
            entry.Adapter.Layer.IsClass(ClassText.Text) && entry.GameObject.GetComponent<Graphic>() == null;

        /// <inheritdoc/>
        public override bool ShouldBreakingDescendants(IDecoratorEntry entry) => ShouldDecorate(entry);

        /// <inheritdoc/>
        public override void Decorate(IDecoratorEntry entry)
        {
            var attributes = entry.Adapter.LayerStyle.TextStyle.EncodedAttributes;
            var color = attributes.MsAttributedStringColorAttribute;

            var text = entry.GameObject.AddComponent<Text>();
            text.text = entry.Adapter.LayerAttributedStringString;
            text.fontSize = (int) Math.Round(attributes.MsAttributedStringFontAttribute.Attributes.Size);
            text.alignment = ToTextAnchor(attributes.ParagraphStyle.AlignmentEnum, entry.Adapter.LayerStyle.TextStyle.VerticalAlignmentEnum);
            text.color = color.ToUnityColor();
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            var fontInfo = SketchConverterSettings.FindUnityFontBySketchFontInfo(attributes.MsAttributedStringFontAttribute.Attributes.Name);
            text.font = fontInfo.Font;

            var lineSpacing = 0.0;
            lineSpacing += attributes.ParagraphStyle.MaximumLineHeight ?? attributes.MsAttributedStringFontAttribute.Attributes.Size * fontInfo.LineRate;
            text.lineSpacing = (float) (lineSpacing / attributes.MsAttributedStringFontAttribute.Attributes.Size) *
                               (text.font.fontSize / (float) text.font.lineHeight);

            if (attributes.ParagraphStyle.ParagraphSpacing != null)
            {
                Debug.LogWarningFormat(
                    entry.GameObject,
                    @"TextのParagraphが設定されています。
SketchConverterではParagraphには対応していないため設定を無視しました。
＜対象テキスト＞
{0}",
                    text.text);
            }
        }

        /// <summary>
        /// Sketch の Alignment 設定から uGUI の設定を返す
        /// </summary>
        protected virtual TextAnchor ToTextAnchor(TextHorizontalAlignment horizontal, TextVerticalAlignment vertical)
        {
            switch (horizontal)
            {
                case TextHorizontalAlignment.Left:
                case TextHorizontalAlignment.Justified: // uGUI にないので一番見た目が近いLeftと同じにする
                case TextHorizontalAlignment.Natural: // uGUI にないので一番見た目が近いLeftと同じにする
                    switch (vertical)
                    {
                        case TextVerticalAlignment.Top:
                            return TextAnchor.UpperLeft;
                        case TextVerticalAlignment.Middle:
                            return TextAnchor.MiddleLeft;
                        case TextVerticalAlignment.Bottom:
                            return TextAnchor.LowerLeft;
                    }
                    break;
                case TextHorizontalAlignment.Right:
                    switch (vertical)
                    {
                        case TextVerticalAlignment.Top:
                            return TextAnchor.UpperRight;
                        case TextVerticalAlignment.Middle:
                            return TextAnchor.MiddleRight;
                        case TextVerticalAlignment.Bottom:
                            return TextAnchor.LowerRight;
                    }
                    break;
                case TextHorizontalAlignment.Centered:
                    switch (vertical)
                    {
                        case TextVerticalAlignment.Top:
                            return TextAnchor.UpperCenter;
                        case TextVerticalAlignment.Middle:
                            return TextAnchor.MiddleCenter;
                        case TextVerticalAlignment.Bottom:
                            return TextAnchor.LowerCenter;
                    }
                    break;
            }
            return TextAnchor.MiddleCenter;
        }
    }
}
