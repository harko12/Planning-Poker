using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class Scenery : MonoBehaviour {
    public string[] Scenes;
    public Material[] Skyboxes;
    private static int mSceneKey = -1;
    public int SceneKey
    {
        get
        {
            if (Scenery.mSceneKey == -1)
            {
                Scenery.mSceneKey = Random.Range(0, Scenes.Length);
            }
            return mSceneKey;
        }
    }

    void Start()
    {
        StartCoroutine("InitScenery");
    }

    // Use this for initialization
    IEnumerator InitScenery () {

        if (Scenes.Length > 0)
        {
            bool go = false;
            var sceneName = Scenes[SceneKey];
            AsyncOperation async = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            while (!go)
            {
                yield return async;
                go = true;
            }
            RenderSettings.skybox = Skyboxes[SceneKey];
            go = false;
            async = SceneManager.LoadSceneAsync("Camera Moves", LoadSceneMode.Additive);
        }
    }

    void OnLevelWasLoaded(int level)
    {
        RenderSettings.skybox = Skyboxes[SceneKey];
    }

    public void ReloadScenery(int SceneIndex = -1)
    {
// clean up old stuff
        var currentSceneKey = mSceneKey;
        SceneManager.UnloadScene(Scenes[currentSceneKey]);
        SceneManager.UnloadScene("Camera Moves");
        var sceneryObjects = GameObject.FindGameObjectsWithTag("Scenery");

        // clean up old stuff then reload
        foreach (var go in sceneryObjects)
        {
            DestroyImmediate(go);
        }
// load new scenery
        mSceneKey = SceneIndex; // either choose what was passed in or -1 to re-randomize
        StartCoroutine("InitScenery");
    }

    void OnGUI()
    {
        /*
        GUILayout.BeginArea(new Rect(0, 0, 50f, 50f));
        if (GUI.Button(new Rect(0,0,45f,45f), "Reset" ))
        {
            ReloadScenery();
        }
        GUILayout.EndArea();
        */
    }
}
