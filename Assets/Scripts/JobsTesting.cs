using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Jobs;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;


public class JobsTesting : MonoBehaviour {

    [SerializeField] private bool useJobs;
    [SerializeField] private Transform entity;
    [SerializeField] private int numOfEntities;
    private List<Entity> entities = new List<Entity>();

    public class Entity {
        public Transform transform;
        public float moveY;
    }

    private void Start() {
        for (int i = 0; i < numOfEntities; i++) {
            Transform entityTransform = Instantiate(entity, new Vector3(UnityEngine.Random.Range(-8f, 8f), UnityEngine.Random.Range(-5f, 5f)), Quaternion.identity);
            entityTransform.gameObject.GetComponent<SpriteRenderer>().color = UnityEngine.Random.ColorHSV(0, 1, 1, 1);
            entities.Add(new Entity {
                transform = entityTransform,
                moveY = UnityEngine.Random.Range(1f, 3f)
            });
        }
    }

    private void Update() {
        float startTime = Time.realtimeSinceStartup;
        HandleJobs();
       // Debug.Log(((Time.realtimeSinceStartup - startTime) * 1000f) + "ms");

        if (Input.GetKeyDown(KeyCode.Space)) { useJobs = !useJobs; }
    }

    private void HandleJobs()
    {
        if (useJobs)
        {
            NativeArray<float> moveYArray = new NativeArray<float>(entities.Count, Allocator.TempJob);
            TransformAccessArray transformAccessArray = new TransformAccessArray(entities.Count);

            for (int i = 0; i < entities.Count; i++)
            {
                moveYArray[i] = entities[i].moveY;
                transformAccessArray.Add(entities[i].transform);
            }

            ReallyToughParallelJobTransforms reallyToughParallelJobTransforms = new ReallyToughParallelJobTransforms
            {
                deltaTime = Time.deltaTime,
                moveYArray = moveYArray,
            };

            JobHandle jobHandle = reallyToughParallelJobTransforms.Schedule(transformAccessArray);
            jobHandle.Complete();

            for (int i = 0; i < entities.Count; i++)
            {
                entities[i].moveY = moveYArray[i];
            }

            moveYArray.Dispose();
            transformAccessArray.Dispose();

        }
        else
        {
            foreach (Entity entity in entities)
            {
                entity.transform.position += new Vector3(0, entity.moveY * Time.deltaTime);
                if (entity.transform.position.y > 5f)
                {
                    entity.moveY = -math.abs(entity.moveY);
                }
                if (entity.transform.position.y < -5f)
                {
                    entity.moveY = +math.abs(entity.moveY);
                }
                float value = 0f;
                for (int i = 0; i < 1000; i++)
                {
                    value = math.exp10(math.sqrt(value));
                }
            }
        }
    }

    [BurstCompile]
    public struct ReallyToughParallelJobTransforms : IJobParallelForTransform
    {
        public NativeArray<float> moveYArray;
        public float deltaTime;
     
        public void Execute(int index, TransformAccess transform)
        {
            transform.position += new Vector3(0, moveYArray[index] * deltaTime, 0f);
            if (transform.position.y > 5f)
            {
                moveYArray[index] = -math.abs(moveYArray[index]);
            }
            if (transform.position.y < -5f)
            {
                moveYArray[index] = +math.abs(moveYArray[index]);
            }

            float value = 0f;
            for (int i = 0; i < 1000; i++)
            {
                value = math.exp10(math.sqrt(value));
            }
        }
    }
}
