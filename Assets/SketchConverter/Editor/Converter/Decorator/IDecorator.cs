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

namespace SketchConverter
{
    /// <summary>
    /// レイヤー情報からゲームオブジェクトに対して装飾する機能を担う基底のインターフェース
    /// </summary>
    public interface IDecorator
    {
        /// <summary>ゲームオブジェクトに対する装飾処理をすべきかどうか</summary>
        bool ShouldDecorate(IDecoratorEntry entry);

        /// <summary>子孫要素の生成を打ち切るかどうか</summary>
        bool ShouldBreakingDescendants(IDecoratorEntry entry);

        /// <summary>ゲームオブジェクトに対する装飾処理</summary>
        void Decorate(IDecoratorEntry entry);

        /// <summary>ゲームオブジェクトに対する装飾処理。Decorate を全てのレイヤーに対して行った後の実行</summary>
        void DecorateAfter(IDecoratorEntry entry);

        /// <summary>ゲームオブジェクトに対する装飾処理。子が生成された後のタイミングとなる</summary>
        void DecorateReverse(IDecoratorEntry entry);

        /// <summary>ゲームオブジェクトに対する装飾処理。DecorateReverse を全てのレイヤーに対して行った後の実行</summary>
        void DecorateReverseAfter(IDecoratorEntry entry);
    }
}
