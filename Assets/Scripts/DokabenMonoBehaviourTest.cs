using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MainContents.ECS;

namespace MainContents
{
    /// <summary>
    /// ドカベンロゴの回転(MonoBehaviour)
    /// </summary>
    public class DokabenMonoBehaviourTest : MonoBehaviour
    {
        // ------------------------------
        #region // Defines

        /// <summary>
        /// ドカベンロゴのサイズ
        /// </summary>
        /// <remarks>※Quadを変形させているのでサイズ指定が必要</remarks>
        public static readonly Vector3 DokabenScale = new Vector3(4.82f, 1.3f, 1f);

        /// <summary>
        /// ドカベンロゴ
        /// </summary>
        public class Dokaben
        {
            /// <summary>
            /// 自信のメッシュの参照
            /// </summary>
            Mesh _mesh;

            /// <summary>
            /// 頂点数
            /// </summary>
            int _vertsLength;

            /// <summary>
            /// 元となる頂点
            /// </summary>
            Vector3[] _originalVerts;

            /// <summary>
            /// 計算用頂点バッファ
            /// </summary>
            Vector3[] _vertsBuff;


            /// <summary>
            /// コンストラクタ
            /// </summary>
            /// <param name="basePrefab">ベース</param>
            /// <param name="position">生成一</param>
            public Dokaben(GameObject basePrefab, Vector3 position)
            {
                var obj = Instantiate<GameObject>(basePrefab);
                var trs = obj.transform;
                trs.localPosition = position;
                trs.localScale = DokabenMonoBehaviourTest.DokabenScale;

                this._mesh = obj.GetComponent<MeshFilter>().mesh;
                this._originalVerts = this._mesh.vertices;
                this._vertsLength = this._originalVerts.Length;
                this._vertsBuff = new Vector3[this._vertsLength];
            }

            /// <summary>
            /// 回転処理
            /// </summary>
            public void Rotate(float time)
            {
                Matrix4x4 m = Matrix4x4.identity;

                // 時間の正弦を算出(再生位置を加算することで角度をずらせるように設定)
                float sinTime = Mathf.Sin(time * Constants.AnimationSpeed);

                // _SinTime0~1に正規化→0~15(コマ数分)の範囲にスケールして要素数として扱う
                float normal = (sinTime + 1f) / 2f;

                // X軸に0~90度回転
                var index = (int)Mathf.Round(normal * (Constants.AnimationTable.Length - 1));
                float rot = Constants.AnimationTable[index] * (90 * Mathf.Deg2Rad);

                // 原点を-0.5ずらして下端に設定
                float y = 0f, z = 0f;
                float halfY = y - 0.5f;
                float sin = Mathf.Sin(rot);
                float cos = Mathf.Cos(rot);
                // 任意の原点周りにX軸回転を行う
                m.m11 = cos;
                m.m12 = -sin;
                m.m21 = sin;
                m.m22 = cos;
                m.m13 = halfY - halfY * cos + z * sin;
                m.m23 = z - halfY * sin - z * cos;

                // 算出結果の反映
                for (int i = 0; i < this._vertsLength; ++i)
                {
                    this._vertsBuff[i] = m.MultiplyPoint3x4(this._originalVerts[i]);
                }
                this._mesh.vertices = this._vertsBuff;
            }
        }

        #endregion  // Defines

        // ------------------------------
        #region // Private Members(Editable)

        /// <summary>
        /// ベースのPrefab
        /// </summary>
        [SerializeField] GameObject _basePrefab;

        /// <summary>
        /// 表示領域のサイズ
        /// </summary>
        [SerializeField] Vector3 _BoundSize = new Vector3(256f, 256f, 256f);

        /// <summary>
        /// 最大オブジェクト数
        /// </summary>
        [SerializeField] int _MaxObjectNum = 100000;

        #endregion // Private Members(Editable)

        // ------------------------------
        #region // Private Members


        /// <summary>
        /// 生成したドカベンロゴ一覧
        /// </summary>
        Dokaben[] _dokabens = null;

        #endregion // Private Members



        // ----------------------------------------------------
        #region // Unity Events

        void Start()
        {
            this._dokabens = new Dokaben[this._MaxObjectNum];
            var halfX = this._BoundSize.x / 2;
            var halfY = this._BoundSize.y / 2;
            var halfZ = this._BoundSize.z / 2;
            for (int i = 0; i < this._MaxObjectNum; ++i)
            {
                var pos = new Vector3(
                    Random.Range(-halfX, halfX),
                    Random.Range(-halfY, halfY),
                    Random.Range(-halfZ, halfZ));

                this._dokabens[i] = new Dokaben(this._basePrefab, pos);
            }
        }

        void Update()
        {
            float time = Time.time;
            for (int i = 0; i < this._MaxObjectNum; ++i)
            {
                this._dokabens[i].Rotate(time);
            }
        }

        #endregion // Unity Events
    }
}
