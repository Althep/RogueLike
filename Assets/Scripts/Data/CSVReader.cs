using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine.AddressableAssets; // 추가
using UnityEngine.ResourceManagement.AsyncOperations; // 추가
using Cysharp.Threading.Tasks;

public class CSVReader
{
    static string SPLIT_RE = @",(?=(?:[^""]*""[^""]*"")*(?![^""]*""))";
    static string LINE_SPLIT_RE = @"\r\n|\n\r|\n|\r";
    static char[] TRIM_CHARS = { '\"' };

    // 1. 핵심 변경 사항: async Task를 사용하여 비동기로 데이터를 가져옵니다.
    public async UniTask<List<Dictionary<string, object>>> ReadAsync(string address)
    {
        var list = new List<Dictionary<string, object>>();

        // Addressables 핸들을 UniTask로 바로 대기 (더 가볍습니다)
        var handle = Addressables.LoadAssetAsync<TextAsset>(address);
        TextAsset data = await handle;

        if (data == null)
        {
            Debug.LogError($"Load Failed: {address}");
            return null;
        }
        // 파싱

        var lines = Regex.Split(data.text, LINE_SPLIT_RE);

        if (lines.Length <= 1) return list;

        var header = Regex.Split(lines[0], SPLIT_RE);
        for (var i = 1; i < lines.Length; i++)
        {
            var values = Regex.Split(lines[i], SPLIT_RE);
            if (values.Length == 0 || values[0] == "") continue;

            var entry = new Dictionary<string, object>();
            for (var j = 0; j < header.Length && j < values.Length; j++)
            {
                string value = values[j].TrimStart(TRIM_CHARS).TrimEnd(TRIM_CHARS).Replace("\\", "");

                object finalvalue = value;
                int n;
                float f;
                if (int.TryParse(value, out n)) finalvalue = n;
                else if (float.TryParse(value, out f)) finalvalue = f;

                if (header[j] == "#주석#") continue;
                entry[header[j]] = finalvalue;
            }
            list.Add(entry);
        }

        // 사용이 끝난 에셋 핸들 해제 (메모리 관리)
        Addressables.Release(handle);

        return list;
    }
}