namespace game.gameplay_core
{
	public interface ISavable
	{
		public string Serialize();
		public void Deserialize(string data);
	}
}
