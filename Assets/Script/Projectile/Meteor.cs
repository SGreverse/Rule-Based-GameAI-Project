using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Script.Projectile
{
    public class Meteor : MonoBehaviour
    {
        [Header("Settings")]
        private float _damage;
        private float _explosionRadius;
        private float _telegraphDuration;

        [Header("Visual References")]
        [Tooltip("The red circle that warns the player")]
        public SpriteRenderer warningDecal;
        [Tooltip("The actual explosion particle effect/sprite")]
        public GameObject explosionEffect;

        private bool _hasExploded = false;

        // The BossManager calls this the moment it instantiates the meteor
        public void Initialize(float damage, float radius, float telegraphTime)
        {
            this._damage = damage;
            this._explosionRadius = radius;
            this._telegraphDuration = telegraphTime;

            // 1. Scale the warning circle to perfectly match the damage radius!
            if (warningDecal != null)
            {
                warningDecal.transform.localScale = new Vector3(radius * 2, radius * 2, 1f);
                warningDecal.color = new Color(1f, 0f, 0f, 0.2f); // Start as faint red
            }

            // 2. Hide the explosion effect until it actually hits
            if (explosionEffect != null) explosionEffect.SetActive(false);

            // 3. Start the countdown
            StartCoroutine(MeteorRoutine());
        }

        private IEnumerator MeteorRoutine()
        {
            float timer = 0f;

            // Phase 1: The Telegraph (Warning)
            while (timer < _telegraphDuration)
            {
                timer += Time.deltaTime;

                // Smoothly increase the opacity of the red circle so the player knows time is running out!
                if (warningDecal != null)
                {
                    float alpha = Mathf.Lerp(0.2f, 0.8f, timer / _telegraphDuration);
                    warningDecal.color = new Color(1f, 0f, 0f, alpha);
                }
                yield return null;
            }

            // Phase 2: The Strike!
            Explode();
        }

        private void Explode()
        {
            if (_hasExploded) return;
            _hasExploded = true;

            // Swap visuals
            if (warningDecal != null) warningDecal.gameObject.SetActive(false);
            if (explosionEffect != null) explosionEffect.SetActive(true);

            // Calculate Damage using Unity's built-in physics overlap
            // This creates a perfect invisible circle and grabs everything inside it
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, _explosionRadius);

            foreach (Collider2D hit in hits)
            {
                if (hit.CompareTag("Player"))
                {
                    PlayerManager player = hit.GetComponent<PlayerManager>();
                    if (player != null)
                    {
                        player.GetBrain().TakeDamage(_damage);
                        Debug.Log($"Player failed to dodge! Took {_damage} Meteor Damage.");
                    }
                }
            }

            // Destroy the meteor object after 1 second (giving the explosion effect time to play)
            Destroy(gameObject, 1.0f);
        }

        // This helps you see the actual damage radius in the Unity Editor!
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _explosionRadius);
        }
    }
}
