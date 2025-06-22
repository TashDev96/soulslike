using dream_lib.src.utils.drawers;
using game.gameplay_core.characters;
using game.gameplay_core.characters.runtime_data;
using game.gameplay_core.utils;
using UnityEngine;

namespace game.gameplay_core.damage_system
{
	public class AttackHelpers
	{
		private static readonly int LayerMask = UnityEngine.LayerMask.GetMask("DamageReceivers");
		private static readonly Collider[] Results = new Collider[40];

		public static void CastAttack(float baseDamage, HitData hitData, CapsuleCaster hitCaster, CharacterContext casterContext, int deflectionRating = 0, bool drawDebug = false)
		{
			var radius = hitCaster.Radius;

			hitCaster.GetCapsulePoints(out var point0, out var point1);

			if(drawDebug)
			{
				DebugDrawUtils.DrawWireCapsulePersistent(point0, point1, radius, Color.red, Time.deltaTime);
			}

			var count = Physics.OverlapCapsuleNonAlloc(point0, point1, radius, Results, LayerMask);

			for(var j = 0; j < count; j++)
			{
				if(hitData.ImpactedTargets.Contains(Results[j]))
				{
					continue;
				}
				var blockReceiver = Results[j].GetComponent<BlockReceiver>();

				if(!blockReceiver)
				{
					continue;
				}
				
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

				blockReceiver.ApplyDamage(new DamageInfo
				{
					DamageAmount = baseDamage * hitData.Config.DamageMultiplier,
					PoiseDamageAmount = hitData.Config.PoiseDamage,
					WorldPos = Vector3.Lerp((point0 + point1) / 2f, Results[j].transform.position, 0.3f),
					DoneByPlayer = casterContext.IsPlayer.Value,
					DamageDealer = casterContext.SelfLink,
					DeflectionRating = deflectionRating,
				}, out var attackDeflected);

				if(attackDeflected)
				{
					casterContext.DeflectCurrentAttack.Execute();
					return;
				}
			}

			for(var j = 0; j < count; j++)
			{
				if(hitData.ImpactedTargets.Contains(Results[j]))
				{
					continue;
				}

				hitData.ImpactedTargets.Add(Results[j]);
				var damageReceiver = Results[j].GetComponent<DamageReceiver>();

				if(!damageReceiver)
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

				damageReceiver.ApplyDamage(new DamageInfo
				{
					DamageAmount = baseDamage * hitData.Config.DamageMultiplier,
					PoiseDamageAmount = hitData.Config.PoiseDamage,
					WorldPos = Vector3.Lerp((point0 + point1) / 2f, Results[j].transform.position, 0.3f),
					DoneByPlayer = casterContext.IsPlayer.Value,
					DamageDealer = casterContext.SelfLink
				});
			}
		}
	}
}
