using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurveTraceUpdater : MonoBehaviour
{
    private GraphicsPath.CurveTracer updater;

    // Start is called before the first frame update
    void Start()
    {
        updater = GetComponent<GraphicsPath.CurveTracer>();
        updater.Initialize();
    }

    // Update is called once per frame
    void Update()
    {
        updater.OnUpdate( Time.deltaTime );

    }
}
