using Unity.Entities;
using Unity.Mathematics;

namespace MainContents.ECS
{
    /// <summary>
    /// ドカベンロゴの回転(ComponentData)
    /// </summary>
    public struct DokabenComponentData : IComponentData
    {
        /// <summary>
        /// 経過時間計測用
        /// </summary>
        public float DeltaTimeCounter;

        /// <summary>
        /// コマ数のカウンタ
        /// </summary>
        public int FrameCounter;

        /// <summary>
        /// 1コマに於ける回転角度
        /// </summary>
        public float CurrentAngle;

        /// <summary>
        /// 算出した回転情報を保持
        /// </summary>
        public float4x4 Matrix;
    }
}