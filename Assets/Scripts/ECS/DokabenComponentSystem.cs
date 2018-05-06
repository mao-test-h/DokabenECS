using Unity.Entities;
using Unity.Mathematics;

namespace MainContents.ECS
{
    /// <summary>
    /// ドカベンロゴの回転(ComponentSystem)
    /// </summary>
    public class DokabenComponentSystem : ComponentSystem
    {
        // Systemで要求されるComponentData
        struct Group
        {
            public ComponentDataArray<DokabenComponentData> DokabenComponentData;
        }

        [Inject] Group _group;

        protected override void OnUpdate()
        {
            // TODO.

        }
    }
}
