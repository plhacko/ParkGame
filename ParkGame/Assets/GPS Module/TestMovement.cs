using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TestMovement : MonoBehaviour
{
    public string sceneName;
    public GameObject map;

    IEnumerator LoadSceneWithGameObject(GameObject gameObject)
    {
        Scene currentScene = SceneManager.GetActiveScene();

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        SceneManager.MoveGameObjectToScene(gameObject, SceneManager.GetSceneByName(sceneName));
        SceneManager.UnloadSceneAsync(currentScene);
    }

    public void LoadScene()
    {
        StartCoroutine(LoadSceneWithGameObject(map));
    }
}
