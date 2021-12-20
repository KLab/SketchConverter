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

namespace SketchConverter
{
    /// <summary>
    /// Project Settings画面に表示されるSketchConverterの設定画面の描画クラス
    /// </summary>
    public class SketchConverterSettingsProvider : SettingsProvider
    {
        public class Contents
        {
            public static readonly GUIContent SpritesDirectoryText = EditorGUIUtility.TrTextContent(@"UIのSpriteが入っているディレクトリを指定してください。
Sketchファイル上のレイヤー名をここで設定したディレクトリからの相対パスにすると、変換時には該当パスのSpriteが使われるようになります。");

            public static readonly GUIContent HelpText =
                EditorGUIUtility.TrTextContent(
                    @"Sketch Converterでフォント情報を正常に読み込むために、
sketchファイル内のフォント名とUnityのフォントの関連付けを行って下さい。
関連付けが未設定のフォントはデフォルトフォントを使って関連付けを行います。");

            public static readonly GUIContent
                SketchFontText = new GUIContent("Sketch Font", "sketchのフォント名を設定して下さい。");

            public static readonly GUIContent UnityFontText =
                new GUIContent("Unity Font", "sketchのフォントと関連付けるUnityのFontを設定して下さい。");

            public static readonly GUIContent SketchTextLineRateText =
                new GUIContent(
                    "SketchデフォルトLine比率",
                    @"sketchのTextのLine設定が未設定の際にデフォルトで設定される値を算出するための比率。
sketchファイルで 「フォントサイズ(1000) ÷ 省略した際のLine値」 で計算した結果の値を設定して下さい。(小数可)");

            public static readonly GUIContent FontRelationErrorText =
                EditorGUIUtility.TrTextContent(
                    @"Fontの関連付けが未設定です。
SketchFontごとに個別で関連付け設定するかデフォルトフォントを設定して下さい。");
        }

        protected static class Styles
        {
            public static readonly GUIStyle BackgroundStyle = new GUIStyle("GroupBox");

            public static readonly GUIStyle ErrorLabelStyle = new GUIStyle
            {
                wordWrap = true,
                margin = new RectOffset(10, 10, 5, 5),
                normal = {textColor = Color.red},
            };

            public static readonly GUIStyle Padding = new GUIStyle {padding = new RectOffset(10, 10, 10, 10)};
        }

        protected bool foldoutFontSection = true;
        protected bool foldoutDirectories = true;

        public SketchConverterSettingsProvider(string path, SettingsScope scope = SettingsScope.User) : base(path, scope)
        {
        }

        public override void OnGUI(string searchContext)
        {
            base.OnGUI(searchContext);
            using (new GUILayout.VerticalScope(Styles.Padding))
            {
                OnGUIContents(searchContext);
            }
        }

        protected virtual void OnGUIContents(string searchContext)
        {
            using (new GUILayout.VerticalScope(GUI.skin.box))
            using (new GUILayout.VerticalScope(Styles.Padding))
            {
                OnGUIDirectories(searchContext);
            }
            EditorGUILayout.Space();
            using (new GUILayout.VerticalScope(GUI.skin.box))
            using (new GUILayout.VerticalScope(Styles.Padding))
            {
                OnGUIFonts(searchContext);
            }
        }

        protected virtual void OnGUIDirectories(string searchContext)
        {
            GUILayout.Label("Sprite 設定", EditorStyles.boldLabel);
            GUILayout.Label(Contents.SpritesDirectoryText, EditorStyles.helpBox);
            EditorGUILayout.Space();
            using (var changeCheckScope = new EditorGUI.ChangeCheckScope())
            {
                foldoutDirectories = EditorGUILayout.Foldout(foldoutDirectories, "設定リスト");
                if (foldoutDirectories)
                {
                    var directories = SketchConverterSettings.TextureDirectories.ToArray();
                    for (var i = 0; i < directories.Length; i++)
                    {
                        var directory = directories[i];
                        using (new GUILayout.HorizontalScope())
                        {
                            var inputDirectory = EditorGUILayout.ObjectField($"{i + 1}", directory, typeof(DefaultAsset), false) as DefaultAsset;
                            if (directory != inputDirectory && SketchConverterSettings.ToDirectoryPath(inputDirectory) != null)
                            {
                                SketchConverterSettings.TextureDirectories[i] = inputDirectory;
                            }
                            if (GUILayout.Button("削除", GUILayout.Width(40)))
                            {
                                SketchConverterSettings.TextureDirectories.Remove(directory);
                            }
                        }
                    }
                }
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("追加", GUILayout.Width(80)))
                    {
                        SketchConverterSettings.TextureDirectories.Add(default);
                    }
                }
                if (changeCheckScope.changed)
                {
                    SketchConverterSettings.Save();
                }
            }
        }

        protected virtual void OnGUIFonts(string searchContext)
        {
            GUILayout.Label("Font 関連付け", EditorStyles.boldLabel);
            GUILayout.Label(Contents.HelpText, EditorStyles.helpBox);
            EditorGUILayout.Space();

            bool HasSketchFontToSettings(string fontName) => SketchConverterSettings.FontRelations.Select(x => x.GetSketchFont()).Contains(fontName);
            var hasSettingDefaultFont = SketchConverterSettings.DefaultFontRelation.IsValid;
            var hasSettingFonts = SketchConverterSettings.FontRelations.All(x => x.IsValid) &&
                                  EditorUserSettings.UsedSketchFontList.All(HasSketchFontToSettings);
            var validFontSettings = hasSettingDefaultFont || hasSettingFonts;

            OnGUIDefaultFontRelation(searchContext, validFontSettings);
            EditorGUILayout.Space();
            OnGUIFontRelations(searchContext);
            EditorGUILayout.Space();
            OnGUIFontNotRelations(searchContext);
        }

        /// <summary>
        /// 関連付け未設定時のデフォルトフォントのGUI描画を行ないます
        /// </summary>
        protected virtual void OnGUIDefaultFontRelation(string searchContext, bool validFontSettings)
        {
            if (!validFontSettings)
            {
                GUILayout.Label(Contents.FontRelationErrorText, Styles.ErrorLabelStyle);
            }

            GUILayout.Label("関連付け未設定時のデフォルトフォント");
            using (new GUILayout.VerticalScope(Styles.BackgroundStyle))
            using (var changeCheckScope = new EditorGUI.ChangeCheckScope())
            {
                // UnityFont
                using (new ErrorColorScope(!SketchConverterSettings.DefaultFontRelation.IsValidUnityFont &&
                                           SketchConverterSettings.DefaultFontRelation.IsValidSketchTextLineRate))
                {
                    var unityFont = EditorGUILayout.ObjectField(Contents.UnityFontText, SketchConverterSettings.DefaultFontRelation.GetUnityFont(),
                        typeof(Font),
                        false) as Font;
                    if (SketchConverterSettings.DefaultFontRelation.GetUnityFont() != unityFont)
                    {
                        SketchConverterSettings.DefaultFontRelation.SetUnityFont(unityFont);
                    }
                }

                // SketchのFontSizeと1行の高さの比率
                using (new ErrorColorScope(SketchConverterSettings.DefaultFontRelation.IsValidUnityFont &&
                                           !SketchConverterSettings.DefaultFontRelation.IsValidSketchTextLineRate))
                {
                    const string zeroStringValue = "";
                    var stringValue = !SketchConverterSettings.DefaultFontRelation.IsValidSketchTextLineRate
                        ? zeroStringValue
                        : SketchConverterSettings.DefaultFontRelation.GetSketchTextLineRate().ToString();
                    var inputStringValue = EditorGUILayout.TextField(Contents.SketchTextLineRateText, stringValue);
                    if (stringValue != inputStringValue && (float.TryParse(inputStringValue, out var value) || inputStringValue == zeroStringValue))
                    {
                        SketchConverterSettings.DefaultFontRelation.SetSketchTextLineRate(value);
                    }
                }

                // 変更があればストレージに保存
                if (changeCheckScope.changed)
                {
                    SketchConverterSettings.Save();
                }
            }
        }

        /// <summary>
        /// 個別フォント関連付け設定リストのGUI描画を行ないます
        /// </summary>
        protected virtual void OnGUIFontRelations(string searchContext)
        {
            foldoutFontSection = EditorGUILayout.Foldout(foldoutFontSection, "個別フォント関連付け設定リスト");
            if (foldoutFontSection)
            {
                foreach (var fontRelation in SketchConverterSettings.FontRelations.ToArray())
                {
                    using (new GUILayout.VerticalScope(Styles.BackgroundStyle))
                    using (var changeCheckScope = new EditorGUI.ChangeCheckScope())
                    {
                        // SketchFont
                        using (new ErrorColorScope(!fontRelation.IsValidSketchFont))
                        {
                            var sketchFont = EditorGUILayout.TextField(Contents.SketchFontText, fontRelation.GetSketchFont());
                            if (fontRelation.GetSketchFont() != sketchFont)
                            {
                                fontRelation.SetSketchFont(sketchFont);
                            }
                        }

                        // UnityFont
                        using (new ErrorColorScope(!fontRelation.IsValidUnityFont))
                        {
                            var unityFont = EditorGUILayout.ObjectField(Contents.UnityFontText, fontRelation.GetUnityFont(), typeof(Font), false) as Font;
                            if (fontRelation.GetUnityFont() != unityFont)
                            {
                                fontRelation.SetUnityFont(unityFont);
                            }
                        }

                        // SketchのFontSizeと1行の高さの比率
                        using (new ErrorColorScope(!fontRelation.IsValidSketchTextLineRate))
                        {
                            const string zeroStringValue = "";
                            var stringValue = !fontRelation.IsValidSketchTextLineRate ? zeroStringValue : fontRelation.GetSketchTextLineRate().ToString();
                            var inputStringValue = EditorGUILayout.TextField(Contents.SketchTextLineRateText, stringValue);
                            if (stringValue != inputStringValue && (float.TryParse(inputStringValue, out var value) || inputStringValue == zeroStringValue))
                            {
                                fontRelation.SetSketchTextLineRate(value);
                            }
                        }

                        // 削除
                        using (new GUILayout.HorizontalScope())
                        {
                            GUILayout.FlexibleSpace();
                            if (GUILayout.Button("削除", GUILayout.Width(80)))
                            {
                                SketchConverterSettings.FontRelations.Remove(fontRelation);
                                // MEMO 削除したTextFieldにFocusが残っていた場合、追加ボタンでFontRelationsを追加した際に削除前の内容が復元されてしまうのでfocusを解除する
                                GUI.FocusControl("");
                            }
                        }

                        // 変更があればストレージに保存
                        if (changeCheckScope.changed)
                        {
                            SketchConverterSettings.Save();
                        }
                    }
                }

                EditorGUILayout.Space();

                using (new GUILayout.HorizontalScope())
                using (var changeCheckScope = new EditorGUI.ChangeCheckScope())
                {
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("追加", GUILayout.Width(80)))
                    {
                        SketchConverterSettings.FontRelations.Add(new SketchConverterSettings.FontRelation());
                    }
                    if (changeCheckScope.changed)
                    {
                        SketchConverterSettings.Save();
                    }
                }
            }
        }

        protected virtual void OnGUIFontNotRelations(string searchContext)
        {
            GUILayout.Label("最後の変換時にSketchで使用されていた関連付け未設定のフォント");
            var usedSketchFontList = EditorUserSettings.UsedSketchFontList;
            using (new GUILayout.VerticalScope(Styles.BackgroundStyle))
            {
                if (usedSketchFontList.Count > 0)
                {
                    foreach (var sketchFont in usedSketchFontList)
                    {
                        using (new GUILayout.HorizontalScope())
                        {
                            var canAdd = SketchConverterSettings.FontRelations.All(fontRelation => fontRelation.GetSketchFont() != sketchFont);
                            EditorGUI.BeginDisabledGroup(!canAdd);
                            if (GUILayout.Button(canAdd ? "追加" : "追加済", GUILayout.Width(80)))
                            {
                                SketchConverterSettings.FontRelations.Add(
                                    new SketchConverterSettings.FontRelation(sketchFont));
                            }
                            EditorGUI.EndDisabledGroup();
                            GUILayout.Label(sketchFont);
                        }
                    }
                }
                else
                {
                    GUILayout.Label("なし");
                }
            }
        }

        /// <summary>項目に問題がある場合に表示を赤くするスコープ</summary>
        protected class ErrorColorScope : GUI.Scope
        {
            Color backgroundColor;
            bool error;

            public ErrorColorScope(bool isError)
            {
                error = isError;
                if (error)
                {
                    backgroundColor = GUI.backgroundColor;
                    GUI.backgroundColor = Color.red;
                }
            }

            protected override void CloseScope()
            {
                if (error)
                {
                    GUI.backgroundColor = backgroundColor;
                }
            }
        }
    }
}
