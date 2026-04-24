using UnityEngine;

namespace game.gameplay_core.characters.runtime_data
{
	public class RigidBodyWrapper
	{
		private readonly Rigidbody _rigidbody;

		public float Mass => _rigidbody.mass;

		public Vector3 LinearVelocity
		{
			get => _rigidbody.linearVelocity;
			set => _rigidbody.linearVelocity = value;
		}

		public RigidBodyWrapper(Rigidbody rigidbody)
		{
			_rigidbody = rigidbody;
		}
	}
}
