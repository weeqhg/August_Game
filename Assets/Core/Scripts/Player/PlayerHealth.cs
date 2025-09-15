using UnityEngine;
using UnityEngine.Events;

public class PlayerHealth : Health
{
    public UnityEvent<float, float> OnHealthChanged { get; } = new UnityEvent<float, float>();

    private MovePlayer movePlayer;
    private PlayerWeapon playerWeapon;
    private DashPlayer dashPlayer;
    private PlayerView playerView;
    private Rigidbody2D rb;
    private GameObject children;
    private SaveSystem saveSystem;

    protected override MonoBehaviour MovementComponent => movePlayer;
    protected override MonoBehaviour WeaponComponent => playerWeapon;
    protected override MonoBehaviour SpecialComponent => dashPlayer;
    protected override GameObject ChildrenObject => children;

    private float originalMoveSpeed;

    protected override void Start()
    {
        base.Start();

        saveSystem = GameManager.Instance.Get<SaveSystem>();
        if (saveSystem != null)
            LoadPlayerData();

        movePlayer = GetComponent<MovePlayer>();
        playerWeapon = GetComponentInChildren<PlayerWeapon>();
        dashPlayer = GetComponent<DashPlayer>();
        playerView = GetComponent<PlayerView>();
        rb = GetComponent<Rigidbody2D>();
        children = transform.GetChild(0).gameObject;
    }

    public override void TakeDamage(float damage, DamageType damageType, bool isCritical)
    {
        base.TakeDamage(damage, damageType, isCritical);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

    }

    protected override float SaveOriginalSpeed()
    {
        originalMoveSpeed = movePlayer.GetMoveSpeed();
        return originalMoveSpeed;
    }

    protected override void SetFrozenState(bool frozen)
    {
        if (frozen)
        {
            movePlayer.SetMoveSpeed(0f);
            playerWeapon.ChangeFreeze(true);
            dashPlayer.enabled = false;
        }
    }

    protected override void RestoreFromFreeze(float originalSpeed)
    {
        movePlayer.SetMoveSpeed(originalSpeed);
        playerWeapon.ChangeFreeze(false);
        dashPlayer.enabled = true;
    }

    protected override void DisableComponents()
    {
        saveSystem.DeleteSave();
        animator.enabled = false;
        movePlayer.enabled = false;
        dashPlayer.enabled = false;
        playerView.enabled = false;
        rb.isKinematic = true;
        rb.velocity = Vector3.zero;
        children.SetActive(false);
    }

    protected override void PlayDeathAnimation()
    {
        if (deathSound != null)
        {
            AudioSource.PlayClipAtPoint(deathSound, transform.position);
        }
    }

    // Переопределяем DamageFlash для проверки даша
    protected override void DamageFlash()
    {
        if (dashPlayer.IsDashing) return;
        base.DamageFlash();
    }


    public void LoadPlayerData()
    {

        GameData data = saveSystem.GetCurrentGameData();

        if (data != null)
        {
            currentHealth = data.currentHealth;
        }

    }

    public void SaveGameData()
    {
        var gameData = saveSystem.GetCurrentGameData();

        if (gameData != null)
        {
            gameData.currentHealth = this.currentHealth;
        }

        saveSystem.SaveGame();
    }

    private void OnApplicationQuit()
    {
        SaveGameData();
    }
}