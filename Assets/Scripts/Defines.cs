namespace MainContents.ECS
{
    /// <summary>
    /// 共通定数
    /// </summary>
    public class Constants
    {
        /// <summary>
        /// コマ落ちアニメーションテーブル
        /// </summary>
        public static readonly float[] AnimationTable =
            new float[] {
                1f,
                0.9333333333333333f,
                0.8666666666666667f,
                0.8f,
                0.7333333333333333f,
                0.6666666666666666f,
                0.6f,
                0.5333333333333333f,
                0.4666666666666667f,
                0.4f,
                0.3333333333333333f,
                0.26666666666666666f,
                0.2f,
                0.13333333333333333f,
                0.06666666666666667f,
                0f };

        /// <summary>
        /// アニメーションの再生速度
        /// </summary>
        public const float AnimationSpeed = 1f;

    }
}