﻿/**
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

namespace SketchConverter
{
    /// <summary>
    /// .sketch ファイルをロードする責務を担うインターフェース
    /// </summary>
    public interface ISketchFileLoader
    {
        /// <summary>
        /// .sketch ファイルのパスから .sketch を解析した情報が詰まった SketchFile クラスを返す
        /// </summary>
        SketchFile Load(string filePath);
    }
}
