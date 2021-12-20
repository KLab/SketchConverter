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
using UnityEditor;
using UnityEngine;

namespace SketchConverter.Examples
{
    /// <summary>
    /// SketchConverter.Examples の初期設定追加用クラス
    /// </summary>
    class ExampleSettingsInitializer : ScriptableObject
    {
#pragma warning disable 649
        public SketchConverterSettings.Data defaultSettings;
#pragma warning restore 649

        [InitializeOnLoadMethod]
        static void Initialize()
        {
            if (!SketchConverterSettings.Exists())
            {
                EditorApplication.update += InitializeUpdate;
            }
        }

        static void InitializeUpdate()
        {
            if (!SketchConverterSettings.Exists())
            {
                var instance = AssetDatabase
                    .FindAssets($"t:{nameof(ExampleSettingsInitializer)}")
                    .Select(AssetDatabase.GUIDToAssetPath)
                    .Select(AssetDatabase.LoadAssetAtPath<ExampleSettingsInitializer>)
                    .FirstOrDefault(x => x != null);
                if (instance != null)
                {
                    SketchConverterSettings.FontRelations.AddRange(instance.defaultSettings.FontRelations);
                    SketchConverterSettings.TextureDirectories.AddRange(instance.defaultSettings.TextureDirectories);
                    SketchConverterSettings.Save();
                }
            }
            else
            {
                EditorApplication.update -= InitializeUpdate;
            }
        }
    }
}
