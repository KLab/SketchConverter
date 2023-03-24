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
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace SketchConverter
{
    /// <summary>
    /// SketchConverter の設定ファイルとの仲介クラス
    /// </summary>
    public static class SketchConverterSettings
    {
        [Serializable]
        public class Data
        {
            [SerializeField]
            List<FontRelation> fontRelations = new List<FontRelation>();

            public List<FontRelation> FontRelations => fontRelations;

            [SerializeField]
            List<DefaultAsset> textureDirectories = new List<DefaultAsset>();

            public List<DefaultAsset> TextureDirectories => textureDirectories;

            public FontRelation DefaultFontRelation = new FontRelation("_DefaultFont");
        }

        /// <summary>
        /// SketchConverter のフォント設定
        /// </summary>
        [Serializable]
        public class FontRelation
        {
            [SerializeField]
            string SketchFont;

            [SerializeField]
            Font UnityFont;

            [SerializeField]
            float SketchTextLineRate;

            public bool IsValidUnityFont => UnityFont != null;
            public bool IsValidSketchFont => !string.IsNullOrEmpty(SketchFont);
            public bool IsValidSketchTextLineRate => !Mathf.Approximately(SketchTextLineRate, 0);

            public bool IsValid => IsValidUnityFont &&
                                   IsValidSketchFont &&
                                   IsValidSketchTextLineRate;

            public FontRelation()
            {
            }

            public FontRelation(string newSketchFont) => SketchFont = newSketchFont;

            /// <summary>
            /// SketchのFont名を返します
            /// </summary>
            public string GetSketchFont() => SketchFont;

            /// <summary>
            /// SketchのFont名を設定します
            /// </summary>
            public void SetSketchFont(string font)
            {
                SketchFont = font;
            }

            /// <summary>
            /// UnityのFontを返します
            /// </summary>
            public Font GetUnityFont() => UnityFont;

            /// <summary>
            /// UnityのFontを設定します
            /// </summary>
            public void SetUnityFont(Font font)
            {
                UnityFont = font;
            }

            /// <summary>
            /// SketchデフォルトLine比率を返します
            /// </summary>
            public float GetSketchTextLineRate() => SketchTextLineRate;

            /// <summary>
            /// SketchデフォルトLine比率を設定します
            /// </summary>
            public void SetSketchTextLineRate(float textLineRate)
            {
                SketchTextLineRate = textLineRate;
            }
        }

        /// <summary>
        /// SketchConverter のフォント設定に関する戻り値専用のクラス
        /// </summary>
        public class FontInfo
        {
            public Font Font;
            public float LineRate;
        }

        const string SketchConverterSettingsPath = "ProjectSettings/SketchConverterSettings.json";

        static FontInfo defaultFontInfo;

        /// <summary>
        /// 未設定の場合のデフォルトのフォント設定
        /// </summary>
        static FontInfo DefaultFontInfo
        {
            get => defaultFontInfo ??= new FontInfo
            {
                Font = Resources.GetBuiltinResource<Font>("Arial.ttf"),
                LineRate = 1.117f,
            };
            set => defaultFontInfo = value;
        }

        public static List<FontRelation> FontRelations => saveableFile.Data.FontRelations;

        public static List<DefaultAsset> TextureDirectories => saveableFile.Data.TextureDirectories;

        public static FontRelation DefaultFontRelation => saveableFile.Data.DefaultFontRelation;

        static SaveableFile<Data> saveableFile = new SaveableFile<Data>(SketchConverterSettingsPath);

        /// <summary>設定ファイルがあるかどうか</summary>
        public static bool Exists() => saveableFile.Exists();

        /// <summary>データをファイル保存します</summary>
        public static void Save() => saveableFile.Save();

        /// <summary>
        /// データをファイルから読み込みます
        /// </summary>
        public static void Load() => saveableFile.Load();

        /// <summary>
        /// sketchのフォント名に関連するフォント情報を返します。
        /// </summary>
        /// <param name="fontName">sketchのフォント名</param>
        /// <returns>フォント情報を返す</returns>
        public static FontInfo FindUnityFontBySketchFontInfo(string fontName)
        {
            var settings = saveableFile.Data.FontRelations.FirstOrDefault(x => x.GetSketchFont() == fontName);
            if (settings is {IsValid: true})
            {
                return new FontInfo
                {
                    Font = settings.GetUnityFont(),
                    LineRate = settings.GetSketchTextLineRate(),
                };
            }
            if (settings == null)
            {
                // 関連付け未設定のフォントを保存する
                var usedSketchFontList = EditorUserSettings.UsedSketchFontList;
                usedSketchFontList.RemoveAll(f => f == fontName);
                usedSketchFontList.Insert(0, fontName);
                EditorUserSettings.UsedSketchFontList = usedSketchFontList;
            }
            // フォントが未設定の場合はデフォルトのフォント設定を返す
            return saveableFile.Data.DefaultFontRelation.IsValid
                ? new FontInfo
                {
                    Font = saveableFile.Data.DefaultFontRelation.GetUnityFont(),
                    LineRate = saveableFile.Data.DefaultFontRelation.GetSketchTextLineRate(),
                }
                : DefaultFontInfo;
        }

        /// <summary>DefaultAsset をディレクトリのパスに変換。DefaultAsset がディレクトリでない場合は null を返す</summary>
        public static string ToDirectoryPath(DefaultAsset asset)
        {
            var path = AssetDatabase.GetAssetPath(asset);
            return AssetDatabase.IsValidFolder(path) ? path : null;
        }

        /// <summary>基点となるスプライトのディレクトリパスを返す</summary>
        public static string[] GetTextureDirectoryPaths() => saveableFile.Data.TextureDirectories
            .Select(ToDirectoryPath)
            .Where(x => !string.IsNullOrEmpty(x))
            .ToArray();
    }
}
