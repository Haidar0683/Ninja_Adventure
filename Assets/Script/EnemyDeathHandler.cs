using UnityEngine;
using System.Collections;

public class EnemyDeathHandler : MonoBehaviour
{
    public static EnemyDeathHandler Instance;

    [SerializeField] private GameObject defeatPanel;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        if (defeatPanel != null)
            defeatPanel.SetActive(false);
    }

    public void HandleEnemyDestroyedAfterDelay(float delay)
    {
        StartCoroutine(WaitAndShowPanel(delay));
    }

    private IEnumerator WaitAndShowPanel(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (defeatPanel != null)
            defeatPanel.SetActive(true);
    }
}
