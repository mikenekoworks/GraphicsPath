using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GraphicsPath
{

    public partial class BezierCurve
    {
        [System.Serializable]
        public class AnchorPoint
        {
            public enum TypeHandleStyle
            {
                FreeSmooth,
                Broken,
                None,
            }

            [SerializeField]
            private TypeHandleStyle handleStyle;
            public TypeHandleStyle HandleStyle
            {
                get
                {
                    return handleStyle;
                }
                set
                {
                    handleStyle = value;

                    if ( handleStyle == TypeHandleStyle.None )
                    {
                        InHandle = Vector3.zero;
                        OutHandle = Vector3.zero;
                    }
                }
            }

            public AnchorPoint()
            {
                guid = System.Guid.NewGuid().ToString( "N" );
            }
            public AnchorPoint( Vector3 p0 )
            {
                guid = System.Guid.NewGuid().ToString( "N" );
                position = p0;
            }

            [SerializeField]
            private string guid;
            public string Guid
            {
                get { return guid; }
            }

            public Vector3 position;

            [SerializeField]
            private Vector3 inHandle;
            [SerializeField]
            private Vector3 outHandle;

            public Vector3 InHandle
            {
                get
                {
                    return inHandle;
                }
                set
                {
                    if ( inHandle == value )
                    {
                        return;
                    }

                    inHandle = value;

                    if ( handleStyle == TypeHandleStyle.FreeSmooth )
                    {
                        outHandle = -inHandle;
                    }
                }
            }

            public Vector3 OutHandle
            {
                get
                {
                    return outHandle;
                }
                set
                {
                    if ( outHandle == value )
                    {
                        return;
                    }

                    outHandle = value;

                    if ( handleStyle == TypeHandleStyle.FreeSmooth )
                    {
                        inHandle = -outHandle;
                    }
                }
            }

        }
    }

}
