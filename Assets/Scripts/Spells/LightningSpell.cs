using System.Collections;
using Player;
using UnityEngine;

namespace Spells
{
    [CreateAssetMenu(fileName = "LightningSpell", menuName = "Spells/LightningSpell", order = 1)]
    public class LightningSpell : Spell
    {
        [SerializeField] private TrailRenderer trailRenderer;
        [SerializeField] private LayerMask hitLayerMask;
        
        public override void Activate(SpellHolder spellHolder)
        {
            if (Physics.Raycast(spellHolder.SpellTransform.position,
                    spellHolder.Player.GetComponent<PlayerController>().DebugTransform.position - spellHolder.SpellTransform.position, out RaycastHit hit, float.MaxValue , hitLayerMask))
            {
                TrailRenderer trail = Instantiate(trailRenderer, spellHolder.SpellTransform.position, Quaternion.identity);

                spellHolder.StartCoroutine(SpawnTrail(trail, hit));
            }
        }

        public override void Deactivate(SpellHolder spellHolder)
        {
            
        }
        
        private IEnumerator SpawnTrail(TrailRenderer trail, RaycastHit hitInfo)
        {
            float time = 0;
            Vector3 startPosition = trail.transform.position;

            while (time < 1)
            {
                trail.transform.position = Vector3.Lerp(startPosition, hitInfo.point, time);
                time += Time.deltaTime / trail.time;

                yield return null;
            }

            trail.transform.position = hitInfo.point;
            Instantiate(impactParticles, hitInfo.point, Quaternion.LookRotation(hitInfo.normal));
            
            Destroy(trail.gameObject, trail.time);
        }
    }
}