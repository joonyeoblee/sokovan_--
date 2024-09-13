using UnityEngine;
using UnityEngine.SceneManagement;
public class GameManager : MonoBehaviour
{
    //TODO: Player info and game state, Change UI, etc.

    // make a singleton
    public static GameManager instance;
    public GameObject player;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void LoadScene()
    {
        SceneManager.LoadScene("GameScene");
    }

    public void LoadBossScene()
    {
        SceneManager.LoadScene("Boss");
    }

    public void GameQuit()
    {
        Application.Quit();
    }
}
