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
using SketchConverter.FileFormat;

namespace SketchConverter
{
    /// <summary>
    /// オーバーライド値とそれに関連する状態管理を担う
    /// </summary>
    public interface IOverrideValueAdapter
    {
        ParsedOverrideValue OverrideValue { get; }
        Value Value { get; }
        string OverrideProperty { get; }

        /// <summary>オーバーライド値を適用しないことが確定しているかどうか</summary>
        bool IsInvalid { get; }

        /// <summary>オーバーライドの適用先となるパスを経由しているかを確認する</summary>
        void Trace(ILayer symbolInstance);

        /// <summary>オーバーライドの適用が制限されているかを反映する</summary>
        void ApplyProperties(ParsedOverrideProperty[] properties);

        /// <summary>引数のレイヤーにオーバーライド情報を適用して良いかを返す</summary>
        bool CanOverride(ILayer layer);

        /// <summary>コピーコンストラクタをコールする</summary>
        IOverrideValueAdapter Clone();
    }

    public class OverrideValueAdapter : IOverrideValueAdapter
    {
        /// <summary>オーバーライドの適用先となるパスをどこまで経由したかを示す</summary>
        protected int traceIndex;

        public bool IsInvalid { get; protected set; }
        public ParsedOverrideValue OverrideValue { get; }
        public Value Value => OverrideValue.Value;
        public string OverrideProperty => OverrideValue.OverrideProperty;
        protected bool hasTraceId => traceIndex < OverrideValue.OverrideSymbolIds.Length;
        protected string traceId => OverrideValue.OverrideSymbolIds[traceIndex];
        protected string lastId => OverrideValue.OverrideObjectId;

        public OverrideValueAdapter(ParsedOverrideValue overrideValue) => OverrideValue = overrideValue;

        public OverrideValueAdapter(OverrideValueAdapter other)
        {
            OverrideValue = other.OverrideValue;
            traceIndex = other.traceIndex;
            IsInvalid = other.IsInvalid;
        }

        public virtual void Trace(ILayer symbolInstance)
        {
            if (!hasTraceId || IsInvalid)
            {
                return;
            }
            if (symbolInstance.DoObjectId == traceId)
            {
                traceIndex++;
            }
            else
            {
                IsInvalid = true;
            }
        }

        public virtual void ApplyProperties(ParsedOverrideProperty[] properties)
        {
            if (IsInvalid)
            {
                return;
            }
            var currentName = $"{string.Join("/", OverrideValue.OverrideSymbolIds.Skip(traceIndex).Concat(new[] {lastId}).ToArray())}_{OverrideProperty}";
            if (properties.Any(x => x.OverrideName == currentName && x.CanOverride == false))
            {
                IsInvalid = true;
            }
        }

        public virtual bool CanOverride(ILayer layer) => !hasTraceId && !IsInvalid && layer.DoObjectId == lastId;

        public virtual IOverrideValueAdapter Clone() => new OverrideValueAdapter(this);
    }
}
