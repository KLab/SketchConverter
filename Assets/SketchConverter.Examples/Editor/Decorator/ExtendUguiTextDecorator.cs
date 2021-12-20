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
using UnityEngine.UI;

namespace SketchConverter.Examples
{
    /// <summary>
    /// 標準のUguiTextDecoratorでは対応していない以下のsketch機能に対応したサンプル実装です。<br />
    /// - sketchファイルで1つのText枠内に部分文字ごとに複数のTextStyleを設定した場合に、uGUI TextのRichTextを使ってColorとSizeのみ対応するサンプル実装です。
    /// </summary>
    public class ExtendUguiTextDecorator : UguiTextDecorator
    {
        public override void Decorate(IDecoratorEntry entry)
        {
            base.Decorate(entry);

            var styleAttributes = entry.Adapter.LayerStyle.TextStyle.EncodedAttributes;
            var baseColor = styleAttributes.MsAttributedStringColorAttribute;
            var attributedStringAttributes = (entry.Adapter.Layer as OriginalMasterLayer).AttributedString.Attributes;

            var text = entry.GameObject.GetComponent<Text>();

            // sketchファイルの1テキストに複数のstyleを設定している場合の対応。
            // 複数style設定されている場合はattributedStringAttributes.Lengthが2以上になるので
            // 複数指定されている場合のみ対応を行う。
            if (attributedStringAttributes.Length > 1)
            {
                var addLength = 0;
                foreach (var attributedStringAttribute in attributedStringAttributes)
                {
                    // SketchのTextStyle設定はSize,Color,alignment,Line,Paragraph,Font等があるが、この実装ではUGUI TextのRichTextで対応するのでSizeとColorのみの対応とする。
                    // Style設定の対象となる文字の範囲はattributedStringAttribute.LocationとattributedStringAttribute.Lengthに格納されていて、
                    // TextSizeはattributedStringAttribute.Attributes.MsAttributedStringFontAttribute.Attributes.Sizeに、
                    // TextColorはattributedStringAttribute.Attributes.MsAttributedStringColorAttributeに格納されています。
                    // できる限りRichTextを使わないようにしたいため、ベースのテキスト設定（text.fontSize,text.color）と異なる場合のみRichTextのタグを挿入するようにしています。
                    var startTag = "";
                    var endTag = "";
                    var attributedStringFontAttribute = attributedStringAttribute.Attributes.MsAttributedStringFontAttribute;
                    var attributedStringColorAttribute = attributedStringAttribute.Attributes.MsAttributedStringColorAttribute;
                    if (text.fontSize != (int) attributedStringFontAttribute.Attributes.Size)
                    {
                        startTag =
                            $"<size={(int) attributedStringFontAttribute.Attributes.Size}>" +
                            startTag;
                        endTag += "</size>";
                    }
                    if (!Color.Equality(baseColor, attributedStringColorAttribute))
                    {
                        startTag =
                            $"<color=#{(int) (255.0 * attributedStringColorAttribute.Red):X2}" +
                            $"{(int) (255.0 * attributedStringColorAttribute.Green):X2}" +
                            $"{(int) (255.0 * attributedStringColorAttribute.Blue):X2}" +
                            $"{(int) (255.0 * attributedStringColorAttribute.Alpha):X2}>" +
                            startTag;
                        endTag += "</color>";
                    }
                    text.text = text.text.Insert(addLength + (int) attributedStringAttribute.Location, startTag);
                    addLength += startTag.Length;
                    text.text = text.text.Insert(
                        addLength + (int) attributedStringAttribute.Location + (int) attributedStringAttribute.Length,
                        endTag);
                    addLength += endTag.Length;
                }
            }
        }
    }
}
