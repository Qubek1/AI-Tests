using System.Collections.Generic;
using UnityEngine;

public class SwarmController : MonoBehaviour
{
    public GameObject entityPrefab;
    public int entityAmount;
    public Vector3 RandomPosition;
    public ComputeShader swarmEntityCS;

    public float speed;
    public float seperationForce;
    public float aligmentForce;
    public float cohesionForce;
    public float targetForce;
    public float rotationRestriction;
    public float maxSquaredDistance;

    public Transform target;

    public List<Transform> entities;

    EntityData[] entityData;

    ComputeBuffer entityDataBuffer;

    private void Start()
    {
        entityData = new EntityData[entityAmount];
        for (int i = 0; i < entityAmount; i++)
        {
            Vector3 newRandomPosition = new Vector3(
                Random.Range(0, RandomPosition.x), 
                Random.Range(0, RandomPosition.y),
                Random.Range(0, RandomPosition.z));
            entities.Add(Instantiate(entityPrefab, newRandomPosition, new Quaternion(0, 0, 0, 0), transform).transform);
            entityData[i] = new EntityData(entities[i].position, new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)));
        }
        entityDataBuffer = new ComputeBuffer(entityAmount, sizeof(float) * 4);
    }

    private void Update()
    {
        for (int i = 0; i < entityAmount; i++)
        {
            entityData[i].position = entities[i].position;
        }
        entityDataBuffer.SetData(entityData);
        swarmEntityCS.SetBuffer(0, "entities", entityDataBuffer);

        swarmEntityCS.SetInt("entityAmount", entityAmount);
        swarmEntityCS.SetFloat("seperationForce", seperationForce);
        swarmEntityCS.SetFloat("aligmentForce", aligmentForce);
        swarmEntityCS.SetFloat("cohesionForce", cohesionForce);
        swarmEntityCS.SetFloat("targetForce", targetForce);
        swarmEntityCS.SetFloat("rotationRestriction", rotationRestriction);
        swarmEntityCS.SetFloat("maxSquaredDistance", maxSquaredDistance);
        swarmEntityCS.SetFloats("target", target.position.x, target.position.y);

        swarmEntityCS.Dispatch(0, (entityAmount + 63) / 64, 1, 1);

        entityDataBuffer.GetData(entityData);

        for (int i = 0; i < entityAmount; i++)
        {
            entities[i].position += new Vector3(entityData[i].direction.x, entityData[i].direction.y) * speed * Time.deltaTime;
            entities[i].rotation = Quaternion.LookRotation(Vector3.forward, entityData[i].direction);

            entities[i].position = new Vector3(
                Mathf.Repeat(entities[i].position.x, RandomPosition.x), 
                Mathf.Repeat(entities[i].position.y, RandomPosition.y));
        }
        //entityDataBuffer.Release();
    }
}

struct EntityData
{
    public Vector2 position;
    public Vector2 direction;
    public EntityData(Vector2 position, Vector2 direction)
    {
        this.position = position;
        this.direction = direction;
    }
}