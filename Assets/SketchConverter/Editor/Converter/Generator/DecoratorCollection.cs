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
using System.Collections;
using System.Collections.Generic;

namespace SketchConverter
{
    /// <summary>
    /// 複数の Decorator を管理する責務を担う
    /// </summary>
    public class DecoratorCollection : IEnumerable<IDecorator>
    {
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public IEnumerator<IDecorator> GetEnumerator() => List.GetEnumerator();

        /// <summary>全ての Decorator を持つリスト</summary>
        protected virtual List<IDecorator> List { get; } = new List<IDecorator>();

        /// <summary>レイヤーの Decorator を返す</summary>
        public virtual IReadOnlyList<IDecorator> All => List;

        /// <summary>
        /// 要素の追加
        /// </summary>
        public virtual void Add(IDecorator decorator)
        {
            List.Add(decorator);
        }

        /// <summary>
        /// 要素の削除
        /// </summary>
        public virtual void Remove(IDecorator decorator)
        {
            List.Remove(decorator);
        }

        /// <summary>
        /// 要素の置換。最初に当てはまった T 型を置き換える
        /// </summary>
        /// <param name="decorator">置き換え後の Decorator</param>
        /// <typeparam name="T">置き換え対象の型。最初に当てはまったインスタンスのみが置き換えられる</typeparam>
        public virtual void Replace<T>(IDecorator decorator) where T : IDecorator
        {
            var index = FirstIndex(List, x => x.GetType() == typeof(T));
            if (index < 0)
            {
                throw new InvalidOperationException();
            }
            List[index] = decorator;
        }

        /// <summary>
        /// 要素の挿入。最初に当てはまった T 型の次に挿入される
        /// </summary>
        /// <typeparam name="T">挿入基準となる型。最初に当てはまったインスタンスの次に挿入される</typeparam>
        public virtual void InsertAfter<T>(IDecorator decorator) where T : IDecorator
        {
            var index = FirstIndex(List, x => x.GetType() == typeof(T));
            if (index < 0)
            {
                throw new InvalidOperationException();
            }
            List.Insert(index + 1, decorator);
        }

        /// <summary>
        /// 要素の挿入。最初に当てはまった T 型の前に挿入される
        /// </summary>
        /// <typeparam name="T">挿入基準となる型。最初に当てはまったインスタンスの前に挿入される</typeparam>
        public virtual void InsertBefore<T>(IDecorator decorator) where T : IDecorator
        {
            var index = FirstIndex(List, x => x.GetType() == typeof(T));
            if (index < 0)
            {
                throw new InvalidOperationException();
            }
            List.Insert(index, decorator);
        }

        /// <summary>
        /// 条件に当てはまる最初のインデックスを得る
        /// </summary>
        protected virtual int FirstIndex<T>(IEnumerable<T> enumerable, Predicate<T> match)
        {
            using (var enumerator = enumerable.GetEnumerator())
            {
                var index = 0;
                while (enumerator.MoveNext())
                {
                    if (match(enumerator.Current))
                    {
                        return index;
                    }
                    index++;
                }
                return -1;
            }
        }

        /// <summary>
        /// 保持している Decorator のソートを行う。Decorator の実行順序を正しくするために必要な処理。
        /// </summary>
        public virtual void Sort()
        {
            List.Sort(Compare);
        }

        /// <summary>
        /// Decorator ソート用の比較ロジック
        /// </summary>
        public virtual int Compare(IDecorator a, IDecorator b) => 0;
    }
}
