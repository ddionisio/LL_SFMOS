using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace LoLExt {
    public class LoLLocalizePostProcess : AssetPostprocessor {
        public class Item {
            public string Key = "";
            public string Value = "";
            public float VoiceDuration = 0f;
            public int MaxChars = 0;

            public bool IsExtraInfoValid() {
                if(VoiceDuration <= 0f)
                    return false;

                return true;
            }

            public LoLLocalize.LanguageExtraInfo CreateExtraInfo() {
                return new LoLLocalize.LanguageExtraInfo() { key = Key, voiceDuration = VoiceDuration };
            }
        }

        public struct MetaItem {
            public int maxChar;

            public MetaItem(Item item) {
                maxChar = item.MaxChars;
            }

            public void WriteToJSON(System.Text.StringBuilder sb) {

            }
        }

        public static bool ApplyAsset(LoLLocalize localize, ref string error) {
            var excelParser = new M8.SpreadsheetParser.ExcelParser(localize.editorExcelPath);

            if(excelParser.sheetCount == 0) {
                error = "No sheets are found.";
                return false;
            }

            var dataLookup = excelParser.DeserializeAllSheets<Item>(null);

            var extraInfoLookup = new Dictionary<string, LoLLocalize.LanguageExtraInfo>();

            var jsonStringBuffer = new System.Text.StringBuilder();

            //populate meta data
            var metaLookup = new Dictionary<string, MetaItem>();

            foreach(var pair in dataLookup) {
                List<Item> items = pair.Value;
                foreach(var item in items) {
                    if(metaLookup.ContainsKey(item.Key))
                        metaLookup[item.Key] = new MetaItem(item);
                    else
                        metaLookup.Add(item.Key, new MetaItem(item));
                }
            }

            int counter = 0;

            //fill JSON

            jsonStringBuffer.Append("{\n");

            //fill in meta data to JSON
            jsonStringBuffer.Append("  \"_meta\": {\n");

            //max chars
            jsonStringBuffer.Append("    \"maxChars\": {");

            counter = 0;
            foreach(var pair in metaLookup) {
                if(pair.Value.maxChar <= 0) //ignore
                    continue;

                if(counter > 0 && counter < metaLookup.Count - 1)
                    jsonStringBuffer.Append(',');

                jsonStringBuffer.Append('\n');
                jsonStringBuffer.Append("      \"").Append(pair.Key).Append("\": \"").Append(pair.Value.maxChar).Append('"');

                counter++;
            }

            jsonStringBuffer.Append('\n').Append("    }\n");
            //

            jsonStringBuffer.Append("  }");
            //

            //fill in languages        
            foreach(var pair in dataLookup) {
                jsonStringBuffer.Append(",\n");

                jsonStringBuffer.Append("  \"").Append(pair.Key).Append("\": {");

                //fill in items
                counter = 0;
                List<Item> items = pair.Value;
                foreach(var item in items) {
                    //add extra info
                    if(item.IsExtraInfoValid()) {
                        if(extraInfoLookup.ContainsKey(item.Key))
                            extraInfoLookup[item.Key] = item.CreateExtraInfo();
                        else
                            extraInfoLookup.Add(item.Key, item.CreateExtraInfo());
                    }

                    //fill item
                    jsonStringBuffer.Append('\n');
                    jsonStringBuffer.Append("    \"").Append(item.Key).Append("\": \"").Append(item.Value).Append('"');

                    counter++;
                    if(counter < items.Count)
                        jsonStringBuffer.Append(',');
                }

                jsonStringBuffer.Append('\n').Append("  }");
            }
            //

            jsonStringBuffer.Append('\n').Append('}');

            //save json
            System.IO.File.WriteAllText(localize.editorLanguagePath, jsonStringBuffer.ToString());

            if(!string.IsNullOrEmpty(localize.editorLanguagePathExtra))
                System.IO.File.WriteAllText(localize.editorLanguagePathExtra, jsonStringBuffer.ToString());

            //save extra info
            var languageExtraInfoList = new List<LoLLocalize.LanguageExtraInfo>();
            foreach(var pair in extraInfoLookup) {
                languageExtraInfoList.Add(pair.Value);
            }

            localize.languageExtraInfos = languageExtraInfoList.ToArray();
            //

            localize.ClearEntries(); //this will refresh lookup for keys

            EditorUtility.SetDirty(localize);

            AssetDatabase.SaveAssets();

            return true;
        }
    }
}