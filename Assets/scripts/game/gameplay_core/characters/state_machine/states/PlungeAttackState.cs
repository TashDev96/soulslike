using game.gameplay_core.characters.config.animation;
using game.gameplay_core.characters.view;
using game.gameplay_core.damage_system;
using game.gameplay_core.inventory.item_configs;

namespace game.gameplay_core.characters.state_machine.states
{
	public class PlungeAttackState : CharacterStateBase
	{
		private float _fallDamageBonus;
		private readonly PlungeAttackTargetView _pivot;

		private readonly CharacterDomain _target;
		private AnimationConfig _targetAnimation;
		private WeaponItemConfig _weaponConfig;

		private float _time;

		protected float TargetAnimNormalizedTime => _time / _targetAnimation.Duration;

		public PlungeAttackState(CharacterContext context, CharacterDomain target, PlungeAttackTargetView pivot) : base(context)
		{
			_pivot = pivot;
			_target = target;
		}

		public override void OnEnter()
		{
			_time = 0;
			_target.CharacterStateMachine.LockInAnimation(_pivot.TargetAnimation);
			_pivot.SetAttackerLocalRotation(_context.Transform);

			_fallDamageBonus = _context.Logic.FallDamageLogic.FallSpeed;
			_context.Logic.MovementLogic.ResetVelocity();
			_context.Logic.MovementLogic.LockedInAnimationSlot = true;

			_targetAnimation = _pivot.TargetAnimation;

			_weaponConfig = _context.Logic.InventoryLogic.RightWeapon.Config;
			base.OnEnter();
		}

		public override void OnExit()
		{
			base.OnExit();
			_context.Logic.MovementLogic.LockedInAnimationSlot = false;
			_context.Transform.ResetRotationVertical();
		}

		public override void Update(float deltaTime)
		{
			var oldTime = TargetAnimNormalizedTime;
			_time += deltaTime;
			var newTime = TargetAnimNormalizedTime;

			_context.Transform.Position = _pivot.GetWorldPosAttacker();
			_context.Transform.Rotation = _pivot.GetWorldRotationAttacker();

			if(_targetAnimation.CheckFlagBegin(AnimationFlags.TakeDamage, oldTime, newTime))
			{
				ApplyGuaranteedDamage();
			}

			if(_targetAnimation.CheckFlagEnded(AnimationFlags.StateLocked, oldTime, newTime))
			{
				IsComplete = true;
				_context.Logic.MovementLogic.LockedInAnimationSlot = false;
				_context.Logic.MovementLogic.SetFallVelocity(_pivot.AttackerOutSpeed);
				_context.Transform.ResetRotationVertical();
			}
		}

		private void ApplyGuaranteedDamage()
		{
			if(_target == null)
			{
				return;
			}

			var damageAmount = _fallDamageBonus * _weaponConfig.FallDamageMultiplier;

			var damageInfo = new DamageInfo
			{
				DamageAmount = damageAmount,
				PoiseDamageAmount = 0.1f,
				WorldPos = _context.Transform.Position,
				DoneByPlayer = _context.IsPlayer.Value,
				DamageDealer = _context.SelfLink,
				DeflectionRating = 0,
				KnockbackImpulse = 0,
				Direction = _context.Transform.Forward,
				IsPlunge = true
			};

			_target.ExternalData.ApplyDamage.Execute(damageInfo);
		}
	}
}
