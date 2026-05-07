using UnityEngine;
using System.Collections;

public class MeleeHitboxVisualizer : MonoBehaviour
{
    private SpriteRenderer _spriteRenderer;
    public float duration = 0.2f;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void PlayVisual(float attackLength, float attackWidth, Vector2 direction)
    {
        transform.localScale = new Vector3(attackWidth, attackLength, 1);

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;

        transform.rotation = Quaternion.Euler(0, 0, angle);

        StartCoroutine(FadeOutRoutine());
    }

    private IEnumerator FadeOutRoutine()
    {
        float time = 0;
        Color startColor = _spriteRenderer.color;

        while (time < duration)
        {
            time += Time.deltaTime;
            float alpha = Mathf.Lerp(startColor.a, 0, time / duration);
            _spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }

        Destroy(gameObject);
    }
}