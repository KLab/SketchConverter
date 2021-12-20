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

using UnityEditor;

namespace SketchConverter.Examples
{
    public static class SketchConverterExamplesMenu
    {
        public const string ExtendUguiTextDecoratorExampleLabel = "ExtendUguiTextDecorator";
        public const string CleanAnchorDecoratorExampleLabel = "CleanAnchorDecorator";
        public const string ButtonDecoratorExampleLabel = "ButtonDecorator";

        [MenuItem("Window/SketchConverter.Examples/" + ExtendUguiTextDecoratorExampleLabel)]
        public static void OpenExtendUguiTextDecoratorExample()
        {
            SketchConverterMenu.SetupSketchConverterWindowFunc = EditorWindow.GetWindow<ExampleEditorWindowExtendUguiTextDecorator>;
            SketchConverterMenu.OpenSketchConverter();
            SketchConverterMenu.SetupSketchConverterWindowFunc = SketchConverterMenu.DefaultSetupSketchConverterWindow;
        }

        [MenuItem("Window/SketchConverter.Examples/" + CleanAnchorDecoratorExampleLabel)]
        public static void OpenCleanAnchorDecoratorExample()
        {
            SketchConverterMenu.SetupSketchConverterWindowFunc = EditorWindow.GetWindow<ExampleEditorWindowCleanAnchorDecorator>;
            SketchConverterMenu.OpenSketchConverter();
            SketchConverterMenu.SetupSketchConverterWindowFunc = SketchConverterMenu.DefaultSetupSketchConverterWindow;
        }

        [MenuItem("Window/SketchConverter.Examples/" + ButtonDecoratorExampleLabel)]
        public static void OpenButtonDecoratorExample()
        {
            SketchConverterMenu.SetupSketchConverterWindowFunc = EditorWindow.GetWindow<ExampleEditorWindowButtonDecorator>;
            SketchConverterMenu.OpenSketchConverter();
            SketchConverterMenu.SetupSketchConverterWindowFunc = SketchConverterMenu.DefaultSetupSketchConverterWindow;
        }
    }
}
