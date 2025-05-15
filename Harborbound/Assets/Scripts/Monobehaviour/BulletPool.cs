using UnityEngine;
using UnityEngine.Pool;

public class BulletPool : MonoBehaviour
{
    public GameObject bulletPrefab;
    public static BulletPool Instance;

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
        return bullet;
    }

    private void OnBulletGet(GameObject bullet)
    {
        bullet.SetActive(true);
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

