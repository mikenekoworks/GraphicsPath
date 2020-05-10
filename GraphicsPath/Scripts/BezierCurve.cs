using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GraphicsPath
{

    [System.Serializable]
    public partial class BezierCurve
    {
        [SerializeField]
        private List<AnchorPoint> anchorPoints = new List<AnchorPoint>();

        /// <summary>
        /// アンカーポイントの個数
        /// </summary>
        public int PointCount
        {
            get { return anchorPoints.Count; }
        }

        /// <summary>
        /// アンカーポイントを配列にして取得
        /// </summary>
        /// <returns></returns>
        public AnchorPoint[] ToArray()
        {
            return anchorPoints.ToArray();
        }

        /// <summary>
        /// ベジェ曲線を線分化します
        /// </summary>
        /// <param name="points"></param>
        public List<Vector3> ToLineSegments()
        {
            var points = new List<Vector3>();
            var next_index = 0;

            for ( int i = 0; i < PointCount; i++ )
            {
                next_index = GetNextIndex( i, PointCount, IsClosed );

                if ( next_index == -1 )
                {
                    break;
                }

                var new_points = GetInterpolationPoints( anchorPoints[ i ], anchorPoints[ next_index ], Segment );

                if ( i != 0 )
                {
                    // 先頭の頂点は重複するので削除する。
                    new_points.RemoveAt( 0 );
                }

                points.AddRange( new_points );
            }

            // クローズパスは最期が先頭と重複するので削除する。
            if ( isClosed == true )
            {
                points.Remove( points.Last() );
            }

            return points;
        }

        /// <summary>
        /// インデックス指定でアンカーポイントを取得
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public AnchorPoint this[ int index ]
        {
            get
            {
                return anchorPoints[ index ];
            }
            set
            {
                anchorPoints[ index ] = value;
            }
        }

        public AnchorPoint First()
        {
            return anchorPoints[ 0 ];
        }

        public AnchorPoint Last()
        {
            return anchorPoints.Last();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public AnchorPoint FindByGuid( string guid )
        {
            return anchorPoints.Find( anchor => anchor.Guid == guid );
        }
        public int FindIndexByGuid( string guid )
        {
            return anchorPoints.FindIndex( anchor => anchor.Guid == guid );
        }

        /// <summary>
        /// アンカー間の分割数
        /// </summary>
        [SerializeField]
        private int segment = 30;
        public int Segment
        {
            get
            {
                return segment;
            }
            set
            {
                if ( segment != value )
                {
                    Segment = value;
                    CalculateLength();
                }
            }
        }

        /// <summary>
        /// 線分が閉じるか？
        /// </summary>
        ///
        [SerializeField]
        private bool isClosed;
        public bool IsClosed
        {
            get { return isClosed;  }
            set { isClosed = value; }
        }

        /// <summary>
        /// 長さ
        /// </summary>
        private float length;
        public float Length
        {
            get
            {
                if ( needRecalculateLength == true )
                {
                    CalculateLength();
                }

                return length;
            }
        }

        private bool needRecalculateLength = true;

        public void CalculateLength()
        {

            length = 0.0f;
            var next_index = 0;

            for ( int i = 0; i < PointCount; i++ )
            {
                next_index = GetNextIndex( i, PointCount, IsClosed );

                if ( next_index == -1 )
                {
                    break;
                }

                length += ApproximateLength( anchorPoints[ i ], anchorPoints[ next_index ], Segment );
            }


            needRecalculateLength = false;

        }

        /// <summary>
        /// アンカーポイントの追加
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public AnchorPoint Add( Vector3 pos )
        {

            AnchorPoint p = new AnchorPoint
            {
                position = pos,
                HandleStyle = AnchorPoint.TypeHandleStyle.None
            };

            anchorPoints.Add( p );

            CalculateLength();

            return p;
        }

        /// <summary>
        /// アンカーポイントの削除
        /// </summary>
        /// <param name="p"></param>
        public void Remove( AnchorPoint p )
        {
            anchorPoints.Remove( p );

            CalculateLength();
        }

        /// <summary>
        /// アンカーポイントの全削除
        /// </summary>
        public void Clear()
        {
            anchorPoints.Clear();

            CalculateLength();

        }

        /// <summary>
        /// 全体単位時間から座標を求める
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public Vector3 Evaluate( float t )
        {
            t = Mathf.Clamp01( t );

            if ( t <= 0.0f )
            {
                return anchorPoints.First().position;
            }

            if ( t >= 1.0f )
            {
                if ( IsClosed == true )
                {
                    return anchorPoints.First().position;
                }

                return anchorPoints.Last().position;
            }


            float current_length = 0;
            float seg_length = 0;
            float target_length = t * Length;

            AnchorPoint p1 = null;
            AnchorPoint p2 = null;

            int next_index = 0;
            int i;

            // 目的の評価時間をさがす。
            for ( i = 0; i < anchorPoints.Count; ++i )
            {
                next_index = GetNextIndex( i, anchorPoints.Count, IsClosed );

                if ( next_index == -1 )
                {
                    break;
                }

                seg_length = ApproximateLength( anchorPoints[ i ], anchorPoints[ next_index ], Segment );

                p1 = anchorPoints[ i ];
                p2 = anchorPoints[ next_index ];

                if ( ( current_length < target_length ) && ( target_length <= current_length + seg_length ) )
                {
                    // 2点間の間に収まっているので終了
                    break;
                }

                current_length += seg_length;
            }

            float base_percent = current_length / Length;

            t -= base_percent;

            return GetInterpolationPoint( p1, p2, t / ( seg_length / Length ) );
        }

    }

}
