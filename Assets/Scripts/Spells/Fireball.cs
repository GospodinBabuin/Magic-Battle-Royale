using UnityEngine;

namespace Spells
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(SphereCollider))]
    public class Fireball : MonoBehaviour
    {
        private Rigidbody _rigidbody;
        private Vector3 _targetPosition;
        private GameObject _owner;

        [SerializeField] private float fireballSpeed = 1000f;
        [SerializeField] private ParticleSystem impactParticles;

        public void Initialize(Vector3 targetPosition, GameObject owner)
        {
            _owner = owner;
            _rigidbody = GetComponent<Rigidbody>();
            
            Vector3 direction = targetPosition - transform.position;
            _rigidbody.AddForce(direction.normalized * fireballSpeed);
            Debug.Log("Initialize");
        }

        private void OnCollisionEnter(Collision other)
        {
            if (other.gameObject == _owner) return;
            
            Instantiate(impactParticles, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }
}
