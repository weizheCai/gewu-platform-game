using UnityEngine;

public class GroundEdge : MonoBehaviour
{
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("PlayerAgent"))
        {
            other.GetComponent<PlayerAgent>().AddReward(-1f);
            other.GetComponent<PlayerAgent>().EndEpisode();
        }
    }
}
