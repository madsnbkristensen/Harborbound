using UnityEngine;
using UnityEngine.Pool;
using System.Collections.Generic;

public class BulletPool : MonoBehaviour
{
    [Header("Default Bullet Settings")]
    public GameObject defaultBulletPrefab; // Fallback bullet prefab
    public static BulletPool Instance;

    [Header("Rendering Settings")]
    [SerializeField]
    private string bulletSortingLayer = "TopMost"; // Create this layer in Unity

    [SerializeField]
    private int bulletSortingOrder = 100;

    // Dictionary to store pools for different bullet types
    private Dictionary<GameObject, ObjectPool<GameObject>> bulletPools = new Dictionary<GameObject, ObjectPool<GameObject>>();

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // Create the default bullet pool
        if (defaultBulletPrefab != null)
        {
            CreatePoolForBulletType(defaultBulletPrefab);
        }
    }

    // Create a new pool for a specific bullet type
    private void CreatePoolForBulletType(GameObject bulletPrefab)
    {
        if (bulletPools.ContainsKey(bulletPrefab))
            return; // Pool already exists

        var pool = new ObjectPool<GameObject>(
            createFunc: () => CreateBullet(bulletPrefab),
            actionOnGet: OnBulletGet,
            actionOnRelease: OnBulletRelease,
            actionOnDestroy: OnBulletDestroy,
            collectionCheck: false,
            defaultCapacity: 10, // Smaller default capacity per bullet type
            maxSize: 50          // Smaller max size per bullet type
        );

        bulletPools[bulletPrefab] = pool;
        Debug.Log($"Created bullet pool for: {bulletPrefab.name}");
    }

    private GameObject CreateBullet(GameObject bulletPrefab)
    {
        GameObject bullet = Instantiate(bulletPrefab);
        bullet.transform.SetParent(transform);

        // Set the bullet's sprite to always render on top
        SpriteRenderer spriteRenderer = bullet.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingLayerName = bulletSortingLayer;
            spriteRenderer.sortingOrder = bulletSortingOrder;
        }

        return bullet;
    }

    private void OnBulletGet(GameObject bullet)
    {
        bullet.SetActive(true);

        // Ensure sorting settings are maintained when reused
        SpriteRenderer spriteRenderer = bullet.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingLayerName = bulletSortingLayer;
            spriteRenderer.sortingOrder = bulletSortingOrder;
        }
    }

    private void OnBulletRelease(GameObject bullet)
    {
        bullet.SetActive(false);
    }

    private void OnBulletDestroy(GameObject bullet)
    {
        Destroy(bullet);
    }

    // Get a bullet of a specific type
    public GameObject GetBullet(GameObject bulletPrefab = null)
    {
        // Use default bullet if no specific type is requested
        if (bulletPrefab == null)
        {
            bulletPrefab = defaultBulletPrefab;
        }

        // If we don't have this bullet prefab, use default
        if (bulletPrefab == null)
        {
            Debug.LogError("No bullet prefab specified and no default bullet prefab set!");
            return null;
        }

        // Create pool for this bullet type if it doesn't exist
        if (!bulletPools.ContainsKey(bulletPrefab))
        {
            CreatePoolForBulletType(bulletPrefab);
        }

        return bulletPools[bulletPrefab].Get();
    }

    // Return a bullet to the appropriate pool
    public void ReturnBullet(GameObject bullet)
    {
        if (bullet == null) return;

        // Find which pool this bullet belongs to
        foreach (var kvp in bulletPools)
        {
            // Check if this bullet was created from this prefab
            // We can do this by comparing the bullet's name with the prefab name
            string bulletName = bullet.name.Replace("(Clone)", "").Trim();
            string prefabName = kvp.Key.name;

            if (bulletName == prefabName)
            {
                kvp.Value.Release(bullet);
                return;
            }
        }

        // If we can't find the right pool, destroy the bullet
        Debug.LogWarning($"Could not find pool for bullet: {bullet.name}. Destroying instead.");
        Destroy(bullet);
    }

    // Utility method to get bullet prefab from weapon
    public GameObject GetBulletPrefabFromWeapon(Weapon weapon)
    {
        if (weapon?.definition?.bulletPrefab != null)
        {
            return weapon.definition.bulletPrefab;
        }

        return defaultBulletPrefab;
    }

    // Clean up all pools
    private void OnDestroy()
    {
        bulletPools.Clear();
    }
}
