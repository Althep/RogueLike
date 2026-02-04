using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using static Defines;
public class SceneController: MonoBehaviour
{

    Dictionary<Scenes, string> scenes = new Dictionary<Scenes, string>()
    {
        { Scenes.MainScene,"MainScene" },
        { Scenes.DungeonScene,"DungeonScene" },
        { Scenes.EndingScene,"EndingScene"}
    };


    public void LoadCene(Action action, Scenes scene)
    {
        action?.Invoke();
        if (scenes.ContainsKey(scene))
        {
            SceneManager.LoadScene(scenes[scene]);
        }
        else
        {
            Debug.Log("매핑되지 않은 씬 타입");
        }
    }

    public IEnumerator LoadSceneRoutine(IEnumerator routine,Scenes scene)
    {
        yield return routine;
        if (scenes.ContainsKey(scene))
        {
            SceneManager.LoadScene(scenes[scene]);
        }
        else
        {
            Debug.Log("매핑되지 않은 씬 타입, 루틴");
        }
    }

    public async UniTask PrePostAwaits(Func<UniTask> asyncPreAction, Scenes sceneType, Func<UniTask> asyncPostAction)
    {
        if (asyncPreAction != null)
        {
            await asyncPreAction.Invoke();
        }
        Debug.Log("비동기 선행작업 완료, 씬로드 시작");

        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(scenes[sceneType]);

        await asyncOperation.ToUniTask(Progress.Create<float>(p =>
        {
            // 로딩 바 등에 진행률을 표시하는 코드를 여기에 넣을 수 있습니다.
            Debug.Log($"로딩 진행률: {p * 100:F0}%");
        }));
        //await asyncPreAction.Invoke();
        // ... 씬 로드 로직 ...

        // 씬 로드 완료 후 비동기 후처리 실행 및 대기
        if (asyncPostAction != null)
        {
            await asyncPostAction.Invoke();
        }

        Debug.Log($"씬로드 완료 및 후처리 완료 :{sceneType}");
    }
}

