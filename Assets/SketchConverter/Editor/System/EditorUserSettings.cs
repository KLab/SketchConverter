using System.Collections.Generic;
using UnityEditorUserSettings = UnityEditor.EditorUserSettings;

namespace SketchConverter
{
    /// <summary>
    /// UnityEditor.EditorUserSettings を利用したデータ保存処理のラップクラス
    /// </summary>
    public static class EditorUserSettings
    {
        static readonly string PreviousOpenedSketchFileKey = "SketchConverter.PreviousOpenedSketchFile";
        static readonly string UsedSketchFontListKey = "SketchConverter.UsedSketchFontList";

        /// <summary>最後に開いたファイルパス</summary>
        public static string PreviousOpenedSketchFile
        {
            get => UnityEditorUserSettings.GetConfigValue(PreviousOpenedSketchFileKey);
            set => UnityEditorUserSettings.SetConfigValue(PreviousOpenedSketchFileKey, value);
        }

        /// <summary>最近使用したSketchフォント</summary>
        public static List<string> UsedSketchFontList
        {
            get
            {
                var value = UnityEditorUserSettings.GetConfigValue(UsedSketchFontListKey);
                return string.IsNullOrEmpty(value) ? new List<string>() : new List<string>(value.Split('\t'));
            }
            set => UnityEditorUserSettings.SetConfigValue(UsedSketchFontListKey, string.Join("\t", value));
        }
    }
}
