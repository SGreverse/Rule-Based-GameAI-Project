using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Script.Projectile
{
    public class Shockwave : MonoBehaviour
    {
        private float _damage;
        private float _stunDuration;
        private float _maxRadius;
        private float _expandTime;

        private bool _hasHitPlayer = false; // Prevents hitting the player 60 times a second!

        private EdgeCollider2D _edgeCollider;
        private int _circleResolution = 36;//how smooth the collision ring is

        [Header("Visual Calibration")]
        [Tooltip("Tweak this until the green EdgeCollider line perfectly overlaps your rocks at scale 1,1,1")]
        public float baseColliderRadius = 1.0f; 
        private void Awake()
        {
            _edgeCollider = GetComponent<EdgeCollider2D>();
            _edgeCollider.isTrigger = true;

            // 1. Generate the perfect hollow ring at a base radius of 1.
            // As the transform.localScale grows, this ring will naturally stretch with it!
            CreatePerfectRing();
        }

        private void CreatePerfectRing()
        {
            Vector2[] points = new Vector2[_circleResolution + 1];
            for (int i = 0; i <= _circleResolution; i++)
            {
                float angle = (i / (float)_circleResolution) * Mathf.PI * 2f;

                // THE FIX: Multiply the X and Y by your baseColliderRadius!
                float x = Mathf.Cos(angle) * baseColliderRadius;
                float y = Mathf.Sin(angle) * baseColliderRadius;

                points[i] = new Vector2(x, y);
            }
            _edgeCollider.points = points;
        }
        // The Boss calls this immediately after spawning the shockwave
        public void Initialize(float damage, float stunDuration, float maxRadius, float expandTime)
        {
            this._damage = damage;
            this._stunDuration = stunDuration;
            this._maxRadius = maxRadius;
            this._expandTime = expandTime;

            // Start as a tiny point
            transform.localScale = new Vector3(0.01f, 0.01f, 1f);

            // Start the expansion process
            StartCoroutine(ExpandRoutine());
        }

        private IEnumerator ExpandRoutine()
        {
            float timer = 0f;
            Vector3 targetScale = new Vector3(_maxRadius, _maxRadius, 1f);

            while (timer < _expandTime)
            {
                timer += Time.deltaTime;
                // Smoothly grow the circle
                transform.localScale = Vector3.Lerp(Vector3.zero, targetScale, timer / _expandTime);
                yield return null;
            }

            // Once fully expanded, wait a tiny fraction of a second, then destroy itself
            yield return new WaitForSeconds(0.1f);
            Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (!_hasHitPlayer && collision.CompareTag("Player"))
            {
                PlayerManager player = collision.GetComponent<PlayerManager>();
                if (player != null)
                {
                    _hasHitPlayer = true;

                    // Deal Damage 
                    player.GetBrain().TakeDamage(_damage);

                    // Apply Stun
                    int randomChance = UnityEngine.Random.Range(0, 3);
                    if (randomChance == 2) { 
                    player.GetStunned(_stunDuration);
                    }

                    Debug.Log($"Player hit by Shockwave! Took {_damage} damage and stunned for {_stunDuration}s.");
                }
            }
        }
    }
}
