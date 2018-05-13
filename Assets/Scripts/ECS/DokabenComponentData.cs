using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace MainContents.ECS
{
    /// <summary>
    /// ドカベンロゴの回転(ComponentData)
    /// </summary>
    public struct DokabenComponentData : IComponentData
    {
        /// <summary>
        /// アニメーションテーブル内に於ける再生位置
        /// </summary>
        public float AnimationHeader;
        /// <summary>
        /// 位置
        /// </summary>
        public float3 Position;
    }
}