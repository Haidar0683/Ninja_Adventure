using UnityEngine;

public class QuitOnClick : MonoBehaviour
{
    /// <summary>
    /// Panggil metode ini di OnClick() event pada Button
    /// </summary>
    public void QuitGame()
    {
        // Jika dijalankan dalam build (Windows/Mac/Linux/iOS/Android/etc.)
        Application.Quit();

        // Jika dijalankan di Editor, hentikan play mode
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif

        Debug.Log("QuitGame() dipanggil: aplikasi akan ditutup.");
    }
}
