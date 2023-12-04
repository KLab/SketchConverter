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
using System.Linq;
using SketchConverter.FileFormat;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace SketchConverter
{
    /// <summary>
    /// Sketch ファイルの変換対象を選択するためのツリー構造表示の管理を担う
    /// </summary>
    public class SketchFileTreeView : TreeView
    {
        static readonly Texture2D TextureIconFolder = EditorGUIUtility.FindTexture("Folder Icon");

        static readonly Texture2D TextureIconArtboard =
            AssetDatabase.LoadAssetAtPath<Texture2D>(
                $"{Utility.GetProjectRootPath()}/EditorWindow/Textures/icon_artboard.png");

        static readonly Texture2D TextureIconSymbolMaster =
            AssetDatabase.LoadAssetAtPath<Texture2D>(
                $"{Utility.GetProjectRootPath()}/EditorWindow/Textures/icon_symbol_master.png");

        static readonly Texture2D TextureIconFile =
            AssetDatabase.LoadAssetAtPath<Texture2D>(
                $"{Utility.GetProjectRootPath()}/EditorWindow/Textures/icon_file.png");

        SketchFile sketchFile;

        /// <summary>
        /// SketchFileをロード済みか否か
        /// </summary>
        public bool IsLoaded => sketchFile != null;

        public SketchFileTreeView(TreeViewState treeViewState)
            : base(treeViewState)
        {
            Reload();
        }

        protected override TreeViewItem BuildRoot()
        {
            var id = -1;
            var root = new SketchFileTreeViewItem
            {
                id = id,
                depth = -1,
                displayName = "Root",
            };
            var allItems = new List<TreeViewItem>();
            if (sketchFile != null)
            {
                foreach (var page in sketchFile.Pages)
                {
                    id++;
                    if (!page.HasArtboard())
                    {
                        allItems.Add(new SketchFileTreeViewItem
                        {
                            id = id,
                            depth = 0,
                            displayName = page.Name,
                            icon = TextureIconFile,
                            Layer = page,
                        });
                        continue;
                    }
                    allItems.Add(new SketchFileTreeViewItem
                    {
                        id = id,
                        depth = 0,
                        displayName = page.Name,
                        icon = TextureIconFolder,
                    });
                    foreach (var layer in page.Layers)
                    {
                        id++;
                        var item = new SketchFileTreeViewItem
                        {
                            id = id,
                            depth = 1,
                            displayName = layer.Name,
                            Layer = layer,
                        };
                        if (layer.IsClass(ClassText.Artboard))
                        {
                            item.icon = TextureIconArtboard;
                        }
                        else if (layer.IsClass(ClassText.SymbolMaster))
                        {
                            item.icon = TextureIconSymbolMaster;
                        }
                        else
                        {
                            item.icon = TextureIconFile;
                        }
                        allItems.Add(item);
                    }
                }
            }

            SetupParentsAndChildrenFromDepths(root, allItems);

            return root;
        }

        /// <summary>
        /// SketchFileをロードする
        /// </summary>
        /// <param name="sketchFile">ロードするSketchFileオブジェクト</param>
        public void Load(SketchFile sketchFile)
        {
            this.sketchFile = sketchFile;
            Reload();
        }

        /// <summary>
        /// 選択中のLayerを返します
        /// </summary>
        /// <returns>選択中のLayer</returns>
        public IEnumerable<ILayer> GetSelectedItems()
        {
            var treeViewItems = FindRows(state.selectedIDs);
            foreach (var treeViewItem in treeViewItems)
            {
                yield return ((SketchFileTreeViewItem) treeViewItem).Layer;
            }
        }

        /// <summary>
        /// Layerを選択しているかどうかをチェックする。フォルダを選択している場合はfalseを返す。
        /// </summary>
        /// <returns>Layerを選択している場合はtrueを返す</returns>
        public bool IsSelectedLayer()
        {
            var treeViewItems = FindRows(state.selectedIDs);
            return treeViewItems.Cast<SketchFileTreeViewItem>().Any(item => item.Layer != null);
        }

        /// <summary>
        /// カーソル位置を初期位置に戻す
        /// </summary>
        public void ResetPosition()
        {
            state.selectedIDs = new List<int> { 0 };
        }
    }
}
