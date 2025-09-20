using UnityEngine;
using UnityEngine.Playables;

public class LoopTimeline : MonoBehaviour
{
    [SerializeField] private PlayableDirector playableDirector;
    [SerializeField] private bool loop = true;
    
    private void Start()
    {
        if (playableDirector == null)
            playableDirector = GetComponent<PlayableDirector>();
            
        if (playableDirector != null)
        {
            // Подписываемся на событие завершения таймлайна
            playableDirector.stopped += OnTimelineStopped;
            
            // Запускаем таймлайн
            playableDirector.Play();
        }
    }
    
    private void OnTimelineStopped(PlayableDirector director)
    {
        if (loop && director == playableDirector)
        {
            // Перематываем в начало и запускаем снова
            director.time = 0;
            director.Play();
        }
    }
    
    // Метод для управления циклом
    public void SetLoop(bool shouldLoop)
    {
        loop = shouldLoop;
    }
    
    private void OnDestroy()
    {
        // Отписываемся от события при уничтожении объекта
        if (playableDirector != null)
            playableDirector.stopped -= OnTimelineStopped;
    }
}