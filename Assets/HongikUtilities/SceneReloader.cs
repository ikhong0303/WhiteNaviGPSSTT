using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneReloader : MonoBehaviour
{
    // 이 메서드를 호출하면 현재 씬이 다시 로드됩니다.
    public void RestartScene()
    {
        // 현재 활성화된 씬의 이름을 얻습니다.
        string currentSceneName = SceneManager.GetActiveScene().name;

        // 현재 씬을 다시 로드합니다.
        SceneManager.LoadScene(currentSceneName);
    }
}
