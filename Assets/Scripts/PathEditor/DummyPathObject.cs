using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DummyPathObject : MonoBehaviour
{
    [SerializeField] private PathBehaviour pathBehaviour;
    [SerializeField] private float speed;
    [SerializeField] private int segmentIndex = 1;


    private PathMade pathMade;

    private void Start()
    {
        pathMade = new PathMade(transform, pathBehaviour.path);
        pathMade.SetStartPoint(segmentIndex,0);
    }

    private void Update() 
    {
        pathMade.ProgressPath(speed, Time.deltaTime);
    }
}