using UnityEngine;

using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

using Unity.Jobs;
using Unity.Collections;

namespace MainContents.ECS
{
    // 共通定数
    public class Constants
    {
        // コマ数
        public const int Framerate = 9;
        // 1コマに於ける回転角度
        public const float Angle = (90f / Framerate); // 90度をコマ数で分割
        // コマ中の待機時間
        public const float Interval = 0.2f;

        // コマ落ちアニメーションテーブル
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

        // アニメーションの再生速度
        public const float AnimationSpeed = 1f;

    }

    /// <summary>
    /// ドカベンロゴの回転(ComponentSystem)
    /// </summary>
    public class DokabenComponentSystem : ComponentSystem
    {
        // Systemで要求されるComponentData
        struct Group
        {
            public int Length;
            public ComponentDataArray<DokabenComponentData> Dokabens;
            public ComponentDataArray<TransformMatrix> Transforms;
        }

        [Inject] Group _group;

        protected override void OnUpdate()
        {
            float time = Time.time;
            float animLength = Constants.AnimationTable.Length;

            for (int i = 0; i < this._group.Length; i++)
            {
                var data = this._group.Dokabens[i];
                float4x4 m = float4x4.identity;

                // 時間の正弦を算出(再生位置を加算することで角度をずらせるように設定)
                float sinTime = math.sin(time * Constants.AnimationSpeed) + data.AnimationHeader;

                // _SinTime0~1に正規化→0~15(コマ数分)の範囲にスケールして要素数として扱う
                float normal = (sinTime + 1f) / 2f;

                // X軸に0~90度回転
                int index = (int)math.round(normal * (animLength - 1));
                float rot = Constants.AnimationTable[index] * math.radians(90f);

                // 任意の原点周りにX軸回転を行う(原点を-0.5ずらして下端に設定)
                // ※Matrix4x4は列優先でfloat4x4は行優先みたいなので注意
                float y = 0f, z = 0f;
                float halfY = y - 0.5f;
                float sin = math.sin(rot);
                float cos = math.cos(rot);
                m.m1.y = cos;
                m.m1.z = sin;
                m.m2.y = -sin;
                m.m2.z = cos;
                m.m3.y = halfY - halfY * cos + z * sin;
                m.m3.z = z - halfY * sin - z * cos;


                // 計算結果の保持
                this._group.Dokabens[i] = data;

                var trs = this._group.Transforms[i];
                trs.Value = m;
                this._group.Transforms[i] = trs;
            }
        }
    }
}
