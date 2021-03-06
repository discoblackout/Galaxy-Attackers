using UnityEngine;
using System.Collections;

[RequireComponent( typeof(VoxelAnimation) )]
public class AlienBullet : MonoBehaviour {
	
	/// <summary>
	/// The velocity of the bullet.
	/// </summary>
	public Vector3 velocity = Vector3.zero;
	
	/// <summary>
	/// Collision point offset.
	/// </summary>
	public Vector3 hitOffset = Vector3.zero;
	
	/// <summary>
	/// Force of the bullet collision explosion.
	/// </summary>
	public float explosionForce = 10000.0f;
	
	/// <summary>
	/// Radius of the bullet collision explosion.
	/// </summary>
	public float explosionRadius = 20.0f;

	/// <summary>
	/// Reference to the debris voxel model.
	/// </summary>
	public Transform debris;

	private VoxelAnimation voxelAnimation;

	private BoxCollider boxCollider;
	
	// Use this for initialization
	void Start () {
		boxCollider = GetComponent<BoxCollider>();

		voxelAnimation = GetComponent<VoxelAnimation>();
		voxelAnimation.OnFrameChange += LoadBounds;
		
		// If the first frame hasn't loaded yet, listen for it
		if (voxelAnimation.CurrentFrame.Loaded != true)
		{
			voxelAnimation.CurrentFrame.OnLoad += LoadBounds;
		}
		else
		{
			LoadBounds(voxelAnimation.CurrentFrame);
		}
	}

	/// <summary>
	/// Sets the box collider bounds to that of the current frame of the voxel animation.
	/// </summary>
	/// <param name="animation">Animation.</param>
	void LoadBounds(VoxelAnimation animation)
	{
		LoadBounds(animation.CurrentFrame);
	}

	/// <summary>
	/// Sets the box collider bounds to that of the voxel model.
	/// </summary>
	/// <param name="animation">Animation.</param>
	void LoadBounds(VoxelModel model)
	{
		Bounds bounds = model.GetLocalBounds();
		boxCollider.center = bounds.center;
		boxCollider.size = bounds.size;
	}
	
	// Update bounds on frame change
	void voxelAnimation_OnFrameChange(VoxelAnimation animation)
	{
		Bounds bounds = animation.CurrentFrame.GetLocalBounds();
		boxCollider.center = bounds.center;
		boxCollider.size = bounds.size;
	}
	
	/// <summary>
	/// Draws the bullet's collision point gizmo.
	/// </summary>
	void OnDrawGizmos()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(transform.position + hitOffset, 1.0f);
	}
	
	void Update()
	{
		// Move at a fixed velocity
		transform.position += velocity * Time.deltaTime;
	}
	
	/// <summary>
	/// Bullet collision handler.
	/// </summary>
	/// <param name="other">Collider colliding with.</param>
	void Explode(Collider other)
	{		
		if (other.tag == "Player")
		{
            // Potential player hit
			Player player = other.GetComponent<Player>();
			
			Vector3 hitPoint = transform.position + hitOffset;
						
			if (player.CheckCollision(hitPoint))
			{
                // Definite hit
				player.ExplodeAt(hitPoint, explosionForce, explosionRadius);				
				
				// Kill thyself
				Destroy(gameObject);
			}
		}
		else if (other.tag == "Building")
		{
            // Potential building hit
			Building building = other.GetComponent<Building>();
			
			Vector3 hitPoint = transform.position + hitOffset;
						
			if (building.CheckCollision(hitPoint))
			{
                // Definite hit
				building.ExplodeAt(hitPoint, explosionForce, explosionRadius);
				
				// Kill thyself
				Destroy(gameObject);
			}
		}
	}
	
	void OnTriggerEnter(Collider other)
	{
		Explode(other);
	}
	
	void OnTriggerStay(Collider other)
	{
		Explode(other);
	}

	/// <summary>
	/// Check for collision between a point and the bullet.
	/// </summary>
	/// <param name="position">Point in world coordinates.</param>
	/// <returns>True on collision, false otherwise.</returns>
	public bool CheckCollision(Vector3 position)
	{
		Vector3 localPos = voxelAnimation.CurrentFrame.transform.InverseTransformPoint(position);
		return voxelAnimation.CurrentFrame.GetVoxel(localPos) > 0;
	}

	/// <summary>
	/// Explodes the bullet into debris by a force at a given location.
	/// </summary>
	/// <param name="position">Location of the explosion force.</param>
	/// <param name="force">Magnitude of the explosion force.</param>
	/// <param name="radius">Radius of the explosion force.</param>
	public void ExplodeAt(Vector3 position, float force, float radius)
	{
		VoxelModel vm = voxelAnimation.CurrentFrame;
		
		foreach (Vector3 point in vm.ToLocalPoints())
		{
			GameObject go = Instantiate(debris.gameObject, vm.transform.TransformPoint(point), Quaternion.identity) as GameObject;
			go.rigidbody.AddExplosionForce(force, position, radius);
		}
		
		Destroy(gameObject);
	}
}
