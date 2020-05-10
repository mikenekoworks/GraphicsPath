using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GraphicsPath
{

    public partial class BezierCurve
    {

        /// <summary>
        /// 2点間おおよその長さを取得します。
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="segment"></param>
        /// <returns></returns>
        public static float ApproximateLength( AnchorPoint p1, AnchorPoint p2, int segment )
        {
            float length = 0;

            Vector3 last_pos = p1.position;
            Vector3 current_pos;

            for ( int i = 0; i < segment + 1; ++i )
            {
                current_pos = GetInterpolationPoint( p1, p2, i / (float)segment );
                length += ( current_pos - last_pos ).magnitude;
                last_pos = current_pos;
            }

            return length;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="segment"></param>
        /// <returns></returns>
        public static List<float> Segmentalized( AnchorPoint p1, AnchorPoint p2, int segment )
        {
            List<float> list = new List<float>();

            Vector3 last_pos = p1.position;
            Vector3 current_pos;

            for ( int i = 1; i < segment + 1; ++i )
            {
                current_pos = GetInterpolationPoint( p1, p2, i / ( float )segment );
                list.Add( ( current_pos - last_pos ).magnitude );
                last_pos = current_pos;
            }

            return list;
        }

        /// <summary>
        /// 2点間をセグメントで分割した座標を取得する
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="segment"></param>
        /// <returns></returns>
        public static List<Vector3> GetInterpolationPoints( AnchorPoint p1, AnchorPoint p2, int segment )
        {
            var points = new List< Vector3 >();

            for ( int i = 0; i < segment + 1; ++i )
            {
                points.Add( GetInterpolationPoint( p1, p2, i / ( float )segment ) );
            }

            return points;
        }

        /// <summary>
        /// 曲線間の中間点を取得
        /// </summary>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static Vector3 GetInterpolationPoint( AnchorPoint p0, AnchorPoint p1, float t )
        {

            if ( ( p0.OutHandle == Vector3.zero ) && ( p1.InHandle == Vector3.zero ) )
            {
                // 両アンカーの制御点がない場合は線形補間
                return Vector3.Lerp( p0.position, p1.position, t );
            }

            if ( ( p0.OutHandle != Vector3.zero ) && ( p1.InHandle != Vector3.zero ) )
            {
                // 両アンカーの制御点がある場合は三次曲線
                return GetCubicCurvePoint( p0.position, p0.position + p0.OutHandle, p1.position + p1.InHandle, p1.position, t );
            }

            // どちらか片方の場合は二次曲線

            var pos0 = p0.position;
            var pos1 = p0.position + p0.OutHandle;
            var pos2 = p1.position;

            if ( p0.OutHandle == Vector3.zero )
            {
                pos1 = p1.position + p1.InHandle;
            }

            return GetQuadraticCurvePoint( pos0, pos1, pos2, t );

        }

        /// <summary>
        /// 二次曲線
        /// P = (1−t)^2 * P1 + 2 * (1−t) * t * P2 + t^2 * P3
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        private static Vector3 GetQuadraticCurvePoint( Vector3 p1, Vector3 p2, Vector3 p3, float t )
        {
            float one_minus = 1.0f - t;

            return ( one_minus * one_minus * p1 ) +
                   ( 2 * one_minus * t * p2 ) +
                   ( t * t * p3 );

        }

        /// <summary>
        /// 三次曲線
        /// P = (1−t)^3 * P1 + 3 * (1−t)^2 * t * P2 +3 * (1−t) * t ^ 2 * P3 + t^3 * P4
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        /// <param name="p4"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static Vector3 GetCubicCurvePoint( Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4, float t )
        {
            t = Mathf.Clamp01( t );

            float one_minus = 1.0f - t;

            return ( one_minus * one_minus * one_minus * p1 ) +
                   ( 3 * one_minus * one_minus * t * p2 ) +
                   ( 3 * one_minus * t * t * p3 ) +
                   ( t * t * t * p4 );
        }

        /// <summary>
        /// インデックスを計算する
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public static int GetNextIndex( int index, int item_count, bool is_loop )
        {
            if ( is_loop == true )
            {
                return ( index + 1 ) % item_count;
            }

            if ( ( index + 1 ) < item_count )
            {
                return index + 1;
            }

            return -1;
        }

    }
}
