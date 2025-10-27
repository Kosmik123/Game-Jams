using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public float spawnInterval = 2f;

    public Transform container;

    public Transform leftSpawnPoint;
        public Transform rightSpawnPoint;

    private void Start()
    {
        StartCoroutine(SpawnEnemies());
    }

    private IEnumerator SpawnEnemies()
    {
        while (true)
        {
            SpawnEnemy();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void SpawnEnemy()
    {
        spawnInterval *= 0.99f;
        float spanwX = Random.Range(leftSpawnPoint.position.x, rightSpawnPoint.position.x); 
        float spanwY = Random.Range(leftSpawnPoint.position.y, rightSpawnPoint.position.y); 
        Instantiate(enemyPrefab, new Vector3(spanwX, spanwY), Quaternion.identity, container);
    }
}
