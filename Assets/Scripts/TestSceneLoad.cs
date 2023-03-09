using UnityEngine;
using UnityEngine.SceneManagement;

public class TestSceneLoad : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        SceneManager.LoadScene("TestingScene", LoadSceneMode.Additive);        
    }

    void OnEnable()
    {
        Scene scene = SceneManager.GetSceneByBuildIndex(1);
        SceneManager.SetActiveScene(scene);
        scene = SceneManager.GetActiveScene();
        Debug.Log("Active Scene is '" + scene.name + "'.");
        GameObject obj = GameObject.Find("ShaderTestObject");
        Debug.Log("Object name is '" + obj.name + "'.");
    }

}
