using System.Collections.Generic;
using Events;
using UniRx;
using UnityEngine;
using UnityEngine.Events;

namespace ActionComponents
{
    public interface IDamageable
    {
        public void Damage(float amount, Vector3? position = null);
        GameCharacterType GetCharacterType();
        void Heal(float amount);
        void HealArmor(float amount);
    }

    public class Damageable : MonoBehaviour, IDamageable
    {
        public GameCharacterType type;
        [SerializeField] private float armor, maxArmor, armorRegenPerSecond;
        [SerializeField] private float life, maxLife, lifeRegenPerSecond;
        [SerializeField] private UnityEvent onDie;
        [SerializeField] private List<GameObject> spawnOnDamages;

        public float Life => life;
        
        private void Start()
        {
            MessageBroker.Default.Publish(new CharacterHealthChanged(type, maxLife, life, life));
            MessageBroker.Default.Publish(new DamageableArmorChanged(type, maxArmor, armor, armor));
        }

        private void Update()
        {
            if (lifeRegenPerSecond > 0 && life < maxLife)
            {
                Heal(lifeRegenPerSecond * Time.deltaTime);
            }
            
            if (armorRegenPerSecond > 0 && armor < maxArmor)
            {
                HealArmor(armorRegenPerSecond * Time.deltaTime);
            }
        }

        public void Damage(float amount, Vector3? position = null)
        {
            if (life < 0)
            {
                return;
            }
            
            if (armor > 0)
            {
                var oldArmor = armor;
                armor -= amount;
                armor = Mathf.Max(0f, armor);
                MessageBroker.Default.Publish(new DamageableArmorChanged(type, maxArmor, armor, oldArmor));
                return;
            }


            var oldLife = life;
            life -= amount;
            MessageBroker.Default.Publish(new CharacterHealthChanged(type, maxLife, life, oldLife));

            if (spawnOnDamages != null && position.HasValue)
            {
                foreach (var spawnOnDamage in spawnOnDamages)
                {
                    Instantiate(spawnOnDamage, position.Value, Quaternion.identity);
                }
            }

            if (life < 0)
            {
                OnDie();
            }
        }

        public void Heal(float amount)
        {
            // no zombies allowed
            if (life <= 0)
            {
                return;
            }

            var oldLife = life;
            life += amount;
            MessageBroker.Default.Publish(new CharacterHealthChanged(type, maxLife, life, oldLife));
        }
        
        public void HealArmor(float amount)
        {
            // no zombies allowed
            if (life <= 0)
            {
                return;
            }

            var oldAmor = armor;
            armor += amount;
            MessageBroker.Default.Publish(new DamageableArmorChanged(type, maxLife, life, oldAmor));
        }

        public void AddMaxHealth(float amount)
        {
            maxLife = amount;
            MessageBroker.Default.Publish(new CharacterHealthChanged(type, maxLife, life, life));
        }

        public void AddHealthRegen(float amount)
        {
            lifeRegenPerSecond += amount;
            MessageBroker.Default.Publish(new CharacterHealthChanged(type, maxLife, life, life));
        }


        private void OnDie()
        {
            onDie?.Invoke();
        }

        public GameCharacterType GetCharacterType()
        {
            return type;
        }
    }
}