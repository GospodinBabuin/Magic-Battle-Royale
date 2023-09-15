using System;
using System.Collections;
using System.Collections.Generic;
using Player;
using UnityEngine;

namespace Spells
{
    public class SpellHolder : MonoBehaviour
    {
        [SerializeField] private Spell spell;
        [SerializeField] private Transform spellTransform;
        [SerializeField] private List<Spell> spells;
        
        [SerializeField] private List<ParticleSystem> spellParticles = new List<ParticleSystem>();
        public byte CurrentSpellNumber { get; private set; } = 5;

        private int _animIDLightAttackSpell;
        private int _animIDHeavyAttackSpell;
        private int _animIDBuffSpell;
        
        public Transform SpellTransform { get => spellTransform; private set => spellTransform = value; }
        public GameObject Player { get; private set; }

        private void Awake()
        {
            _animIDLightAttackSpell = Animator.StringToHash("LightAttackSpell");
            _animIDHeavyAttackSpell = Animator.StringToHash("HeavyAttackSpell");
            _animIDBuffSpell = Animator.StringToHash("BuffSpell");
            
            Player = gameObject;

            for (int i = 0; i < spells.Count; i++)
            {
                spells[i].spellIsReady = true;
                spellParticles.Add(spells[i].particles);
                spellParticles[i] = Instantiate(spells[i].particles, spellTransform);
            }

            StartCoroutine(TurnOffLightsBug());
            
            //ChangeSpell(CurrentSpellNumber, false);
        }

        public void UseSpell()
        {
            if (spell == null) return;
            
            if (spell.spellIsReady)
            {
                spell.Activate(this);
                spell.spellIsReady = false;

                switch (spell.spellType)
                {
                    case Spell.SpellType.LightAttack:
                        Player.GetComponent<PlayerController>().Animator.SetTrigger(_animIDLightAttackSpell);
                        break;
                    
                    case Spell.SpellType.HeavyAttack:
                        Player.GetComponent<PlayerController>().Animator.SetTrigger(_animIDHeavyAttackSpell);
                        break;
                    
                    case Spell.SpellType.Buff:
                        Player.GetComponent<PlayerController>().Animator.SetTrigger(_animIDBuffSpell); 
                        break;
                }
                
                StartCoroutine(SpellActive(spell));
            }
        }

        public void ChangeSpell(byte spellNumber, bool needToActivate)
        {
            if (spells.Count < spellNumber || spells[spellNumber] == null) return;
            
            CurrentSpellNumber = spellNumber;
            
            spell = spells[spellNumber];
            CurrentSpellNumber = spellNumber;
            
            ActivateSpellVisuals(needToActivate);
        }

        private IEnumerator SpellActive(Spell spell)
        {
            yield return new WaitForSeconds(spell.activeTime);
            
            spell.Deactivate(this);
            
            StartCoroutine(SpellCooldown(spell));
        }
        
        private IEnumerator SpellCooldown(Spell spell)
        {
            yield return new WaitForSeconds(spell.cooldownTime);

            spell.spellIsReady = true;
        }
        
        public void ActivateSpellVisuals(bool needToActivate)
        {
            if (spellParticles.Count < CurrentSpellNumber || spellParticles[CurrentSpellNumber] == null) return;

            if (needToActivate)
            {
                foreach (ParticleSystem particles in spellParticles)
                    particles.Stop();

                spellParticles[CurrentSpellNumber].Play();
            }
            else
                spellParticles[CurrentSpellNumber].Stop();
        }

        private IEnumerator TurnOffLightsBug()
        {
            yield return new WaitForSeconds(0.5f);

            foreach (ParticleSystem particles in spellParticles)
                particles.Stop();
            
            ChangeSpell(0, false);
        }
        
    }
}