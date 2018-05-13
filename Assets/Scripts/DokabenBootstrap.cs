using System;
using System.Linq;

using UnityEngine;

using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;

namespace MainContents.ECS
{
    /// <summary>
    /// ドカベンロゴの回転(起動部)
    /// </summary>
    public class DokabenBootstrap : MonoBehaviour
    {
        // ------------------------------
        #region // Private Members(Editable)

        /// <summary>
        /// ドカベンロゴのスプライト
        /// </summary>
        [SerializeField] Sprite _dokabenSprite;

        /// <summary>
        /// ドカベンロゴの表示マテリアル
        /// </summary>
        [SerializeField] Material _dokabenMaterial;

        #endregion // Private Members(Editable)

        // ------------------------------
        #region // Private Members

        #endregion // Private Members


        // ----------------------------------------------------
        #region // Unity Events

        /// <summary>
        /// UnityEvent : Start
        /// </summary>
        void Start()
        {
            var entityManager = World.Active.GetOrCreateManager<EntityManager>();

            // ドカベンロゴ用のアーキタイプ
            var archeType = entityManager.CreateArchetype(
                typeof(DokabenComponentData),
                typeof(TransformMatrix));

            // テストで一体生成
            var entity = entityManager.CreateEntity(archeType);
            entityManager.SetComponentData(entity,
                new DokabenComponentData
                {
                    AnimationHeader = 0f,
                });
            entityManager.SetComponentData(entity, new TransformMatrix { Value = float4x4.identity });

            // 描画用の情報としてMeshInstanceRendererを紐付ける
            // ※MeshInstanceRendererとTransformMatrixを紐付けることでMeshInstanceRendererSystemから呼ばれるようになる
            entityManager.AddSharedComponentData(entity, this.CreateDokabenMeshInstanceRenderer());
        }

        #endregion // Unity Events

        // ----------------------------------------------------
        #region // Private Functions

        /// <summary>
        /// ドカベンロゴ表示用のMeshInstanceRendererの生成
        /// </summary>
        /// <returns>生成したMeshInstanceRenderer</returns>
        MeshInstanceRenderer CreateDokabenMeshInstanceRenderer()
        {
            // Sprite to Mesh
            var mesh = new Mesh();
            mesh.SetVertices(Array.ConvertAll(this._dokabenSprite.vertices, _ => (Vector3)_).ToList());
            mesh.SetUVs(0, this._dokabenSprite.uv.ToList());
            mesh.SetTriangles(Array.ConvertAll(this._dokabenSprite.triangles, _ => (int)_), 0);

            // 渡すマテリアルはGPU Instancingに対応させる必要がある
            var meshInstanceRenderer = new MeshInstanceRenderer(); ;
            meshInstanceRenderer.mesh = mesh;
            meshInstanceRenderer.material = this._dokabenMaterial;
            return meshInstanceRenderer;
        }

        #endregion // Private Functions
    }

}