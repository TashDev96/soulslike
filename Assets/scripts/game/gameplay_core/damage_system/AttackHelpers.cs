using dream_lib.src.utils.drawers;
using game.gameplay_core.characters;
using game.gameplay_core.characters.runtime_data;
using game.gameplay_core.location.view;
using game.gameplay_core.utils;
using UnityEngine;

namespace game.gameplay_core.damage_system
{
	public class AttackHelpers
	{
		private static readonly int LayerMaskDamageReceivers = LayerMask.GetMask("DamageReceivers");
		private static readonly int LayerMaskWalls = LayerMask.GetMask("Default");
		private static readonly Collider[] Results = new Collider[40];

		public static bool CastAttackObstacles(CapsuleCaster hitCaster, bool checkBlockReceivers, bool checkWalls, bool drawDebug = false)
		{
			var radius = hitCaster.Radius;
			hitCaster.GetCapsulePoints(out var point0, out var point1);

			if(drawDebug)
			{
				DebugDrawUtils.DrawWireCapsulePersistent(point0, point1, radius, Color.blue, Time.deltaTime);
			}

			if(checkWalls)
			{
				var count = Physics.OverlapCapsuleNonAlloc(point0, point1, radius, Results, LayerMaskWalls);
				for(var j = 0; j < count; j++)
				{
					if(Results[j].TryGetComponent<SurfacePropertiesView>(out var surfaceProperties) && !surfaceProperties.StopWeapons)
					{
						continue;
					}
					DebugDrawUtils.DrawWireCapsulePersistent(point0, point1, radius, Color.blue, 1f);
					return true;
				}
			}

			if(checkBlockReceivers)
			{
				var count = Physics.OverlapCapsuleNonAlloc(point0, point1, radius, Results, LayerMaskDamageReceivers);

				for(var j = 0; j < count; j++)
				{
					if(Results[j].TryGetComponent<BlockReceiver>(out _))
					{
						if(drawDebug)
						{
							DebugDrawUtils.DrawWireCapsulePersistent(point0, point1, radius, Color.blue, 1f);
						}
						return true;
					}
				}
			}

			return false;
		}

		public static void CastAttack(float baseDamage, HitData hitData, CapsuleCaster hitCaster, CharacterContext casterContext, int deflectionRating = 0, bool drawDebug = false)
		{
			var radius = hitCaster.Radius;

			hitCaster.GetCapsulePoints(out var point0, out var point1);

			if(drawDebug)
			{
				DebugDrawUtils.DrawWireCapsulePersistent(point0, point1, radius, Color.red, Time.deltaTime);
			}

			var count = Physics.OverlapCapsuleNonAlloc(point0, point1, radius, Results, LayerMaskDamageReceivers);

			for(var j = 0; j < count; j++)
			{
				if(hitData.ImpactedTargets.Contains(Results[j]))
				{
					continue;
				}

				if(Results[j].TryGetComponent<ParryReceiver>(out var parryReceiver))
				{
					if(parryReceiver.OwnerTeam == casterContext.Team.Value && !hitData.Config.FriendlyFire)
					{
						continue;
					}

					if(hitData.ImpactedCharacters.Contains(parryReceiver.CharacterId) || parryReceiver.CharacterId == casterContext.CharacterId.Value)
					{
						continue;
					}

					hitData.ImpactedTargets.Add(Results[j]);
					hitData.ImpactedCharacters.Add(parryReceiver.CharacterId);

					if(parryReceiver.TryResolveParry(casterContext.SelfLink))
					{
						return;
					}
				}

				if(Results[j].TryGetComponent<BlockReceiver>(out var blockReceiver))
				{
					if(blockReceiver.OwnerTeam == casterContext.Team.Value && !hitData.Config.FriendlyFire)
					{
						continue;
					}

					if(hitData.ImpactedCharacters.Contains(blockReceiver.CharacterId) || blockReceiver.CharacterId == casterContext.CharacterId.Value)
					{
						continue;
					}

					hitData.ImpactedTargets.Add(Results[j]);
					hitData.ImpactedCharacters.Add(blockReceiver.CharacterId);

					blockReceiver.ApplyDamage(CreateDamageInfo(baseDamage, hitData, Results[j], casterContext, hitCaster, deflectionRating), out var attackDeflected);

					if(attackDeflected)
					{
						if(drawDebug)
						{
							DebugDrawUtils.DrawWireCapsulePersistent(point0, point1, radius, Color.yellow, 1f);
						}
						casterContext.DeflectCurrentAttack.Execute();
						return;
					}
				}
			}

			for(var j = 0; j < count; j++)
			{
				if(hitData.ImpactedTargets.Contains(Results[j]))
				{
					continue;
				}

				hitData.ImpactedTargets.Add(Results[j]);
				if(!Results[j].TryGetComponent<DamageReceiver>(out var damageReceiver))
				{
					continue;
				}

				if(damageReceiver.OwnerTeam == casterContext.Team.Value && !hitData.Config.FriendlyFire)
				{
					continue;
				}

				if(hitData.ImpactedCharacters.Contains(damageReceiver.CharacterId) || damageReceiver.CharacterId == casterContext.CharacterId.Value)
				{
					continue;
				}

				hitData.ImpactedCharacters.Add(damageReceiver.CharacterId);

				damageReceiver.ApplyDamage(CreateDamageInfo(baseDamage, hitData, Results[j], casterContext, hitCaster));
			}
		}

		private static DamageInfo CreateDamageInfo(float baseDamage, HitData hitData, Collider targetCollider, CharacterContext casterContext, CapsuleCaster hitCaster, int deflectionRating = 0)
		{
			hitCaster.GetCapsulePoints(out var point0, out var point1);
			var hitCapsuleCenter = (point0 + point1) / 2f;
			var worldPos = targetCollider.ClosestPoint(hitCapsuleCenter);
			var movementDirection = hitCaster.SampleMoveDirection(worldPos);
			var direction = movementDirection.sqrMagnitude > 0.0001f ? movementDirection.normalized : (targetCollider.transform.position - hitCapsuleCenter).normalized;
			return new DamageInfo
			{
				DamageAmount = baseDamage * hitData.Config.DamageMultiplier,
				PoiseDamageAmount = hitData.Config.PoiseDamage,
				WorldPos = worldPos,
				Direction = direction,
				DoneByPlayer = casterContext.IsPlayer.Value,
				DamageDealer = casterContext.SelfLink,
				DeflectionRating = deflectionRating
			};
		}
	}
}
