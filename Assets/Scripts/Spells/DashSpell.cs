using Player;
using UnityEngine;

namespace Spells
{
    [CreateAssetMenu(fileName = "DashSpell", menuName = "Spells/DashSpell", order = 0)]
    public class DashSpell : Spell
    {
        [SerializeField] private float dashSpeed;

        public override void Activate(SpellHolder spellHolder)
        {
            Instantiate(impactParticles, spellHolder.SpellTransform);
            
            PlayerController playerController = spellHolder.Player.GetComponent<PlayerController>();
            
            playerController.SetSpeedMultiplier(dashSpeed);
        }
        
        public override void Deactivate(SpellHolder spellHolder)
        {
            PlayerController playerController = spellHolder.Player.GetComponent<PlayerController>();
            
            playerController.SetSpeedMultiplier(1);
        }
    }
}
