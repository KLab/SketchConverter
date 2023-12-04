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
using SketchConverter.Newtonsoft.Json;
using SketchConverter.Newtonsoft.Json.Linq;

namespace SketchConverter.FileFormat
{
    /// <summary>
    /// ValueConverter の Color 対応版
    /// </summary>
    class SketchConverterValueConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(Value) || t == typeof(Value?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            switch (reader.TokenType)
            {
                case JsonToken.String:
                case JsonToken.Date:
                    var stringValue = serializer.Deserialize<string>(reader);
                    return new Value { String = stringValue };
                case JsonToken.StartObject:
                    var jObject = JObject.Load(reader);
                    var type = (string) jObject.Property("_class");
                    if (type == "color")
                    {
                        var obj = new Color();
                        serializer.Populate(jObject.CreateReader(), obj);
                        return new Value { Color = obj };
                    }
                    else
                    {
                        var obj = new Reference();
                        serializer.Populate(jObject.CreateReader(), obj);
                        return new Value { Reference = obj };
                    }
            }
            throw new Exception("Cannot unmarshal type Value");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            var value = (Value) untypedValue;
            if (value.String != null)
            {
                serializer.Serialize(writer, value.String);
                return;
            }
            if (value.Reference != null)
            {
                serializer.Serialize(writer, value.Reference);
                return;
            }
            if (value.Color != null)
            {
                serializer.Serialize(writer, value.Color);
                return;
            }
            throw new Exception("Cannot marshal type Value");
        }

        public static readonly SketchConverterValueConverter Singleton = new SketchConverterValueConverter();
    }
}
