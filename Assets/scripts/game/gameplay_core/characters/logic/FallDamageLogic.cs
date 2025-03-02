using dream_lib.src.reactive;
using game.gameplay_core.characters.runtime_data;
using game.gameplay_core.characters.runtime_data.bindings;
using game.gameplay_core.damage_system;
using UnityEngine;

namespace game.gameplay_core.characters.logic
{
public class FallDamageLogic
    {
        public struct Context
        {
            public ApplyDamageCommand ApplyDamage { get; set; }
            public IsDead IsDead { get; set; }
            public Transform CharacterTransform { get; set; }
            public CharacterStats CharacterStats { get; set; }
            public ReactiveProperty<bool> IsFalling { get; set; }
            public InvulnerabilityLogic InvulnerabilityLogic { get; set; }
            public ReactiveCommand TriggerStagger { get; set; }
            

            public float MinimumFallDamageHeight { get; set; }
            public float LethalFallHeight { get; set; }
            public float StaggerThreshold { get; set; }
        }

        private Context _context;
        private float _fallStartY;
        

        public ReactiveProperty<bool> FallDamageProtectionActive { get; } = new ReactiveProperty<bool>(false);
        

        private float _lastProtectionActivationTime;
        private const float PROTECTION_COOLDOWN = 0.5f; 
        private const float PROTECTION_DURATION = 1.5f; 
        
        [SerializeField]
        private float _minFallDamagePercent = 0.1f; 
        
        [SerializeField]
        private float _maxFallDamagePercent = 1.0f; 

        public void SetContext(Context context)
        {
            _context = context;
            

            _context.IsFalling.OnChangedFromTo += HandleFallingChanged;
            _lastProtectionActivationTime = -PROTECTION_COOLDOWN; 
        }

        public void CustomUpdate(float deltaTime)
        {

            if (FallDamageProtectionActive.Value)
            {
                float timeSinceActivation = Time.realtimeSinceStartup - _lastProtectionActivationTime;
                if (timeSinceActivation > PROTECTION_DURATION)
                {
                    FallDamageProtectionActive.Value = false;
                    Debug.Log("Fall damage protection expired");
                }
            }
        }

        private void HandleFallingChanged(bool wasFalling, bool isFalling)
        {
            if (isFalling && !wasFalling)
            {

                HandleStartFalling();
            }
            else if (!isFalling && wasFalling)
            {

                HandleLanded();
            }
        }

        private void HandleStartFalling()
        {
            if (!_context.IsDead.Value)
            {
                _fallStartY = _context.CharacterTransform.position.y;
            }
        }

        private void HandleLanded()
        {
            if (!_context.IsDead.Value && !FallDamageProtectionActive.Value && !_context.InvulnerabilityLogic.IsInvulnerable)
            {
                float fallDistance = _fallStartY - _context.CharacterTransform.position.y;
                
                if (fallDistance > _context.MinimumFallDamageHeight)
                {

                    float damagePercentage = Mathf.Lerp(
                        _minFallDamagePercent, 
                        _maxFallDamagePercent, 
                        Mathf.InverseLerp(_context.MinimumFallDamageHeight, _context.LethalFallHeight, fallDistance)
                    );
                    
                    float damage = damagePercentage * _context.CharacterStats.HpMax.Value;
                    

                    var damageInfo = new DamageInfo
                    {
                        DamageAmount = damage,
                        PoiseDamageAmount = fallDistance > _context.StaggerThreshold ? _context.CharacterStats.PoiseMax.Value : 0f,
                        WorldPos = _context.CharacterTransform.position,
                        DoneByPlayer = false, 
                        DamageDealer = null   
                    };
                    
                    _context.ApplyDamage.Execute(damageInfo);
                    

                    if (fallDistance > _context.StaggerThreshold)
                    {
                        _context.TriggerStagger.Execute();
                    }
                    
                    Debug.Log($"Fall damage applied: {damage} from height {fallDistance}m");
                }
            }
            else if (FallDamageProtectionActive.Value)
            {
                Debug.Log("Fall damage prevented by perfectly timed roll!");
            }
        }

        public bool TryActivateFallDamageProtection()
        {
            float currentTime = Time.realtimeSinceStartup;
            float timeSinceLastActivation = currentTime - _lastProtectionActivationTime;
            

            if (timeSinceLastActivation > PROTECTION_COOLDOWN)
            {
                FallDamageProtectionActive.Value = true;
                _lastProtectionActivationTime = currentTime;
                Debug.Log("Fall damage protection activated");
                return true;
            }
            
            return false;
        }
 
    }
}
