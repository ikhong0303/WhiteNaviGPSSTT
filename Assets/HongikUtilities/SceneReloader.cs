using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneReloader : MonoBehaviour
{
    // �� �޼��带 ȣ���ϸ� ���� ���� �ٽ� �ε�˴ϴ�.
    public void RestartScene()
    {
        // ���� Ȱ��ȭ�� ���� �̸��� ����ϴ�.
        string currentSceneName = SceneManager.GetActiveScene().name;

        // ���� ���� �ٽ� �ε��մϴ�.
        SceneManager.LoadScene(currentSceneName);
    }
}
