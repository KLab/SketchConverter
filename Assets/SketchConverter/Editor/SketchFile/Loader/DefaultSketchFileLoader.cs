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
using System.IO;
using System.Linq;
using SketchConverter.FileFormat;
using SketchConverter.ICSharpCode.SharpZipLib.Zip;
using SketchConverter.Newtonsoft.Json;

namespace SketchConverter
{
    /// <summary>
    /// .sketch ファイルの読み込みを担う
    /// </summary>
    public class DefaultSketchFileLoader : ISketchFileLoader
    {
        /// <summary>.sketch の解凍先となる一時フォルダ</summary>
        protected virtual string UnzippedDirectory { get; set; }

        /// <inheritdoc/>
        public virtual SketchFile Load(string filePath)
        {
            UnzippedDirectory = SketchFilePathToUnzipDirectory(filePath);
            UnzipSketchFile(filePath);
            var sketchFile = AnalyzeSketchFile();
            AnalyzeSketchFilePostprocess(sketchFile);
            DeleteUnzippedSketchFile();
            return sketchFile;
        }

        /// <summary>
        /// .sketch ファイルのパスから解凍先のディレクトリパスを返す
        /// </summary>
        protected virtual string SketchFilePathToUnzipDirectory(string filePath)
        {
            var fileDirectory = Path.GetDirectoryName(filePath);
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            return $"{fileDirectory}/{fileName}";
        }

        /// <summary>
        /// .sketch ファイルを zip 解凍する
        /// </summary>
        protected virtual void UnzipSketchFile(string sketchFilePath)
        {
            if (!File.Exists(sketchFilePath))
            {
                throw new FileNotFoundException($"Not found: {sketchFilePath}");
            }

            if (Path.GetExtension(sketchFilePath) != ".sketch")
            {
                throw new ArgumentException($"Not .sketch file: {sketchFilePath}");
            }

            if (Directory.Exists(UnzippedDirectory))
            {
                Directory.Delete(UnzippedDirectory, true);
            }

            var fastzip = new FastZip();
            fastzip.ExtractZip(sketchFilePath, UnzippedDirectory, "");
        }

        /// <summary>
        /// zip 解凍したデータを削除する
        /// </summary>
        protected virtual void DeleteUnzippedSketchFile()
        {
            if (Directory.Exists(UnzippedDirectory))
            {
                Directory.Delete(UnzippedDirectory, true);
            }
        }

        /// <summary>
        /// zip 解凍したデータを解析して結果を SketchFile クラスとして返す
        /// </summary>
        protected virtual SketchFile AnalyzeSketchFile()
        {
            if (!Directory.Exists(UnzippedDirectory))
            {
                throw new DirectoryNotFoundException($"Not found: {UnzippedDirectory}");
            }

            var settings = new JsonSerializerSettings
            {
                MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
                DateParseHandling = DateParseHandling.None,
                Converters = {SketchConverterValueConverter.Singleton},
            };
            var document = JsonConvert.DeserializeObject<DocumentClass>(File.ReadAllText($"{UnzippedDirectory}/document.json"), settings);
            var pages = document.Pages
                .Select(x => JsonConvert.DeserializeObject<PageLayer>(File.ReadAllText($"{UnzippedDirectory}/{x.Ref}.json"), settings))
                .ToArray();
            var sketchFile = CreateSketchFile(document, pages);
            return sketchFile;
        }

        /// <summary>
        /// SketchFile クラスを作成して返す
        /// </summary>
        protected virtual SketchFile CreateSketchFile(DocumentClass document, PageLayer[] pages) => new SketchFile(document, pages);

        /// <summary>
        /// SketchFile クラス生成後の処理
        /// </summary>
        protected virtual void AnalyzeSketchFilePostprocess(SketchFile sketchFile)
        {
            // .sketch の情報をそのまま読んで扱うと内部的なレイヤー順序が逆になるので直す処理
            void FixLayersOrder(SketchFile file)
            {
                foreach (var page in file.Pages)
                {
                    if (page.Layers != null)
                    {
                        Array.Reverse(page.Layers);
                    }
                }
            }

            FixLayersOrder(sketchFile);
        }
    }
}
