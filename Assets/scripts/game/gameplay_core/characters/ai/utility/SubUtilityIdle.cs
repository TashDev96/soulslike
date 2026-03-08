namespace game.gameplay_core.characters.ai.utility
{
	public class SubUtilityIdle : SubUtilityBase
	{
		public override float GetExecutionWorthWeight()
		{
			return 0.001f;
		}
	}
}
