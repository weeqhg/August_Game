using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Основной класс который регулирует очередность и загрузку сцены игры
/// через него нужно проводить все взаимодействия и связи.
/// </summary>
public class PriorityManager : MonoBehaviour
{
    [Header("Необходимые ссылки")]
    [SerializeField] private LoadingScreen _loadingScreen;
    [SerializeField] private Canvas _deathHud;
    [SerializeField] private Canvas _uiHud;

    private Spawn _spawn;
    private CaveGenerator _caveGenerator;
    private TilemapNavMeshGenerator _navMeshGenerator;
    private SaveSystem _saveSystem;

    public string[] textLoadRu;
    public string[] textLoadEn;
    private string[] textLoad;
    private void Awake()
    {
        GameManager.Instance.Register(this);
    }

    private void Start()
    {
        string lang = PlayerPrefs.GetString("Lang");
        SwitchLanguage(lang);
        _deathHud.enabled = false;
        StartCoroutine(LoadGameCoroutine(true, 1));
    }

    private void GetAllComponent()
    {
        _spawn = GameManager.Instance.Get<Spawn>();
        _caveGenerator = GameManager.Instance.Get<CaveGenerator>();
        _navMeshGenerator = GameManager.Instance.Get<TilemapNavMeshGenerator>();
        _saveSystem = GameManager.Instance.Get<SaveSystem>();
    }

    public void ResetLevel(int level)
    {
        StartCoroutine(LoadGameCoroutine(false, level));
    }
    public void RestartGame()
    {
        _deathHud.enabled = true;
    }

    public void SwitchLanguage(string lang)
    {
        switch (lang)
        {
            case "ru":
                textLoad = textLoadRu;
                break;
            case "en":
                textLoad = textLoadEn;
                break;
            default:
                textLoad = textLoadEn;
                break;
        }
    }

    // Для анимации загрузки игры
    private IEnumerator LoadGameCoroutine(bool isNewGame, int level)
    {
        // Начало анимации загрузки
        if (_loadingScreen != null)
        {
            _uiHud.enabled = false;
            _loadingScreen.ShowLoadingScreen();
        }

        yield return new WaitForSeconds(0.1f);

        // Получение компонентов (только для новой игры)
        if (isNewGame)
        {
            if (_loadingScreen != null)
                _loadingScreen.UpdateProgress(0.1f, textLoad[0]);
            GetAllComponent();
            yield return new WaitForSeconds(0.1f);
        }

        // Этап 1: Сохранение (только для новой игры)
        if (isNewGame)
        {
            if (_loadingScreen != null)
                _loadingScreen.UpdateProgress(0.1f, textLoad[1]);
            _saveSystem.CreateNewGame();
            yield return new WaitForSeconds(0.1f);
        }

        // Этап 2: Генерация пещеры
        if (_loadingScreen != null)
            _loadingScreen.UpdateProgress(isNewGame ? 0.3f : 0.2f, textLoad[2]);

        _caveGenerator.StartGenerate();
        yield return new WaitForSeconds(0.1f);

        // Этап 3: Навигационная сетка
        if (_loadingScreen != null)
            _loadingScreen.UpdateProgress(isNewGame ? 0.6f : 0.6f, textLoad[3]);

        _navMeshGenerator.BuildNavMesh();
        yield return new WaitForSeconds(0.1f);

        // Этап 4: Спавн объектов
        if (_loadingScreen != null)
            _loadingScreen.UpdateProgress(isNewGame ? 0.8f : 0.8f, textLoad[4]);

        _spawn.StartSpawn();
        yield return new WaitForSeconds(0.1f);
        // Ждем завершения спавна с прогрессом    

        // Завершение загрузки
        if (_loadingScreen != null)
        {
            _loadingScreen.UpdateProgress(1f, textLoad[5]);
            yield return new WaitForSeconds(0.2f);
            _loadingScreen.HideLoadingScreen();
            _uiHud.enabled = true;
        }

        yield return new WaitForSeconds(0.1f);
        _spawn.AnimationPlayerSpawn();
        // Показ анимации уровня
        if (_loadingScreen != null)
            _loadingScreen.ShowLevelAnimation(isNewGame ? 1 : level + 1);
    }
}