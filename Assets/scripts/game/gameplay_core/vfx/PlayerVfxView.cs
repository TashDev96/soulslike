using System;
using Cysharp.Threading.Tasks;
using game.gameplay_core.characters;
using UnityEngine;

namespace game.gameplay_core.vfx
{
	public class PlayerVfxView : MonoBehaviour
	{
		private const int PartBufferSize = 100;
		[SerializeField]
		private ParticleSystem _experienceParticles;
		private bool _particleTriggered;
		private readonly ParticleSystem.Particle[] _particlesCache = new ParticleSystem.Particle[PartBufferSize];
		private int _aliveParticlesCount;

		public void Initialize(CharacterDomain player)
		{
		}

		public async UniTask ShowExperienceFlight(Vector3 spawnWorldPos)
		{
			var emitParams = new ParticleSystem.EmitParams
			{
				position = spawnWorldPos
			};
			_experienceParticles.Emit(emitParams, 4);

			RetrieveParticlesInfo();

			var lastParticleId = _particlesCache[0].randomSeed;
			var timeStart = Time.time;
			const float maxDuration = 5f;

			while(Time.time - timeStart < maxDuration)
			{
				await UniTask.WaitForEndOfFrame();
				if(_particleTriggered)
				{
					_particleTriggered = false;
					RetrieveParticlesInfo();
					if(!CheckParticleAlive(lastParticleId))
					{
						return;
					}
				}
			}

			await UniTask.DelayFrame(1);
		}

		private void RetrieveParticlesInfo()
		{
			_aliveParticlesCount = _experienceParticles.particleCount;
			_experienceParticles.GetParticles(_particlesCache, Math.Min(_aliveParticlesCount, PartBufferSize - 1));
		}

		private bool CheckParticleAlive(uint particleRandomSeed)
		{
			for(var i = 0; i < _aliveParticlesCount; i++)
			{
				if(_particlesCache[i].randomSeed == particleRandomSeed)
				{
					return true;
				}
			}
			return false;
		}

		private void OnParticleTrigger()
		{
			_particleTriggered = true;
		}
	}
}
