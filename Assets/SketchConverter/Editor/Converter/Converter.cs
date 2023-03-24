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
using System.IO;
using SketchConverter.FileFormat;
using UnityEditor;
using UnityEngine;

namespace SketchConverter
{
    /// <summary>
    /// Sketch のデータから Prefab への変換処理を担うクラス
    /// </summary>
    public static class Converter
    {
        /// <summary>.sketch を元にゲームオブジェクトを生成するクラスの初期化処理</summary>
        public static Func<SketchFile, IGenerator> SetupGenerator { get; set; } = DefaultSetupGenerator;

        /// <summary>レイヤーに紐づくゲームオブジェクトに対しての処理を担うクラスの初期化処理</summary>
        public static Action<IGenerator> SetupDecorators { get; set; } = DefaultSetupDecorator;

        /// <summary>
        /// Prefab を生成する
        /// </summary>
        public static GameObject GeneratePrefab(SketchFile file, ILayer pageLayer, string prefabPath)
        {
            var gameObject = GenerateGameObject(file, pageLayer);
            CreateDirectory(Path.GetDirectoryName(prefabPath));
            var prefab = PrefabUtility.SaveAsPrefabAsset(gameObject, prefabPath);
            GameObject.DestroyImmediate(gameObject);
            return prefab;

            void CreateDirectory(string path)
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
            }
        }

        /// <summary>
        /// GameObject を生成する
        /// </summary>
        public static GameObject GenerateGameObject(SketchFile file, ILayer pageLayer)
        {
            var generator = SetupGenerator(file);
            SetupDecorators(generator);
            return generator.Generate(pageLayer);
        }

        /// <summary>
        /// 標準挙動の Generator 初期化処理
        /// </summary>
        public static IGenerator DefaultSetupGenerator(SketchFile file) => new Generator(new Adapter(file), new DecoratorCollection());

        /// <summary>
        /// 標準挙動の Decorator 初期化処理
        /// </summary>
        public static void DefaultSetupDecorator(IGenerator generator)
        {
            generator.Decorators.Add(new RectTransformDecorator());
            generator.Decorators.Add(new NameDecorator());
            generator.Decorators.Add(new SpriteImageDecorator());
            generator.Decorators.Add(new RectangleImageDecorator());
            generator.Decorators.Add(new UguiTextDecorator());
            generator.Decorators.Add(new FillColorDecorator());
            generator.Decorators.Add(new OpacityDecorator());
            generator.Decorators.Add(new InactiveDecorator());
        }
    }
}
