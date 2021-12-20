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

namespace SketchConverter
{
    /// <summary>
    /// .sketch ファイルの読み込みを担う
    /// </summary>
    public static class Loader
    {
        /// <summary>.sketch ファイルを読み込むローダーの初期化処理</summary>
        public static Func<ISketchFileLoader> SetupLoader { get; set; } = DefaultSetupLoader;

        /// <summary>
        /// .sketch を解析した情報が詰まった SketchFile クラスを返す
        /// </summary>
        /// <param name="path">.sketch ファイルのパス</param>
        public static SketchFile LoadSketchFile(string path)
        {
            var loader = SetupLoader();
            var sketchFile = loader.Load(path);
            return sketchFile;
        }

        /// <summary>
        /// ローダーの初期化処理
        /// </summary>
        public static ISketchFileLoader DefaultSetupLoader() => new DefaultSketchFileLoader();
    }
}
