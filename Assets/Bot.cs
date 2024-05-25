using UnityEngine.AI;
using UnityEngine;

public class Bot : MonoBehaviour
{
    NavMeshAgent agent;
    public GameObject target;
    Drive driveScript;
    Vector3 wanderTarget = Vector3.zero;
    bool coolDown = false;
    

    // Start is called before the first frame update
    void Start()
    {
        agent = this.GetComponent<NavMeshAgent>();
    }

     // In this update loop, test to see if you can see the target, and if the target can see you. If it can and you do, hide. 
     //Otherwise, pursue the target.
    void Update()
    {   
            if (!coolDown)
        {
            if(!TargetInRange())
            {
                Wander();
            }

            else if(CanSeeTarget() && CanSeeMe())
            {
                CleverHide();
                coolDown = true;
                Invoke("BehaviourCoolDown", 5);
            }
            else
            
                Pursue();
        }  
        
    }

    //Lets slow down the behaviour checks.
    void BehaviourCoolDown()
    {
        coolDown = false;
    }

    // Check if the target is within its range.
    bool TargetInRange()
    {
        if(Vector3.Distance(this.transform.position, target.transform.position) < 10)
           return true;
        return false;
    }

    void Seek(Vector3 location)
    {
        agent.SetDestination(location);    
    }

   //This is a chasing method that will take after the cop.
    void Pursue()
    {
        Vector3 targetDir = target.transform.position - transform.position;
        float relativeHeading = Vector3.Angle(this.transform.forward, this.transform.TransformVector(target.transform.forward));
        float toTarget = Vector3.Angle(this.transform.forward, this.transform.TransformVector(targetDir));

        if((toTarget > 90 && relativeHeading < 20) || target.GetComponent<Drive>().currentSpeed < 0.01f)
        {
            Seek(target.transform.position);
            return;
        }

        float lookhead = targetDir.magnitude/(agent.speed + target.GetComponent<Drive>().currentSpeed);
        Seek(target.transform.position + target.transform.forward*lookhead);
    }

    //This is a runaway method that will move away from your cops position but with a look ahead component.

    void Evade()
    {
        Vector3 targetDir = agent.transform.position - transform.position;
        float lookhead = targetDir.magnitude/(agent.speed + driveScript.currentSpeed);
        Flee(target.transform.position + target.transform.forward*lookhead);
    }

    //The basic movement away from the players location.
    void Flee(Vector3 location)
    {
        Vector3 fleeVector = location - this.transform.position;
        agent.SetDestination(this.transform.position - fleeVector);
    }

    //This is a method that will have the robber hide behind objects.

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

    //This is the robber staying closer to the navmesh and collider of an object and move around to get away.

    void CleverHide()
    {
        float distance = Mathf.Infinity;
        Vector3 chosenSpot = Vector3.zero;
        Vector3 chosenDirection = Vector3.zero;
        GameObject chosenGameObject = World.Instance.GetHidingSpots()[0];

        for(int i = 0; i < World.Instance.GetHidingSpots().Length; i++)
        {
            Vector3 hideDirection = World.Instance.GetHidingSpots()[i].transform.position - target.transform.position;
            Vector3 hidePosition = World.Instance.GetHidingSpots()[i].transform.position + hideDirection.normalized * 10;

            if(Vector3.Distance(this.transform.position, hidePosition) < distance)
            {
                chosenSpot = hidePosition;
                chosenDirection = hideDirection;
                chosenGameObject = World.Instance.GetHidingSpots()[i];
                distance = Vector3.Distance(this.transform.position, hidePosition);
            }
        }
        Collider hideCollider = chosenGameObject.GetComponent<Collider>();
        Ray backRay = new Ray(chosenSpot, -chosenDirection.normalized);
        RaycastHit info;
        float rayDistance = 100.0f;
        hideCollider.Raycast(backRay, out info, distance);

        Seek(info.point + chosenDirection.normalized * 5);

    }

    //Look around you and if you see the cop remember it.
    bool CanSeeTarget()
    {
        RaycastHit raycastInfo;
        Vector3 rayToTarget = target.transform.position - this.transform.position;
        float lookAngle = Vector3.Angle(this.transform.forward, rayToTarget);
        
        // If the look angle is less than 60 degrees you should be able to see the target.
        if(lookAngle < 60 && Physics.Raycast(this.transform.position, rayToTarget, out raycastInfo))
        {
            if(raycastInfo.transform.gameObject.tag == "cop")
            return true;
        }
        return false;
    }

    // Can my enemy see me?
    bool CanSeeMe()
    {
       
        Vector3 rayToTarget = this.transform.position - target.transform.position;
        float lookAngle = Vector3.Angle(target.transform.forward, rayToTarget);

        if(lookAngle < 60)
        {
            return true;
        }
        return false;
    }


    //This is a wandering method for letting the robber patrol around in random directions.
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
