using UnityEngine;

public class GameExit : MonoBehaviour
{
    // Метод для выхода из игры
    public void ExitGame()
    {
        #if UNITY_EDITOR
        // Если в редакторе - остановить воспроизведение
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        // В собранной версии - закрыть приложение
        Application.Quit();
        #endif
    }
}