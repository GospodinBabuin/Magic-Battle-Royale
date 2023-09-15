using Player;
using UnityEngine;

namespace Spells
{
    [CreateAssetMenu(fileName = "FireballSpell", menuName = "Spells/FireballSpell", order = 2)]
    public class FireballSpell : Spell
    {
        [SerializeField] private LayerMask hitLayerMask;
        [SerializeField] private Fireball fireball;
        public override void Activate(SpellHolder spellHolder)
        {
            if (Physics.Raycast(spellHolder.SpellTransform.position,
                    spellHolder.Player.GetComponent<PlayerController>().DebugTransform.position -
                    spellHolder.SpellTransform.position, out RaycastHit hit, float.MaxValue, hitLayerMask))
            {
                Instantiate(fireball, spellHolder.SpellTransform.position, Quaternion.identity).Initialize(hit.point, spellHolder.Player);
            }
        }

        public override void Deactivate(SpellHolder spellHolder)
        {
            
        }
    }
}
