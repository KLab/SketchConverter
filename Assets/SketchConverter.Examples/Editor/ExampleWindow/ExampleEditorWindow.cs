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

using SketchConverter.FileFormat;
using UnityEngine;

namespace SketchConverter.Examples
{
    /// <summary>
    /// ExtendUguiTextDecorator 動作確認用ウィンドウ
    /// </summary>
    public class ExampleEditorWindowExtendUguiTextDecorator : SketchConverterEditorWindow
    {
        public override void SetupWindow()
        {
            base.SetupWindow();
            titleContent.text = SketchConverterExamplesMenu.ExtendUguiTextDecoratorExampleLabel;
        }

        protected override GameObject GenerateGameObject(ILayer layer)
        {
            Converter.SetupDecorators += generator => generator.Decorators.Replace<UguiTextDecorator>(new ExtendUguiTextDecorator());
            var gameObject = base.GenerateGameObject(layer);
            Converter.SetupDecorators = Converter.DefaultSetupDecorator;
            return gameObject;
        }
    }

    /// <summary>
    /// CleanAnchorDecorator 動作確認用ウィンドウ
    /// </summary>
    public class ExampleEditorWindowCleanAnchorDecorator : SketchConverterEditorWindow
    {
        public override void SetupWindow()
        {
            base.SetupWindow();
            titleContent.text = SketchConverterExamplesMenu.CleanAnchorDecoratorExampleLabel;
        }

        protected override GameObject GenerateGameObject(ILayer layer)
        {
            Converter.SetupDecorators += generator => generator.Decorators.Replace<RectTransformDecorator>(new RectTransformCleanAnchorDecorator());
            var gameObject = base.GenerateGameObject(layer);
            Converter.SetupDecorators = Converter.DefaultSetupDecorator;
            return gameObject;
        }
    }

    /// <summary>
    /// ButtonDecorator 動作確認用ウィンドウ
    /// </summary>
    public class ExampleEditorWindowButtonDecorator : SketchConverterEditorWindow
    {
        public override void SetupWindow()
        {
            base.SetupWindow();
            titleContent.text = SketchConverterExamplesMenu.ButtonDecoratorExampleLabel;
        }

        protected override GameObject GenerateGameObject(ILayer layer)
        {
            Converter.SetupDecorators += generator => generator.Decorators.Add(new ButtonDecorator());
            var gameObject = base.GenerateGameObject(layer);
            Converter.SetupDecorators = Converter.DefaultSetupDecorator;
            return gameObject;
        }
    }
}
