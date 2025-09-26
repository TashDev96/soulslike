using System.Collections.Generic;

namespace SkinnedMeshInstancing
{
    public interface IMeshInstanceRenderer
    {
        void RenderInstances(IEnumerable<IMeshInstanceInfo> instances);
        void RegisterInstance(IMeshInstanceInfo instance);
        void UnregisterInstance(IMeshInstanceInfo instance);
        void ClearAllInstances();
        int GetTotalInstanceCount();
    }
}
