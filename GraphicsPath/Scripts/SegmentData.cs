using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GraphicsPath
{

    public class SegmentData : ScriptableObject
    {
        public bool IsClosed;
        public List< Vector3 > Points = new List<Vector3>();
        public List< float > Distances = new List<float>();

        public float TotalLength
        {
            get
            {
                float total = 0.0f;

                foreach ( var value in Distances )
                {
                    total += value;
                }

                return total;
            }
        }
    }

}