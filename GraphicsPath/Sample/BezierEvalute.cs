using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BezierEvalute : MonoBehaviour
{

    private GraphicsPath.BezierCurve bezier;

    [SerializeField]
    private GraphicsPath.BezierCurveMaker bezierCurveMaker;

    public GameObject Target;

    [SerializeField]
    private float travelTime = 1.0f;

    public void Awake()
    {
        bezier = bezierCurveMaker.Bezier;
    }

    public void Update()
    {

        float t = Mathf.Repeat( Time.realtimeSinceStartup % travelTime, travelTime ) / travelTime;

        Target.transform.position = bezier.Evaluate( t );

    }


}
