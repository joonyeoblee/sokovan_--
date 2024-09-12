using UnityEngine;
using UnityEngine.SceneManagement;
public class GameManager : MonoBehaviour
{
    //TODO: Player info and game state, Change UI, etc.
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
