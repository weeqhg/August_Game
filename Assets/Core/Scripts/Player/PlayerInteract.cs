using UnityEngine;
using System.Collections;

public abstract class PlayerInteract : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] protected Material originalMaterial;
    [SerializeField] protected Material outLineMaterial;
    [SerializeField] protected float interactionCooldown = 0.2f;
    [SerializeField] protected SpriteRenderer spriteRendererPressE;

    protected SpriteRenderer spriteRenderer;
    protected bool canInteract = false;
    protected InterfacePrompt prompt;

    protected virtual void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRendererPressE.enabled = false;

        // Автоматически находим материалы если они не назначены
        if (originalMaterial == null)
            originalMaterial = spriteRenderer?.material;
    }

    protected virtual void Update()
    {
        if (canInteract && Input.GetKeyDown(KeyCode.E))
        {
            StartCoroutine(InteractWithCooldown());
        }
    }
    
    private IEnumerator InteractWithCooldown()
    {
        Interact();   
        yield return new WaitForSeconds(interactionCooldown);
    }

    public abstract void Interact();

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            EnableInteraction();
        }
    }

    protected virtual void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            DisableInteraction();
        }
    }

    protected virtual void EnableInteraction()
    {
        canInteract = true;
        if (spriteRenderer != null && outLineMaterial != null)
        {
            spriteRenderer.material = outLineMaterial;
        }
        spriteRendererPressE.enabled = canInteract;
    }

    protected virtual void DisableInteraction()
    {
        canInteract = false;
        if (spriteRenderer != null && originalMaterial != null)
        {
            spriteRenderer.material = originalMaterial;
        }
        spriteRendererPressE.enabled = canInteract;
    }

    protected virtual void OnDestroy()
    {
    }
}