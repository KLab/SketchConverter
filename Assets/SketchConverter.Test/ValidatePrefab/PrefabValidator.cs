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

using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using SketchConverter;
using SketchConverter.FileFormat;
using UnityEditor;
using UnityEngine;

public static class PrefabValidator
{
    const string TestPrefabPath = "Assets/SketchConverter.Test.prefab";

    public static void Validate(ValidateData validateData, ValidateTarget parameter)
    {
        Converter.SetupDecorators = parameter.SetupDecorators;
        var correctPrefabPaths = parameter.LayerNames;
        var sketchFile = Loader.LoadSketchFile(validateData.SketchFilePath);
        var layers = GetLayers(sketchFile);
        foreach (var layer in layers)
        {
            try
            {
                var pageName = layer.Name;
                if (!correctPrefabPaths.Contains(pageName))
                {
                    continue;
                }

                Converter.GeneratePrefab(sketchFile, layer, TestPrefabPath);
                var testPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(TestPrefabPath);
                testPrefab.name = pageName;
                var correctPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{validateData.CorrectPrefabsDirectory}/{parameter.Name}/{pageName}.prefab");
                GameObjectComparer.Test(testPrefab, correctPrefab);
            }
            finally
            {
                if (File.Exists(TestPrefabPath))
                {
                    AssetDatabase.DeleteAsset(TestPrefabPath);
                }
            }
        }
        Converter.SetupDecorators = Converter.DefaultSetupDecorator;
    }

    public static ValidateData GetValidateData(string name) => AssetDatabase.FindAssets($"t:{nameof(ValidateData)}")
        .Select(AssetDatabase.GUIDToAssetPath)
        .Select(AssetDatabase.LoadAssetAtPath<ValidateData>)
        .First(x => x.name == name);

    /// <summary>
    /// テストケースとなる sketch ファイルのテスト対象となるレイヤーを返す
    /// </summary>
    static IEnumerable<ILayer> GetLayers(SketchFile sketchFile)
    {
        foreach (var page in sketchFile.Pages)
        {
            if (!page.HasArtboard())
            {
                yield return page;
            }
            else
            {
                foreach (var layer in page.Layers)
                {
                    yield return layer;
                }
            }
        }
    }

    /// <summary>
    /// テスト時に比較するPrefabを自動生成します
    /// </summary>
    public static void GenerateCorrectPrefabs(ValidateData validateData, ValidateTarget[] dataList)
    {
        try
        {
            var sketchFile = Loader.LoadSketchFile(validateData.SketchFilePath);
            var layers = GetLayers(sketchFile).ToArray();

            foreach (var data in dataList)
            {
                foreach (var layer in layers.Where(layer => data.LayerNames.Contains(layer.Name)))
                {
                    Converter.SetupDecorators = data.SetupDecorators;
                    Converter.GeneratePrefab(sketchFile, layer, TestPrefabPath);
                    var testPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(TestPrefabPath);
                    testPrefab.name = layer.Name;
                    var prefabPath = $"{validateData.CorrectPrefabsDirectory}/{data.Name}/{layer.Name}.prefab";
                    var correctPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                    if (correctPrefab == null)
                    {
                        Converter.GeneratePrefab(sketchFile, layer, prefabPath);
                        continue;
                    }

                    try
                    {
                        GameObjectComparer.Test(testPrefab, correctPrefab);
                    }
                    catch (AssertionException)
                    {
                        Converter.GeneratePrefab(sketchFile, layer, prefabPath);
                    }
                }
            }
        }
        finally
        {
            Converter.SetupDecorators = Converter.DefaultSetupDecorator;
            if (File.Exists(TestPrefabPath))
            {
                AssetDatabase.DeleteAsset(TestPrefabPath);
            }
        }
    }
}
