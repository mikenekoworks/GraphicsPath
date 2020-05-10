using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GraphicsPath
{

    public class CurveTracer : MonoBehaviour
    {
        [SerializeField]
        private SegmentData segmentData;

        [SerializeField]
        private GameObject target;

        [SerializeField]
        private float speed;

        private float elapsedTime = 0.0f;
        private float movedDistance = 0.0f;
        private float totalLength = 0.0f;

        public void Initialize()
        {
            Reset();
        }

        public void Reset()
        {
            elapsedTime = 0.0f;
            movedDistance = 0.0f;

            if ( segmentData != null )
            {
                totalLength = segmentData.TotalLength;

                if ( target != null )
                {
                    target.transform.position = transform.position + segmentData.Points[ 0 ];
                }
            }

        }

        public void OnUpdate( float delta_time )
        {

            int next_index = 0;
            float progress = 0.0f;

            for ( int i = 0; i < segmentData.Points.Count; i++ )
            {
                next_index = BezierCurve.GetNextIndex( i, segmentData.Points.Count, segmentData.IsClosed );

                if ( next_index == -1 )
                {
                    break;
                }

                if ( ( progress < movedDistance ) && ( movedDistance <= progress + segmentData.Distances[ i ] ) )
                {

                    float t = ( movedDistance - progress ) / segmentData.Distances[ i ];

                    target.transform.position = transform.position + Vector3.Lerp( segmentData.Points[ i ], segmentData.Points[ next_index ], t );

                    break;
                }

                progress += segmentData.Distances[ i ];

            }

            movedDistance = Mathf.Repeat( movedDistance + speed * delta_time, totalLength );
        }

        #if UNITY_EDITOR

        public void OnDrawGizmosSelected()
        {
            if ( segmentData == null )
            {
                return;
            }

            Vector3 base_pos = transform.position;

            int next_index = 0;

            for ( int i = 0; i < segmentData.Points.Count; i++ )
            {
                next_index = BezierCurve.GetNextIndex( i, segmentData.Points.Count, segmentData.IsClosed );

                if ( next_index == -1 )
                {
                    break;
                }

                Gizmos.color = Color.green;
                Gizmos.DrawLine( base_pos + segmentData.Points[ i ], base_pos + segmentData.Points[ next_index ] );

                Gizmos.color = Color.white;
                Gizmos.DrawWireSphere( base_pos + segmentData.Points[ i ], UnityEditor.HandleUtility.GetHandleSize( segmentData.Points[ i ] ) * 0.05f );
            }

            // クローズしてない時の終端を描画
            if ( segmentData.IsClosed == false )
            {
                int last_index = segmentData.Points.Count - 1;

                Gizmos.color = Color.white;
                Gizmos.DrawWireSphere( base_pos + segmentData.Points[ last_index ], UnityEditor.HandleUtility.GetHandleSize( segmentData.Points[ last_index ] ) * 0.05f );
            }

        }
        #endif

    }

}