using UnityEngine;

public class ProjectileLauncher : MonoBehaviour
{
    public Transform LaunchPoint;
    public GameObject projectilePrefap;

    public void FireProjectile()
    {
        GameObject projectile = Instantiate(projectilePrefap, LaunchPoint.position, projectilePrefap.transform.rotation);
        
        // Match scale of the launcher to flip the projectile direction correctly when facing left
        Vector3 origScale = projectile.transform.localScale;
        projectile.transform.localScale = new Vector3(
            origScale.x * Mathf.Sign(transform.lossyScale.x),
            origScale.y,
            origScale.z
        );

        // Assign owner to prevent self-collision
        Projectile proj = projectile.GetComponent<Projectile>();
        if (proj != null)
        {
            proj.owner = gameObject;
        }
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
