using UnityEngine;
using System.Collections.Generic;

public class SceneObjectInstaller : MonoBehaviour
{
    [Header("Planets")]
    public int planetCount = 4;
    public float planetHeightMin = 20f;
    public float planetHeightMax = 35f;
    public float planetRadius = 30f;

    [Header("Spaceships")]
    public int spaceshipCount = 4;
    public GameObject spaceshipPrefab; // assign manual

    [Header("Aliens")]
    public int alienCount = 10;
    public GameObject[] alienPrefabs; // assign array (10 prefabs)

    void Start()
    {
        // Periksa apakah objek sudah ada, jika tidak buat
        if (GameObject.Find("Planet_0") == null)
            CreatePlanets();

        if (GameObject.Find("Spaceship_0") == null)
            CreateSpaceships();

        if (GameObject.Find("Alien_1") == null)
            CreateAliens();
    }

    void CreatePlanets()
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");

        Color[] colors = { Color.red, Color.blue, Color.yellow, Color.green };
        for (int i = 0; i < planetCount; i++)
        {
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float radius = Random.Range(planetRadius * 0.5f, planetRadius);
            Vector3 pos = new Vector3(Mathf.Cos(angle) * radius, Random.Range(planetHeightMin, planetHeightMax), Mathf.Sin(angle) * radius);
            GameObject planet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            planet.name = "Planet_" + i;
            planet.transform.position = pos;
            planet.transform.localScale = Vector3.one * (4f + Random.Range(0f, 4f));
            Material mat = new Material(shader);
            mat.color = colors[i % colors.Length];
            mat.SetFloat("_Metallic", 0.8f);
            mat.SetFloat("_Smoothness", 0.9f);
            planet.GetComponent<Renderer>().material = mat;
            Destroy(planet.GetComponent<Collider>());
            var floating = planet.AddComponent<FloatingObject>();
            floating.floatSpeed = Random.Range(0.2f, 0.5f);
            floating.floatAmplitude = Random.Range(0.2f, 0.8f);
            floating.rotationSpeed = Random.Range(3f, 10f);
        }
    }

    void CreateSpaceships()
    {
        if (spaceshipPrefab == null) return;
        for (int i = 0; i < spaceshipCount; i++)
        {
            Vector3 pos = new Vector3(Random.Range(-20f, 20f), Random.Range(15f, 25f), Random.Range(-20f, 20f));
            GameObject ship = Instantiate(spaceshipPrefab);
            ship.name = "Spaceship_" + i;
            ship.transform.position = pos;
            ship.transform.localScale = Vector3.one * 2f;
            var floating = ship.AddComponent<FloatingObject>();
            floating.floatSpeed = Random.Range(0.3f, 0.8f);
            floating.floatAmplitude = Random.Range(0.3f, 1f);
            floating.rotationSpeed = Random.Range(5f, 20f);
        }
    }

    void CreateAliens()
    {
        if (alienPrefabs == null || alienPrefabs.Length == 0) return;
        List<Vector3> positions = new List<Vector3>();
        for (int i = 0; i < alienCount; i++)
        {
            Vector3 pos = new Vector3(Random.Range(-20f, 20f), 0.5f, Random.Range(-20f, 20f));
            positions.Add(pos);
        }
        for (int i = 0; i < alienCount && i < alienPrefabs.Length; i++)
        {
            if (alienPrefabs[i] == null) continue;
            GameObject alien = Instantiate(alienPrefabs[i]);
            alien.name = "Alien_" + (i+1);
            alien.transform.position = positions[i];
            alien.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
            alien.transform.localScale = Vector3.one * Random.Range(0.8f, 1.5f);
            var floating = alien.AddComponent<FloatingObject>();
            floating.floatSpeed = Random.Range(0.3f, 0.7f);
            floating.floatAmplitude = Random.Range(0.2f, 0.6f);
            floating.rotationSpeed = Random.Range(2f, 8f);
        }
    }
}