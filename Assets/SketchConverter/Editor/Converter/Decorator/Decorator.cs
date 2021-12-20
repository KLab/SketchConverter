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
    /// レイヤー情報からゲームオブジェクトに対して装飾する機能を担う
    /// </summary>
    public abstract class Decorator : IDecorator
    {
        /// <inheritdoc/>
        public abstract bool ShouldDecorate(IDecoratorEntry entry);

        /// <inheritdoc/>
        public virtual bool ShouldBreakingDescendants(IDecoratorEntry entry) => false;

        /// <inheritdoc/>
        public virtual void Decorate(IDecoratorEntry entry)
        {
        }

        /// <inheritdoc/>
        public virtual void DecorateAfter(IDecoratorEntry entry)
        {
        }

        /// <inheritdoc/>
        public virtual void DecorateReverse(IDecoratorEntry entry)
        {
        }

        /// <inheritdoc/>
        public virtual void DecorateReverseAfter(IDecoratorEntry entry)
        {
        }
    }
}
