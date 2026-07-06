using System.Collections.Generic;
using UnityEngine;

public class checkfordoornum : MonoBehaviour
{
    [SerializeField] private TrailRenderer tr;
    private door dr;
    private int tpto = 1;
    public List<door> doors;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        tr.enabled = false;
        tpto = door.currentID;
        foreach (door dr in doors)
        {
            if (dr.doorID == tpto)
            {
                transform.position = dr.transform.position;
                tr.enabled = true;

                UnityEngine.Debug.Log("done");
            }
        }

        tr.enabled = true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
