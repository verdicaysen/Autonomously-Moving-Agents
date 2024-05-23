using UnityEngine.AI;
using UnityEngine;

public class Bot : MonoBehaviour
{
    NavMeshAgent agent;
    public GameObject target;
    Drive driveScript;
    Vector3 wanderTarget = Vector3.zero;

    // Start is called before the first frame update
    void Start()
    {
        agent = this.GetComponent<NavMeshAgent>();
    }

     // Update is called once per frame
    void Update()
    {   
        Hide();
    }

    void Seek(Vector3 location)
    {
        agent.SetDestination(location);    
    }

   
    void Pursue()
    {
        Vector3 targetDir = agent.transform.position - transform.position;
        float relativeHeading = Vector3.Angle(this.transform.forward, this.transform.TransformVector(target.transform.forward));
        float toTarget = Vector3.Angle(this.transform.forward, this.transform.TransformVector(targetDir));

        if(toTarget > 90 && relativeHeading < 20 ||driveScript.currentSpeed < 0.01f)
        {
            Seek(target.transform.position);
            return;
        }

        float lookhead = targetDir.magnitude/(agent.speed + driveScript.currentSpeed);
        Seek(target.transform.position + target.transform.forward*lookhead);
    }

    void Evade()
    {
        Vector3 targetDir = agent.transform.position - transform.position;
        float lookhead = targetDir.magnitude/(agent.speed + driveScript.currentSpeed);
        Flee(target.transform.position + target.transform.forward*lookhead);
    }

    void Flee(Vector3 location)
    {
        Vector3 fleeVector = location - this.transform.position;
        agent.SetDestination(this.transform.position - fleeVector);
    }

    void Hide()
    {
        float distance = Mathf.Infinity;
        Vector3 chosenSpot = Vector3.zero;

        for(int i = 0; i < World.Instance.GetHidingSpots().Length; i++)
        {
            Vector3 hideDirection = World.Instance.GetHidingSpots()[i].transform.position - target.transform.position;
            Vector3 hidePosition = World.Instance.GetHidingSpots()[i].transform.position + hideDirection.normalized * 10;

            if(Vector3.Distance(this.transform.position, hidePosition) < distance)
            {
                chosenSpot = hidePosition;
                distance = Vector3.Distance(this.transform.position, hidePosition);
            }
        }

        Seek(chosenSpot);
    }

    void Wander()
    {
        float wanderRadius = 10;
        float wanderDistance = 20;
        float wanderJitter = 1;

        wanderTarget += new Vector3(Random.Range(-1.0f, 1.0f)* wanderJitter, 0, Random.Range(-1.0f, 1.0f)* wanderJitter);
        wanderTarget.Normalize();
        wanderTarget *= wanderRadius;

        Vector3 targetLocal = wanderTarget + new Vector3(0,0, wanderDistance);
        Vector3 targetWorld = this.gameObject.transform.InverseTransformVector(targetLocal);

        Seek(targetWorld);
    }   
}
