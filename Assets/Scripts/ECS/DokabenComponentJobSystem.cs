// 定義時はJobComponentSystemを継承した実装で回す
// ※未定義時の「JobComponentSystemを継承していない方の実装例」について、
//   Editor上でパフォーマンス計測した所、明らかに遅かったので多分使わないパターンと思われる。→一応は参考程度に実装だけ残しておく。
#define ENABLE_JOB_COMPONENT_SYSTEM 

using UnityEngine;

using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Rendering;

#if ENABLE_JOBSYSTEM
namespace MainContents.ECS
{
#if ENABLE_JOB_COMPONENT_SYSTEM

    /// <summary>
    /// ドカベンロゴの回転(ComponentSystem) ※JobSystem併用版
    /// </summary>
    /// <remarks>JobComponentSystemを継承した実装</remarks>
    [UpdateAfter(typeof(MeshInstanceRendererSystem))]   // MeshInstanceRendererSystemでJob待ち?が発生するっぽいので後に実行。しかし毎フレーム解決されるわけではない...
    public class DokabenComponentJobSystem : JobComponentSystem
    {
        /// <summary>
        /// ドカベンロゴ回転計算用Job
        /// </summary>
        //[ComputeJobOptimization]  // TODO BurstCompolerについて、static、配列などが使えないので作り変える必要ありそう感
        struct DokabenJob : IJobProcessComponentData<DokabenComponentData, TransformMatrix>
        {
            public float Time;

            // 時間の設定
            public void SetTime(float time)
            {
                Time = time;
            }

            // Jobで実行されるコード
            public void Execute(ref DokabenComponentData data, ref TransformMatrix transform)
            {
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
                m.m1.yz = new float2(cos, sin);
                m.m2.yz = new float2(-sin, cos);
                m.m3.yz = new float2(halfY - halfY * cos + z * sin, z - halfY * sin - z * cos);

                // 移動
                m.m3.xyz += data.Position.xyz;

                // 計算結果の反映
                transform.Value = m;
            }
        }

        DokabenJob _job;

        protected override void OnCreateManager(int capacity)
        {
            base.OnCreateManager(capacity);
            this._job = new DokabenJob();
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            // Jobの実行
            this._job.SetTime(Time.time);
            return this._job.Schedule(this, 7, inputDeps);
        }
    }

#else

    /// <summary>
    /// ドカベンロゴの回転(ComponentSystem)
    /// </summary>
    /// <remarks>ComponentSystem.OnUpdate内でJobを作成して回す実装</remarks>
    public class DokabenComponentJobSystem : ComponentSystem
    {
        // Systemで要求されるComponentData
        struct Group
        {
            public int Length;
            public ComponentDataArray<DokabenComponentData> Dokabens;
            public ComponentDataArray<TransformMatrix> Transforms;
        }

        /// <summary>
        /// ドカベンロゴ回転計算用Job
        /// </summary>
        struct DokabenParallelForUpdate : IJobParallelFor
        {
            public Group Group;
            [ReadOnly] public float Time;

            // Jobで実行されるコード
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
#endif
}
#endif