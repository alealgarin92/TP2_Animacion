using UnityEngine;

public class ProjectileLauncher : MonoBehaviour
{
    public Transform LaunchPoint;
    public GameObject projectilePrefap;

    public void FireProjectile()
    {
        Instantiate(projectilePrefap, LaunchPoint.position, projectilePrefap.transform.rotation);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
