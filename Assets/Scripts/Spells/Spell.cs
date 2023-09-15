using UnityEngine;

namespace Spells
{
    public abstract class Spell : ScriptableObject
    {
        public float cooldownTime;
        public float activeTime;
        public ParticleSystem particles;
        public ParticleSystem impactParticles;
        public bool spellIsReady = true;

        [HideInInspector]
        public enum SpellType
        {
            LightAttack,
            HeavyAttack,
            Buff
        }
        public SpellType spellType;

        public abstract void Activate(SpellHolder spellHolder);
        public abstract void Deactivate(SpellHolder spellHolder);
    }
}