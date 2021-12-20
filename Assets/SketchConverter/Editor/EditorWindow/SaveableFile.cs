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
using System.Text;
using UnityEditor;

namespace SketchConverter
{
    /// <summary>
    /// データをファイルにセーブ＆ロードを行うクラスです。<br />
    /// 外部からファイルが変更された場合に自動でロードを行う機能も有しています。
    /// </summary>
    public class SaveableFile<TData> where TData : new()
    {
        FileWatcher watcher;
        string settingFilePath;
        public TData Data { get; }

        public SaveableFile(string filePath)
        {
            settingFilePath = filePath;
            Data = new TData();
            Load();
            watcher = new FileWatcher(filePath);
            watcher.Start();
            watcher.Changed += Load;
        }

        /// <summary>設定ファイルがあるかどうか</summary>
        public bool Exists() => File.Exists(settingFilePath);

        /// <summary>
        /// データをファイル保存します
        /// </summary>
        public void Save()
        {
            var json = EditorJsonUtility.ToJson(Data, true);
            watcher.Stop();
            using (var writer = new StreamWriter(settingFilePath, false, Encoding.UTF8))
            {
                writer.Write(json);
            }
            watcher.Start();
        }

        /// <summary>
        /// データをファイルから読み込みます
        /// </summary>
        public void Load()
        {
            if (!Exists())
            {
                return;
            }
            using (var reader = new StreamReader(settingFilePath, Encoding.UTF8))
            {
                var json = reader.ReadToEnd();
                EditorJsonUtility.FromJsonOverwrite(json, Data);
            }
        }

        /// <summary>
        /// ファイル変更の監視クラス
        /// </summary>
        class FileWatcher
        {
            FileSystemWatcher watcher;
            public event Action Changed;

            public FileWatcher(string path)
            {
                watcher = new FileSystemWatcher();
                watcher.Path = Path.GetDirectoryName(path);
                watcher.Filter = Path.GetFileName(path);
                watcher.NotifyFilter = NotifyFilters.LastWrite;
                watcher.Changed += (source, e) => EditorApplication.update += OnChanged;

                void OnChanged()
                {
                    EditorApplication.update -= OnChanged;
                    Changed();
                }
            }

            public void Start() => watcher.EnableRaisingEvents = true;
            public void Stop() => watcher.EnableRaisingEvents = false;
        }
    }
}
