namespace SkinnedMeshInstancing.AnimationBaker.AnimationController
{
	public struct AnimationClipPlayInfo
	{
		public int ClipNameHash;
		public int Loops;

		public AnimationClipPlayInfo(int clipNameHash, int loops)
		{
			ClipNameHash = clipNameHash;
			Loops = loops;
		}
	}
}
