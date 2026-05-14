using UnityEngine;
using UnityEditor; // 유니티 에디터 창을 만들기 위한 필수 네임스페이스
using System;
using System.IO;   // 텍스트 파일 저장 및 폴더 탐색기 기능을 위한 네임스페이스

// 일반적인 게임 오브젝트 스크립트(MonoBehaviour)가 아니라, 
// 유니티 에디터의 독립적인 창을 띄우기 위해 EditorWindow를 상속받습니다.
public class DevLogMini : EditorWindow
{
    // 입력받은 텍스트를 임시로 들고 있을 변수들입니다. (뇌의 작업 기억을 대신할 공간)
    string tasksDone = "";
    string classesWorkedOn = "";

    // 1. 창 띄우기 설정
    // 상단 메뉴바에 [Tools] -> [개발 일지 작성기] 버튼을 만들어 줍니다.
    [MenuItem("Tools/개발 일지 작성기")]
    public static void ShowWindow()
    {
        // 클릭하면 "개발 일지"라는 이름의 창이 열립니다.
        GetWindow<DevLogMini>("개발 일지");
    }

    // 2. UI 그리기 (유니티 에디터 창에 보여질 버튼과 입력칸들)
    void OnGUI()
    {
        GUILayout.Label("오늘의 작업 기록", EditorStyles.boldLabel);
        GUILayout.Space(10); // 여백 10픽셀

        // [입력칸 1] 작업한 클래스 이름 적는 곳 (한 줄 입력)
        GUILayout.Label("작업한 클래스 (예: PlayerModifier, AStarPathfinding):");
        classesWorkedOn = EditorGUILayout.TextField(classesWorkedOn);

        GUILayout.Space(10);

        // [입력칸 2] 오늘 한 일 적는 곳 (엔터키 쳐서 여러 줄 입력 가능하도록 크기 100 할당)
        GUILayout.Label("오늘 한 일 (기억나는 대로 짧게, 단어 위주로):");
        tasksDone = EditorGUILayout.TextArea(tasksDone, GUILayout.Height(100));

        GUILayout.Space(20);

        // [버튼 1] 텍스트 파일로 저장하고 클립보드에 복사하는 버튼 (크게 보이도록 높이 40)
        if (GUILayout.Button("보고서 저장 및 클립보드 복사", GUILayout.Height(40)))
        {
            GenerateAndSaveReport();
        }

        GUILayout.Space(10);

        // [버튼 2] 어제 기록을 슬쩍 보고 싶을 때 누르는 폴더 열기 버튼 (높이 30)
        if (GUILayout.Button("저장된 일지 폴더 열기", GUILayout.Height(30)))
        {
            OpenLogFolder();
        }
    }

    // 3. 실제 저장 및 복사 기능이 돌아가는 로직
    void GenerateAndSaveReport()
    {
        // 오늘 날짜를 YYYY-MM-DD 형태로 가져옵니다.
        string date = DateTime.Now.ToString("yyyy-MM-dd");

        // 텍스트 파일에 적힐 보고서의 형태를 예쁘게 조립합니다. (\n은 줄바꿈)
        string report = $"[개발 일지 - {date}]\n\n" +
                        $"■ 수정한 클래스\n{classesWorkedOn}\n\n" +
                        $"■ 작업 내용\n{tasksDone}\n\n" +
                        $"----------------------";

        // 저장될 경로 설정: Application.dataPath는 Assets 폴더를 의미합니다.
        // 유니티가 리로딩되며 작업 흐름이 끊기는 걸 막기 위해, Assets 폴더 바깥(../)에 DevLogs 폴더를 만듭니다.
        string path = Path.Combine(Application.dataPath, "../DevLogs");

        // 만약 DevLogs 폴더가 아직 없다면 새로 생성해 줍니다.
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        // 최종적으로 저장될 파일의 이름 (예: DevLog_2026-05-14.txt)
        string filePath = Path.Combine(path, $"DevLog_{date}.txt");

        // File.AppendAllText를 사용하면, 기존 파일 내용을 지우지 않고 맨 밑에 계속 이어붙입니다.
        // 생각날 때마다 버튼을 눌러도 안전하게 누적됩니다.
        File.AppendAllText(filePath, report + "\n");

        // 유니티 밖의 메신저나 메모장에 바로 Ctrl+V 할 수 있도록 시스템 클립보드에 복사합니다.
        GUIUtility.systemCopyBuffer = report;

        // 에러가 났는지 저장이 잘 되었는지 알기 쉽게 유니티 콘솔창에 메시지를 띄워줍니다.
        Debug.Log($"[개발일지 저장 완료] 클립보드에 복사되었습니다! (저장 위치: {filePath})");

        // 저장을 완료했으니, 창에 쓰여있던 글씨들을 깔끔하게 지워줍니다. (다음 입력을 위해)
        tasksDone = "";
        classesWorkedOn = "";
    }

    // 4. 저장된 폴더를 윈도우 탐색기(맥은 파인더)로 열어주는 기능
    void OpenLogFolder()
    {
        // 위에서 설정한 경로와 동일한 DevLogs 폴더 경로를 잡습니다.
        string path = Path.Combine(Application.dataPath, "../DevLogs");

        // 버튼을 먼저 눌러버려서 폴더가 없을 경우를 대비해, 없으면 빈 폴더를 만들어줍니다.
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        // 지정된 경로의 폴더를 운영체제의 기본 파일 탐색기로 바로 열어줍니다.
        EditorUtility.RevealInFinder(path);
    }
}