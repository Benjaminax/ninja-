using UnityEngine;
using UnityEngine.Serialization;

public class ZombieSpawner : MonoBehaviour
{
    [SerializeField, FormerlySerializedAs("zombiePrefab")] GameObject skeletonPrefab;
    [SerializeField] float firstSpawnDelay = 5f;
    [SerializeField] float waveInterval = 12f;
    [SerializeField] float spawnDistance = 8f;
    [SerializeField] int maximumPerWave = 12;
    [SerializeField] int firstWaveCount = 8;
    [SerializeField] bool spawnOnlyOnce = true;
    [SerializeField] LayerMask groundLayer;

    float timer;
    int wave;
    Vector3 spawnOrigin;
    float platformHeight;
    bool hasPlatformHeight;

    public void Configure(GameObject prefab)
    {
        skeletonPrefab = prefab;
        groundLayer = LayerMask.GetMask("Ground");
        spawnOrigin = GetPlayerSpawnPosition();
        CachePlatformHeight();
        timer = Mathf.Max(0.1f, firstSpawnDelay);
    }

    void Start()
    {
        if (groundLayer.value == 0)
            groundLayer = LayerMask.GetMask("Ground");
        spawnOrigin = GetPlayerSpawnPosition();
        CachePlatformHeight();
        timer = Mathf.Max(0.1f, firstSpawnDelay);

        GameObject placedSkeleton = GameObject.Find("Skeleton");
        if (placedSkeleton != null && placedSkeleton.GetComponent<ChasingZombie>() == null)
            placedSkeleton.AddComponent<ChasingZombie>();
    }

    void Update()
    {
        if (skeletonPrefab == null)
            return;

        timer -= Time.deltaTime;
        if (timer > 0f)
            return;

        wave++;
        int count = wave == 1 ? Mathf.Min(firstWaveCount, maximumPerWave) : Mathf.Min(1 << (wave - 1), maximumPerWave);
        for (int i = 0; i < count; i++)
        {
            Vector3 candidate = spawnOrigin + GetSpawnOffset(i);
            if (!TryFindWalkablePosition(candidate, out Vector3 position))
                continue;

            GameObject enemy = Instantiate(skeletonPrefab, position, Quaternion.identity, transform.parent);
            if (enemy.GetComponent<ChasingZombie>() == null)
                enemy.AddComponent<ChasingZombie>();
        }

        if (spawnOnlyOnce)
        {
            enabled = false;
            return;
        }

        timer = waveInterval;
    }

    Vector3 GetSpawnOffset(int index)
    {
    Vector3[] positions =
    {
        new Vector3(-5f, 0f), new Vector3(-4f, 0f), new Vector3(-3f, 0f),
        new Vector3(-2f, 0f), new Vector3(2f, 0f), new Vector3(3f, 0f),
        new Vector3(4f, 0f), new Vector3(5f, 0f)
    };
    return index < positions.Length ? positions[index] : Vector3.right * (5f + (index - positions.Length + 1) * 2f);
    }

    bool TryFindWalkablePosition(Vector3 candidate, out Vector3 position)
    {
        position = candidate;
        Collider2D prefabCollider = skeletonPrefab.GetComponentInChildren<Collider2D>();
        float colliderBottom = prefabCollider != null ? prefabCollider.bounds.min.y : -0.5f;

        RaycastHit2D groundHit = Physics2D.Raycast(
            new Vector2(candidate.x, candidate.y + 50f),
            Vector2.down,
            100f,
            groundLayer);
        if (groundHit.collider == null || groundHit.normal.y < 0.5f)
            return false;
        if (hasPlatformHeight && Mathf.Abs(groundHit.point.y - platformHeight) > 0.3f)
            return false;

        position = new Vector3(
            candidate.x,
            groundHit.point.y - colliderBottom + 0.15f,
            candidate.z);
        return true;
    }

    void CachePlatformHeight()
    {
        if (groundLayer.value == 0)
            return;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Collider2D playerCollider = player.GetComponent<Collider2D>();
            if (playerCollider != null)
            {
                RaycastHit2D playerGround = Physics2D.Raycast(
                    new Vector2(playerCollider.bounds.center.x, playerCollider.bounds.min.y + 0.2f),
                    Vector2.down,
                    1f,
                    groundLayer);
                if (playerGround.collider != null && playerGround.normal.y >= 0.5f)
                {
                    platformHeight = playerGround.point.y;
                    hasPlatformHeight = true;
                    return;
                }
            }
        }

        RaycastHit2D groundHit = Physics2D.Raycast(
            new Vector2(spawnOrigin.x, spawnOrigin.y + 50f),
            Vector2.down,
            100f,
            groundLayer);
        if (groundHit.collider != null && groundHit.normal.y >= 0.5f)
        {
            platformHeight = groundHit.point.y;
            hasPlatformHeight = true;
        }
    }

    Vector3 GetPlayerSpawnPosition()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        return player != null ? player.transform.position : transform.position;
    }
}
