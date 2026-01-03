using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine;

public class PlayerAgent : Agent
{
    [SerializeField] private Transform groundTransform;
[SerializeField] private RobotRLAgent robotRLAgent; 
private Rigidbody rb;
    private float rewardTimePenalty = -0.001f; 

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (robotRLAgent == null)
        {
            robotRLAgent = GetComponent<RobotRLAgent>();
            Debug.LogWarning("未手动赋值RobotRLAgent，自动获取组件");
        }
        robotRLAgent.MaxStep = 1000;
    }

    public override void OnEpisodeBegin()
    {
        transform.position = new Vector3(Random.Range(-4f, 4f), 0.5f, Random.Range(-4f, 4f));
        transform.rotation = Quaternion.identity;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        robotRLAgent.ResetRobotState();

        SpawnObjects("Treasure", 5);
        SpawnObjects("Trap", 5);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.position);
        sensor.AddObservation(rb.velocity);
        sensor.AddObservation(FindNearestObject("Treasure").position - transform.position);
        sensor.AddObservation(FindNearestObject("Trap").position - transform.position);

        float[] robotObs = robotRLAgent.GetRobotObservations();
        foreach (float obs in robotObs)
        {
            sensor.AddObservation(obs);
        }
    }

   public override void OnActionReceived(ActionBuffers actions)
    {
        robotRLAgent.ExecuteLegwheeledAction(actions);
AddReward(rewardTimePenalty);
    }

   public override void Heuristic(in ActionBuffers actionsOut)
    {
        robotRLAgent.HeuristicControl(actionsOut);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Treasure"))
        {
            AddReward(1f); 
            Destroy(collision.gameObject);
        }
        if (collision.gameObject.CompareTag("Trap"))
        {
            AddReward(-0.5f); 
            EndEpisode(); 
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("GroundEdge"))
        {
            AddReward(-1f); 
            EndEpisode();
        }
    }

    private void SpawnObjects(string objName, int count)
    {
        foreach (var obj in GameObject.FindGameObjectsWithTag(objName))
            Destroy(obj);

        for (int i = 0; i < count; i++)
        {
            Vector3 spawnPos = new Vector3(
                Random.Range(-4.5f, 4.5f), 0.25f, Random.Range(-4.5f, 4.5f)
            );
            GameObject obj = Instantiate(Resources.Load<GameObject>(objName), spawnPos, Quaternion.identity);
            obj.tag = objName;
        }
    }

    private Transform FindNearestObject(string tag)
    {
        var objects = GameObject.FindGameObjectsWithTag(tag);
        if (objects.Length == 0) return transform; 
        Transform nearest = objects[0].transform;
        foreach (var obj in objects)
        {
            if (Vector3.Distance(obj.transform.position, transform.position) < 
                Vector3.Distance(nearest.position, transform.position))
                nearest = obj.transform;
        }
        return nearest;
    }
}
