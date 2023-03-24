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
using NUnit.Framework;
using SketchConverter;
using SketchConverter.Examples;
using UnityEditor;

public class ValidatePrefabTestCase
{
    const string ValidateDataName = "ValidateData";

    static ValidateTarget[] cases =
    {
        new ValidateTarget
        {
            Name = "Default",
            LayerNames = new[]
            {
                "rect-transform",
                "anchor",
                "text-library",
                "text-symbol",
                "rectangle",
                "sprites",
                "colors",
                "symbols",
                "override",
            },
            SetupDecorators = Converter.DefaultSetupDecorator,
        },
        new ValidateTarget
        {
            Name = "ExtendUguiTextDecorator",
            LayerNames = new[]
            {
                "text-library-extend-ugui",
                "text-symbol",
            },
            SetupDecorators = generator =>
            {
                Converter.DefaultSetupDecorator(generator);
                generator.Decorators.Replace<UguiTextDecorator>(new ExtendUguiTextDecorator());
            },
        },
        new ValidateTarget
        {
            Name = "CleanAnchorDecorator",
            LayerNames = new[] {"anchor"},
            SetupDecorators = generator =>
            {
                Converter.DefaultSetupDecorator(generator);
                generator.Decorators.Replace<RectTransformDecorator>(new RectTransformCleanAnchorDecorator());
            },
        },
        new ValidateTarget
        {
            Name = "ButtonDecorator",
            LayerNames = new[] {"text-library"},
            SetupDecorators = generator =>
            {
                Converter.DefaultSetupDecorator(generator);
                generator.Decorators.Add(new ButtonDecorator());
            },
        },
        new ValidateTarget
        {
            Name = "MaskDecorator",
            LayerNames = new[] {"mask"},
            SetupDecorators = generator =>
            {
                Converter.DefaultSetupDecorator(generator);
                generator.Decorators.Add(new MaskDecorator());
            },
        },
        new ValidateTarget
        {
            Name = "DestroyDecorator",
            LayerNames = new[] {"text-library"},
            SetupDecorators = generator =>
            {
                Converter.DefaultSetupDecorator(generator);
                generator.Decorators.Add(new DestroyDecorator());
            },
        },
    };

    public void Test(ValidateTarget target) => PrefabValidator.Validate(PrefabValidator.GetValidateData(ValidateDataName), target);

    public ValidateTarget GetTarget(string name) => cases.First(x => x.Name == name);

    [Test]
    public void DefaultDecoratorPasses()
    {
        Test(GetTarget("Default"));
    }

    [Test]
    public void ExtendUguiTextDecoratorPasses()
    {
        Test(GetTarget("ExtendUguiTextDecorator"));
    }

    [Test]
    public void CleanAnchorTextDecoratorPasses()
    {
        Test(GetTarget("CleanAnchorDecorator"));
    }

    [Test]
    public void ButtonDecoratorPasses()
    {
        Test(GetTarget("ButtonDecorator"));
    }

    [Test]
    public void MaskDecoratorPasses()
    {
        Test(GetTarget("MaskDecorator"));
    }

    [Test]
    public void DestroyDecoratorPasses()
    {
        Test(GetTarget("DestroyDecorator"));
    }

    /// <summary>
    /// テスト時に比較するPrefabの自動生成
    /// </summary>
    [MenuItem("SketchConverter/Test/GenerateCorrectPrefabs")]
    public static void GenerateCorrectPrefabs()
    {
        PrefabValidator.GenerateCorrectPrefabs(PrefabValidator.GetValidateData(ValidateDataName), cases);
    }
}
