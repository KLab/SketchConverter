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
using System.Text;
using SketchConverter.FileFormat;

namespace SketchConverter
{
    /// <summary>
    /// .sketch を解析して得られた情報が詰まっているクラス
    /// </summary>
    public class SketchFile
    {
        /// <summary>.sketch 全体に関わる情報</summary>
        public DocumentClass Document { get; }

        /// <summary>.sketch のうち Page 単位の情報</summary>
        public PageLayer[] Pages { get; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public SketchFile(DocumentClass document, PageLayer[] pages)
        {
            Document = document;
            Pages = pages;
        }

        /// <summary>
        /// 読み込んだ .sketch ファイルの階層構造が確認できるテキストを返す
        /// </summary>
        public string ToHierarchyString()
        {
            void AppendLayersInfo(StringBuilder sb, ILayer[] layers, int nested = 0)
            {
                foreach (var layer in layers ?? Enumerable.Empty<ILayer>())
                {
                    sb.Append(string.Join("", Enumerable.Repeat("  ", nested).ToArray()));
                    sb.AppendLine($"{layer.Name} ({layer.Frame.X},{layer.Frame.Y},{layer.Frame.Width},{layer.Frame.Height})[{layer.Class}]");
                    AppendLayersInfo(sb, layer.Layers, nested + 1);
                }
            }

            var stringBuilder = new StringBuilder();
            AppendLayersInfo(stringBuilder, Pages);
            return stringBuilder.ToString();
        }
    }
}
