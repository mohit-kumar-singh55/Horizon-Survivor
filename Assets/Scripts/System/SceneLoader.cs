using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public static void LoadScene(int buildIndex)
    {
        SceneManager.LoadScene(buildIndex);
    }
}