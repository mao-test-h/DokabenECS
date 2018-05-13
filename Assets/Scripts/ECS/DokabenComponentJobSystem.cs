using UnityEngine;

using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;
using Unity.Jobs;

#if ENABLE_JOBSYSTEM
namespace MainContents.ECS
{
    /// <summary>
    /// ドカベンロゴの回転(ComponentSystem) ※JobSystem併用版
    /// </summary>
    public class DokabenComponentJobSystem : ComponentSystem
    {
        // Systemで要求されるComponentData
        struct Group
        {
            public int Length;
            public ComponentDataArray<DokabenComponentData> Dokabens;
            public ComponentDataArray<TransformMatrix> Transforms;
        }

        struct DokabenParallelForUpdate : IJobParallelFor
        {
            public Group Group;
            [ReadOnly] public float Time;

            // Jobで実行されるコード実行
            public void Execute(int index)
            {
                var data = this.Group.Dokabens[index];
                float4x4 m = float4x4.identity;

                // 時間の正弦を算出(再生位置を加算することで角度をずらせるように設定)
                float sinTime = math.sin(this.Time * Constants.AnimationSpeed) + data.AnimationHeader;

                // _SinTime0~1に正規化→0~15(コマ数分)の範囲にスケールして要素数として扱う
                float normal = (sinTime + 1f) / 2f;

                // X軸に0~90度回転
                var animIndex = (int)math.round(normal * (Constants.AnimationTable.Length - 1));
                float rot = Constants.AnimationTable[animIndex] * math.radians(90f);

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

                // 移動
                m.m3.x += data.Position.x;
                m.m3.y += data.Position.y;
                m.m3.z += data.Position.z;


                // 計算結果の保持
                this.Group.Dokabens[index] = data;

                var trs = this.Group.Transforms[index];
                trs.Value = m;
                this.Group.Transforms[index] = trs;
            }
        }

        [Inject] Group _group;

        protected override void OnUpdate()
        {
            // 回転計算用jobの作成
            DokabenParallelForUpdate rotateJob = new DokabenParallelForUpdate()
            {
                Time = Time.time,
                Group = this._group,
            };
            // Jobの実行
            var jobHandle = rotateJob.Schedule(this._group.Length, 7);
            // Jobが終わるまで処理をブロック
            jobHandle.Complete();
        }
    }
}
#endif