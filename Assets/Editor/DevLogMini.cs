using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Linq; // 목록 정렬을 위해 추가
using System.Text;

public class DevLogMini : EditorWindow
{
    // 입력 데이터를 저장할 변수들
    string classesWorkedOn = "";
    string tasksDone = "";
    string nextTasks = "";

    // 파일 목록 관리를 위한 변수들
    string[] logFilePaths = new string[0]; // 실제 파일 경로들
    string[] logFileNames = new string[0]; // 화면에 보여줄 날짜 이름들 (예: 2026-05-15)
    int selectedFileIndex = 0;             // 드롭다운에서 선택된 항목의 번호

    // 현재 편집 중인 기준 날짜 (오늘이 아닐 수도 있으므로 기록해둠)
    string currentEditingDate = "";

    [MenuItem("Tools/개발 일지 작성기 V3")]
    public static void ShowWindow()
    {
        var window = GetWindow<DevLogMini>("개발 일지");
        window.RefreshFileList(); // 창을 열 때 파일 목록을 한번 쫙 읽어옵니다.
    }

    // 유니티가 로딩되거나 스크립트가 리로드될 때 자동으로 실행되는 함수
    void OnEnable()
    {
        RefreshFileList();
        // 편집 중인 날짜가 비어있으면 무조건 '오늘'로 세팅
        if (string.IsNullOrEmpty(currentEditingDate))
        {
            currentEditingDate = DateTime.Now.ToString("yyyy-MM-dd");
        }
    }

    // DevLogs 폴더 안의 txt 파일들을 읽어서 최신순으로 정렬하는 핵심 로직
    void RefreshFileList()
    {
        string path = Path.Combine(Application.dataPath, "../DevLogs");
        if (!Directory.Exists(path)) Directory.CreateDirectory(path);

        // "DevLog_"로 시작하는 모든 txt 파일을 찾아 내림차순(최신순)으로 정렬합니다.
        var files = Directory.GetFiles(path, "DevLog_*.txt").OrderByDescending(f => f).ToArray();

        logFilePaths = files;
        logFileNames = new string[files.Length];

        for (int i = 0; i < files.Length; i++)
        {
            // 화면에 깔끔하게 보이기 위해 파일명에서 "DevLog_"와 ".txt"를 떼어냅니다.
            logFileNames[i] = Path.GetFileNameWithoutExtension(files[i]).Replace("DevLog_", "");
        }

        // 에러 방지용 인덱스 초기화
        if (selectedFileIndex >= logFileNames.Length) selectedFileIndex = 0;
    }

    void OnGUI()
    {
        // 상단에 내가 지금 '며칠 자' 일지를 쓰고 있는지 명확하게 띄워줍니다.
        GUILayout.Label($"[현재 편집 중: {currentEditingDate}]", EditorStyles.boldLabel);
        GUILayout.Space(10);

        classesWorkedOn = EditorGUILayout.TextField("작업한 클래스:", classesWorkedOn);

        GUILayout.Space(5);
        GUILayout.Label("오늘 한 일:");
        tasksDone = EditorGUILayout.TextArea(tasksDone, GUILayout.Height(100));

        GUILayout.Space(5);
        GUILayout.Label("내일 할 일 (To-do):", EditorStyles.boldLabel);
        nextTasks = EditorGUILayout.TextArea(nextTasks, GUILayout.Height(60));

        GUILayout.Space(10);

        // ---------------- [저장 버튼] ----------------
        // 이 버튼을 누르면 무조건 'currentEditingDate' 날짜 파일에 덮어씌워집니다.
        if (GUILayout.Button($"'{currentEditingDate}' 일지 저장 및 복사", GUILayout.Height(40)))
        {
            SaveReport();
            RefreshFileList(); // 혹시 오늘 처음 저장한 거라면 파일 목록에 추가해야 하므로 새로고침
        }

        GUILayout.Space(20);

        // ---------------- [파일 목록 및 불러오기 구획] ----------------
        GUILayout.Label("과거 일지 불러오기", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal(); // 여기서부터 가로 배치 시작

        if (GUILayout.Button("?? 갱신", GUILayout.Width(50)))
        {
            RefreshFileList();
        }

        // 파일이 1개라도 있을 때만 드롭다운 메뉴를 띄웁니다.
        if (logFileNames.Length > 0)
        {
            selectedFileIndex = EditorGUILayout.Popup(selectedFileIndex, logFileNames);

            if (GUILayout.Button("불러오기", GUILayout.Width(80)))
            {
                LoadSpecificReport(selectedFileIndex);
            }
        }
        else
        {
            GUILayout.Label("저장된 일지가 없습니다.");
        }
        EditorGUILayout.EndHorizontal(); // 가로 배치 끝

        GUILayout.Space(10);

        // ★ [안전장치] 만약 '어제' 기록을 불러와서 보는 중이라면, 
        // 다시 '오늘' 기록으로 돌아가기 쉽게 노란색 버튼을 띄워줍니다.
        if (currentEditingDate != DateTime.Now.ToString("yyyy-MM-dd"))
        {
            GUI.backgroundColor = Color.yellow;
            if (GUILayout.Button("새 일지 작성 (오늘 날짜로 비우기)", GUILayout.Height(30)))
            {
                CreateNewTodayLog();
            }
            GUI.backgroundColor = Color.white;
        }

        GUILayout.Space(10);
        if (GUILayout.Button("폴더에서 직접 열기", GUILayout.Height(30)))
        {
            OpenLogFolder();
        }
    }

    void SaveReport()
    {
        string report = BuildReportText(currentEditingDate);
        string path = Path.Combine(Application.dataPath, "../DevLogs");
        if (!Directory.Exists(path)) Directory.CreateDirectory(path);

        string filePath = Path.Combine(path, $"DevLog_{currentEditingDate}.txt");
        File.WriteAllText(filePath, report, Encoding.UTF8);

        GUIUtility.systemCopyBuffer = report;
        Debug.Log($"[{currentEditingDate} 일지 갱신 완료] 클립보드에 복사되었습니다!");
    }

    void LoadSpecificReport(int index)
    {
        if (index < 0 || index >= logFilePaths.Length) return;

        string filePath = logFilePaths[index];
        string dateTag = logFileNames[index];
        string fullText = File.ReadAllText(filePath, Encoding.UTF8);

        // 구분선을 기준으로 텍스트를 정확하게 잘라옵니다.
        classesWorkedOn = ExtractText(fullText, "■ 수정한 클래스", "■ 작업 내용");
        tasksDone = ExtractText(fullText, "■ 작업 내용", "■ 내일 할 일");
        nextTasks = ExtractText(fullText, "■ 내일 할 일", "----------------------");

        // 편집 기준일을 방금 불러온 파일의 날짜로 바꿉니다.
        currentEditingDate = dateTag;

        GUI.FocusControl(null); // 텍스트 커서 잔상을 지워줍니다.
        Debug.Log($"{dateTag} 일지를 성공적으로 불러왔습니다.");
    }

    // 다시 '오늘' 날짜로 리셋하는 함수
    void CreateNewTodayLog()
    {
        currentEditingDate = DateTime.Now.ToString("yyyy-MM-dd");
        classesWorkedOn = "";
        tasksDone = "";
        nextTasks = "";
        GUI.FocusControl(null);
    }

    string BuildReportText(string date)
    {
        return $"[개발 일지 - {date}]\n\n" +
               $"■ 수정한 클래스\n{classesWorkedOn}\n\n" +
               $"■ 작업 내용\n{tasksDone}\n\n" +
               $"■ 내일 할 일\n{nextTasks}\n\n" +
               $"----------------------";
    }

    string ExtractText(string source, string startTag, string endTag)
    {
        int start = source.IndexOf(startTag);
        if (start < 0) return "";
        start += startTag.Length;

        int end = source.IndexOf(endTag, start);
        if (end < 0) end = source.Length;

        return source.Substring(start, end - start).Trim();
    }

    void OpenLogFolder()
    {
        string path = Path.Combine(Application.dataPath, "../DevLogs");
        if (!Directory.Exists(path)) Directory.CreateDirectory(path);
        EditorUtility.RevealInFinder(path);
    }
}