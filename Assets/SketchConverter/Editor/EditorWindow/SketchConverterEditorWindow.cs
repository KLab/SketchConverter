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
using System.IO;
using System.Linq;
using SketchConverter.FileFormat;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace SketchConverter
{
    /// <summary>
    /// SketchConverter のメインウィンドウ
    /// </summary>
    public class SketchConverterEditorWindow : EditorWindow
    {
        protected static class Constants
        {
            public static GUIStyle background;
            public static GUIStyle button;
            public static Texture2D textureIconSetting;
            public static GUIStyle padding;
            public static GUIStyle wordWrapLabel;
            public static GUIStyle wordWrapMiniLabel;
            public static GUIStyle dragAndDropButton;
            public static GUIStyle settingButton;
            public static string selectSketchFileStepLabel = "STEP1";
            public static string convertSketchFileStepLabel = "STEP2";

            static Constants()
            {
                background = new GUIStyle("Box") {padding = new RectOffset(8, 8, 8, 8)};
                button = new GUIStyle("Button") {padding = new RectOffset(0, 0, 8, 8)};
                padding = new GUIStyle {padding = new RectOffset(8, 8, 8, 8)};
                textureIconSetting = EditorGUIUtility.IconContent("SettingsIcon").image as Texture2D;
                wordWrapLabel = new GUIStyle(EditorStyles.label) {wordWrap = true};
                wordWrapMiniLabel = new GUIStyle(EditorStyles.miniLabel) {wordWrap = true};
                dragAndDropButton = new GUIStyle("Button") {padding = new RectOffset(0, 0, 15, 15)};
                settingButton = EditorStyles.toolbarButton;
            }
        }

        protected SketchFile sketchFile;
        protected string currentOpenSketchFilePath;

        protected SketchFileTreeView sketchFileTreeView;
        protected TreeViewState pageTreeState;

        protected bool shownExceptions;
        protected bool shownExceptionsStackTrace;
        protected readonly List<Exception> exceptions = new List<Exception>();
        protected Vector2 mainViewScrollPosition = Vector2.zero;
        protected Vector2 exceptionViewScrollPosition = Vector2.zero;

        static readonly string SketchFilePathKey = "SketchFilePathKey";

        public virtual void SetupWindow()
        {
            var icon = AssetDatabase.LoadAssetAtPath<Texture2D>(
                $"{Utility.GetProjectRootPath()}/EditorWindow/Textures/icon_sketch.png");
            titleContent = new GUIContent("SketchConverter", icon);
            minSize = new Vector2(250, 310);
        }

        protected virtual void OnEnable()
        {
            pageTreeState = pageTreeState ?? new TreeViewState();
            sketchFileTreeView = sketchFileTreeView ?? new SketchFileTreeView(pageTreeState);
            currentOpenSketchFilePath = "";
            shownExceptions = false;
        }

        protected virtual void OnGUI()
        {
            if (!shownExceptions)
            {
                try
                {
                    OnGUIMain();
                }
                catch (ExitGUIException)
                {
                    throw;
                }
                catch (Exception exception)
                {
                    shownExceptions = true;
                    exceptions.Add(exception);
                }
            }
            else
            {
                OnGUIException();
            }
        }

        protected virtual void OnGUIMain()
        {
            OnGUIToolBar();
            using (var scrollView = new EditorGUILayout.ScrollViewScope(mainViewScrollPosition, Constants.padding))
            {
                mainViewScrollPosition = scrollView.scrollPosition;
                OnGUIContents();
            }
        }

        protected virtual void OnGUIContents()
        {
            using (new EditorGUILayout.VerticalScope(Constants.background))
            {
                OnGUISelectSketchFile();
            }
            if (sketchFileTreeView.IsLoaded)
            {
                GUILayout.Space(8);
                using (new EditorGUILayout.VerticalScope(Constants.background))
                {
                    OnGUIConvertSketchFile();
                }
            }
        }

        protected virtual void OnGUIToolBar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(new GUIContent
                {
                    text = "️設定",
                    image = Constants.textureIconSetting,
                }, Constants.settingButton))
                {
                    OpenSettingWindow();
                }
            }
        }

        protected virtual void OnGUISelectSketchFile()
        {
            GUILayout.Label(Constants.selectSketchFileStepLabel, EditorStyles.boldLabel);
            var path = Path.GetFileName(currentOpenSketchFilePath);
            if (string.IsNullOrEmpty(path))
            {
                GUILayout.Label(".sketchファイルを選択して下さい。");
            }
            else
            {
                GUILayout.Label(Path.GetFileName(currentOpenSketchFilePath));
            }
            if (GUILayout.Button(".sketch ファイル読み込み\n(このボタンにドラッグ＆ドロップも可能)", Constants.dragAndDropButton))
            {
                OpenSketchFileDialog();
                GUIUtility.ExitGUI();
            }
            if (SketchConverterGUILayout.DraggableRect(GUILayoutUtility.GetLastRect()))
            {
                OpenSketchFile(DragAndDrop.paths[0]);
                GUIUtility.ExitGUI();
            }
            if (sketchFileTreeView.IsLoaded && GUILayout.Button("再読み込み", Constants.button))
            {
                ReopenSketchFile();
            }
        }

        protected virtual void OnGUIConvertSketchFile()
        {
            GUILayout.Label(Constants.convertSketchFileStepLabel, EditorStyles.boldLabel);
            GUILayout.Label("Convertする画面を選択して下さい。");

            using (new EditorGUILayout.VerticalScope(GUI.skin.box))
            {
                GUILayout.Space(0); // GUILayoutUtility.GetRect のエラー回避
                var rect = GUILayoutUtility.GetRect(GUILayoutUtility.GetLastRect().width, 0, GUILayout.ExpandHeight(true));
                sketchFileTreeView.OnGUI(rect);
            }
            GUI.enabled = sketchFileTreeView.IsSelectedLayer();
            if (GUILayout.Button("選択した画面をHierarchyに出力", Constants.button))
            {
                GenerateGameObjects(sketchFileTreeView.GetSelectedItems());
            }
            GUI.enabled = true;
            GUILayout.Space(2);
        }

        /// <summary>
        /// 設定画面を開く
        /// </summary>
        protected virtual void OpenSettingWindow()
        {
            SettingsService.OpenProjectSettings(SketchConverterMenu.SketchConverterSettingsProviderName).Focus();
        }

        /// <summary>
        /// sketchファイルのファイル選択ダイアログを開いて、sketchファイルをロードを行います。
        /// </summary>
        protected virtual void OpenSketchFileDialog()
        {
            // 今回開いたsketchファイルをロード
            var filePath = EditorUserSettings.PreviousOpenedSketchFile;
            var openDir = string.IsNullOrEmpty(filePath) ? "" : Path.GetDirectoryName(filePath);

            // .sketch の読み込み
            var path = EditorUtility.OpenFilePanel(".Sketchファイルを開く", openDir, "sketch");
            if (!string.IsNullOrEmpty(path))
            {
                OpenSketchFile(path);
            }
        }

        /// <summary>
        /// sketchファイルをロードします。
        /// </summary>
        protected virtual void OpenSketchFile(string path)
        {
            if (!path.EndsWith(".sketch"))
            {
                throw new Exception($"未対応のファイルフォーマットです。\nFile:{path}");
            }
            LoadSketchFile(path);
            currentOpenSketchFilePath = path;
            SessionState.SetString(SketchFilePathKey, path);
            // 今回開いたファイルを記憶
            EditorUserSettings.PreviousOpenedSketchFile = path;

            // カーソルを初期位置に戻す
            sketchFileTreeView.ResetPosition();
        }

        /// <summary>
        /// 現在開いているsketchファイルを再度開き直す
        /// </summary>
        protected virtual void ReopenSketchFile()
        {
            if (string.IsNullOrEmpty(currentOpenSketchFilePath) == false &&
                File.Exists(currentOpenSketchFilePath))
            {
                LoadSketchFile(currentOpenSketchFilePath);
            }
            else
            {
                EditorUtility.DisplayDialog("エラー", "ファイルが見つかりませんでした。", "OK");
            }
        }

        /// <summary>
        /// sketchファイルをロードして、Artboardリストを更新する
        /// </summary>
        /// <param name="sketchPath">sketchファイルのパス</param>
        protected virtual void LoadSketchFile(string sketchPath)
        {
            sketchFile = Loader.LoadSketchFile(sketchPath);
            sketchFileTreeView.Load(sketchFile);
        }

        /// <summary>
        /// 選択したArtboardからGameObjectを作成します
        /// </summary>
        protected virtual void GenerateGameObjects(IEnumerable<ILayer> layers)
        {
            var isError = false;
            EditorUserSettings.UsedSketchFontList = new List<string>();
            foreach (var layer in layers)
            {
                if (layer != null)
                {
                    var gameObject = GenerateGameObject(layer);
                    if (!SketchConverterSettings.DefaultFontRelation.IsValid && EditorUserSettings.UsedSketchFontList.Count > 0)
                    {
                        DestroyImmediate(gameObject);
                        isError = true;
                    }
                }
            }
            if (isError)
            {
                SettingsService.OpenProjectSettings(SketchConverterMenu.SketchConverterSettingsProviderName).Focus();
                throw new Exception(
                    @"Sketchファイル内で使用されているフォントでSketchConverterのフォント関連設定が行われていないフォントが存在します。
設定画面でフォント関連設定を行って下さい。");
            }
        }

        protected virtual GameObject GenerateGameObject(ILayer layer) => Converter.GenerateGameObject(sketchFile, layer);

        protected virtual void OnGUIException()
        {
            using (new EditorGUILayout.VerticalScope(Constants.padding))
            {
                GUILayout.Label("エラー発生", EditorStyles.largeLabel);
                using (var scrollView = new EditorGUILayout.ScrollViewScope(exceptionViewScrollPosition, GUILayout.ExpandHeight(true)))
                {
                    exceptionViewScrollPosition = scrollView.scrollPosition;
                    foreach (var exception in exceptions.AsEnumerable().Reverse())
                    {
                        using (new EditorGUILayout.VerticalScope(Constants.background))
                        {
                            EditorGUILayout.TextArea(exception.Message, Constants.wordWrapLabel, GUILayout.ExpandWidth(true));
                            shownExceptionsStackTrace = EditorGUILayout.Foldout(shownExceptionsStackTrace, "StackTrace");
                            if (shownExceptionsStackTrace)
                            {
                                EditorGUILayout.TextArea(exception.StackTrace, Constants.wordWrapMiniLabel, GUILayout.ExpandWidth(true));
                            }
                        }
                    }
                }

                if (GUILayout.Button("戻る", Constants.button))
                {
                    Resume();
                }
            }

            void Resume()
            {
                shownExceptions = false;
                exceptions.Clear();
            }
        }

        [DidReloadScripts]
        static void CompileReopenSketchFile()
        {
            var windowList = Resources.FindObjectsOfTypeAll<SketchConverterEditorWindow>();

            if (windowList.Any())
            {
                var path = SessionState.GetString(SketchFilePathKey, "");
                if (!string.IsNullOrEmpty(path))
                {
                    windowList.First().OpenSketchFile(path);
                }
            }
        }
    }
}
