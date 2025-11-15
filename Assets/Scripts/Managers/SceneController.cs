using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using static Defines;
public class SceneController
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
}
