using UnityEngine;
using System.Collections;

public class Resource : MonoBehaviour
{

    [SerializeField] private int amount;
    private int maxAmount;
    [SerializeField] private bool isDepleted = false;

    [SerializeField] private bool threeDepletedSprites = false;

    [SerializeField] private GameObject defaultSprite;
    [SerializeField] private GameObject depleatedSprite;
    [SerializeField] private GameObject depleatedStage2;
    [SerializeField] private GameObject depleatedStage3;

    [Header("Collect feedback (subtle wobble)")]
    [SerializeField] private float wobbleAngleDegrees = 3f;
    [SerializeField] private float wobbleDuration = 0.2f;
    private Coroutine wobbleCoroutine;
    private Quaternion defaultSpriteBaseLocalRotation;

    [SerializeField] private resourceType _resourceType;
    public enum resourceType
    {
        wood,
        stone,
        iron,
        gold,
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        maxAmount = amount;
        defaultSprite.SetActive(true);
        depleatedSprite.SetActive(false);

        if (threeDepletedSprites){
            depleatedStage2.SetActive(false);
            depleatedStage3.SetActive(false);
        }
    }
    public resourceType GetResourceType()
    {
        return _resourceType;
    }

    public void CollectResource()
    {
        amount--;
        if(threeDepletedSprites)
        {
            if (amount <= maxAmount  * 0.66f && amount > maxAmount  * 0.33f)
            {
                depleatedStage2.SetActive(true);
                depleatedSprite.SetActive(false);
                defaultSprite.SetActive(false);
            }
            if (amount <= maxAmount  * 0.33f)
            {
                depleatedStage3.SetActive(true);
                depleatedStage2.SetActive(false);
                depleatedSprite.SetActive(false);
                defaultSprite.SetActive(false);
            }
        }
        if (amount <= 0)
        {
            depleatedSprite.SetActive(true);
            defaultSprite.SetActive(false);
            if(threeDepletedSprites){
            depleatedStage2.SetActive(false);
            depleatedStage3.SetActive(false);
            }

            isDepleted = true;
        }

        // Subtle feedback: wobble only while the default sprite is still active/visible.
        if (!isDepleted && defaultSprite != null && defaultSprite.activeSelf)
        {
            PlayDefaultSpriteWobble();
        }
    }
    

    private void PlayDefaultSpriteWobble()
    {
        if (defaultSprite == null)
        {
            return;
        }

        Transform t = defaultSprite.transform;
        defaultSpriteBaseLocalRotation = t.localRotation;

        if (wobbleCoroutine != null)
        {
            StopCoroutine(wobbleCoroutine);
            wobbleCoroutine = null;
            t.localRotation = defaultSpriteBaseLocalRotation;
        }

        wobbleCoroutine = StartCoroutine(DefaultSpriteWobbleCoroutine());
    }

    private IEnumerator DefaultSpriteWobbleCoroutine()
    {
        if (defaultSprite == null)
        {
            wobbleCoroutine = null;
            yield break;
        }

        Transform t = defaultSprite.transform;
        float duration = Mathf.Max(0.01f, wobbleDuration);
        float elapsed = 0f;
        const float oscillations = 2f;

        while (elapsed < duration)
        {
            float n = elapsed / duration;         // 0..1
            float damper = 1f - n;                // ease back to rest
            float angle = Mathf.Sin(n * Mathf.PI * 2f * oscillations) * wobbleAngleDegrees * damper;
            t.localRotation = defaultSpriteBaseLocalRotation * Quaternion.Euler(0f, 0f, angle);

            elapsed += Time.deltaTime;
            yield return null;
        }

        t.localRotation = defaultSpriteBaseLocalRotation;
        wobbleCoroutine = null;
    }
    //Wobble Animation------------------------------------

    public bool IsDepleted()
    {
        return isDepleted;
    }
}
