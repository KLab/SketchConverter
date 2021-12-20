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
using UnityEditor;

namespace SketchConverter
{
    public static class SketchConverterMenu
    {
        public const string SketchConverterSettingsProviderName = "Project/Sketch Converter";
        public const string SketchConverterMenuName = "Window/SketchConverter";

        /// <summary>
        /// SketchConverterのメイン画面を生成する関数です。<br />
        /// SketchConverterのメイン画面を拡張する場合は値を変更してください。
        /// </summary>
        public static Func<SketchConverterEditorWindow> SetupSketchConverterWindowFunc = DefaultSetupSketchConverterWindow;

        /// <summary>
        /// SketchConverterの設定画面を生成する関数です。<br />
        /// SketchConverterの設定画面を拡張する場合は値を変更してください。
        /// </summary>
        public static Func<string, SettingsProvider> SetupSketchConverterSettingsProviderFunc = DefaultSetupSketchConverterSettingsProvider;

        [MenuItem(SketchConverterMenuName)]
        public static void OpenSketchConverter()
        {
            var w = SetupSketchConverterWindowFunc();
            w.SetupWindow();
        }

        public static SketchConverterEditorWindow DefaultSetupSketchConverterWindow() => EditorWindow.GetWindow<SketchConverterEditorWindow>();

        [SettingsProvider]
        static SettingsProvider CreateSketchConverterSettingsProvider() => SetupSketchConverterSettingsProviderFunc(SketchConverterSettingsProviderName);

        static SettingsProvider DefaultSetupSketchConverterSettingsProvider(string providerName)
        {
            var provider = new SketchConverterSettingsProvider(providerName, SettingsScope.Project)
            {
                keywords = SettingsProvider.GetSearchKeywordsFromGUIContentProperties<SketchConverterSettingsProvider.Contents>(),
            };
            return provider;
        }
    }
}
