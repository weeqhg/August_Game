using UnityEngine;

public class PlayerHealth : Health
{
    private MovePlayer movePlayer;
    private PlayerWeapon playerWeapon;
    private DashPlayer dashPlayer;
    private PlayerView playerView;
    private Rigidbody2D rb;
    private GameObject children;

    protected override MonoBehaviour MovementComponent => movePlayer;
    protected override MonoBehaviour WeaponComponent => playerWeapon;
    protected override MonoBehaviour SpecialComponent => dashPlayer;
    protected override GameObject ChildrenObject => children;

    private float originalMoveSpeed;

    protected override void Start()
    {
        movePlayer = GetComponent<MovePlayer>();
        playerWeapon = GetComponentInChildren<PlayerWeapon>();
        dashPlayer = GetComponent<DashPlayer>();
        playerView = GetComponent<PlayerView>();
        rb = GetComponent<Rigidbody2D>();
        children = transform.GetChild(0).gameObject;

        base.Start();
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
}