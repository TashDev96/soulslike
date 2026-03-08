using System.Text;

namespace game.gameplay_core.characters.ai
{
	public interface ICharacterBrain
	{
		public void Initialize(CharacterContext context);
		public void Think(float deltaTime);
		public void GetDebugString(StringBuilder sb);
		void Reset();
	}
}
