using UnityEngine;
using UnityEngine.Pool;

public class BulletPool : MonoBehaviour
{
    public GameObject bulletPrefab;
    public static BulletPool Instance;

    [Header("Rendering Settings")]
    [SerializeField]
    private string bulletSortingLayer = "TopMost"; // Create this layer in Unity

    [SerializeField]
    private int bulletSortingOrder = 100;

    private ObjectPool<GameObject> pool;

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

        pool = new ObjectPool<GameObject>(
            createFunc: CreateBullet,
            actionOnGet: OnBulletGet,
            actionOnRelease: OnBulletRelease,
            actionOnDestroy: OnBulletDestroy,
            collectionCheck: false,
            defaultCapacity: 20,
            maxSize: 100
        );
    }

    private GameObject CreateBullet()
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

    public GameObject GetBullet()
    {
        return pool.Get();
    }

    public void ReturnBullet(GameObject bullet)
    {
        pool.Release(bullet);
    }
}
