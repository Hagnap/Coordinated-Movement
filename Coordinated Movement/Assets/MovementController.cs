using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementController : MonoBehaviour
{
    #region Variables
    [SerializeField] private Camera cam;

    [SerializeField] private GameObject agent;
    [SerializeField] private GameObject target;

    private Rigidbody rb;

    private float rotationSpeed;

    private float speed;
    private float maxSpeed;
    private float radiusOfSatisfaction;

    private bool move;
    private bool targetExist;
    private bool targetReached;
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        // Sets values
        move = false;
        targetExist = false;
        speed = 2.5f;
        maxSpeed = 5f;
        radiusOfSatisfaction = 1f;
        rb = agent.GetComponent<Rigidbody>();
        rotationSpeed = 2.5f;
    }

    // Update is called once per frame
    void Update()
    {
        // If the player clicks and the target does not exist
        if (Input.GetMouseButtonDown(0) && !targetExist)
        {
            move = true;
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                // Creates target
                Instantiate(target, hit.point + new Vector3(0f, 0.5f, 0f), Quaternion.identity);
                targetExist = true;
            }
        }

        // If the player left clicks and a target does exist
        if (Input.GetMouseButtonDown(0) && targetExist)
        {
            move = true;
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                target.transform.position = hit.point + new Vector3(0f, 0.5f, 0f);
                targetReached = false;
            }
        }

        // If the player moves → Run the Kinematic Arrival Algorithm
        if (move)
        {
            RunKinematicArrival();
        }
        else
        {
            if (!targetReached)
                Debug.Log("Agent not moving....");
        }
    }

    void RunKinematicArrival()
    {
        ///Debug.Log("Agent moving....");

        //Calcutlates the direction the agent needs to rotate to face towards the target
        Vector3 towards = target.transform.position - agent.transform.position;

        //Rotates the agent using towards
        //agent.transform.rotation = Quaternion.Slerp(agent.transform.rotation, target.transform.rotation, speed * Time.deltaTime);
        Quaternion rotation = Quaternion.LookRotation(towards);
        transform.rotation = Quaternion.Lerp(transform.rotation, rotation, rotationSpeed * Time.deltaTime);

        if (towards.magnitude > radiusOfSatisfaction)
        {
            // Displays the distance remaining to the target
            Debug.Log("Towards Magnitude: " + towards.magnitude);

            //Normalizes the Vector3 to get the direction
            towards.Normalize();
            towards *= speed;

            //Moves agent
            rb.velocity = towards;
        }
        else
        {
            //Stops the agent -- Had to put this in bc the agent kept sliding towards the target after it stopped moving bc its a rb, this helps with it somewhat
            rb.velocity = Vector3.zero;
            
            ///Debug.Log("Destination has been reached...");
            move = false;
            targetReached = true;
        }

        // If the character is moving too fast...
        if (rb.velocity.magnitude > speed)
        {
            rb.velocity = rb.velocity.normalized;
            rb.velocity *= speed;
        }
    }
}
