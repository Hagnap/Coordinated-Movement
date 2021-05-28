using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FormationController : MonoBehaviour
{
    #region Variables
    // Obstacle Detection vars
    [SerializeField] private FormationManager formationManager;
    [SerializeField] private float avoidDistance;
    [SerializeField] private float lookAhead;
    [SerializeField] private float anchorOffset;

    // Eyes are used to spawn the whiskey raycasts (aka the two outside rayscasts)
    [SerializeField] private GameObject eye1;
    [SerializeField] private GameObject eye2;

    // Formation vars
    [SerializeField] private Transform leader;
    private CharacterController leaderController;
    [SerializeField] private Transform anchor;
    [SerializeField] private float distanceOffset;
    [SerializeField] private float rotationOffset;
    private Vector3 projectedPosition;
    private Vector3 formationPosition;
    private Vector3 lookOffset;
    private bool stayInFormation;
    [SerializeField] private int assignedNumber;

    // Kinematic Arrival vars
    private Rigidbody rb;
    private Vector3 towards;
    private float moveSpeed;
    private float turnSpeed;
    private bool isMoving;
    [SerializeField]private float radiusOfSatisfaction;
    #endregion

    #region Unity Methods
    // Start is called before the first frame update
    void Start()
    {
        isMoving = false;
        formationManager = anchor.GetComponentInParent<FormationManager>();
        stayInFormation = true;
        rb = GetComponent<Rigidbody>();
        moveSpeed = 4f;
        turnSpeed = 2.5f;
        leaderController = leader.GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {

        AvoidObstacles();

        if (stayInFormation)
        {
            CheckFormation();
            CoordinatedMovement();
        } 
        else
            GoToAnchor();
    }
    #endregion

    #region Custom Methods
    /// <summary>
    /// This is based off the examples found in the textbook and the example provided by Prof. Jugan. 
    /// This method establishes the Four-Finger Formation with the formation moving with respect to the 
    /// leader (the blue agent). 
    /// </summary>
    void CoordinatedMovement()
    {
        if(leaderController.GetGoToAnchor())
        {
            RunKinematicArrive(formationPosition);
        }
        else
        {
            // Predicts the forward position from the leader's front/forward vector
            projectedPosition = leader.forward * distanceOffset;

            // Rotates the position to find so that this bot can find its place in the formation
            formationPosition = Quaternion.Euler(0f, rotationOffset, 0f) * projectedPosition;

            formationPosition += leader.position;
            RunKinematicArrive(formationPosition);
        }
    }

    void CheckFormation()
    {
        if(leaderController.IsFourFingerFormation)
        {
            if(assignedNumber == 1)
            {
                distanceOffset = 1.5f;
                rotationOffset = -135f;
            }
            if(assignedNumber == 2)
            {
                distanceOffset = 1.5f;
                rotationOffset = 135f;
            }
            if(assignedNumber == 3)
            {
                distanceOffset = 3f;
                rotationOffset = 135f;
            }
        }
        
        else if(leaderController.IsWallFormation)
        {
            if (assignedNumber == 1)
            {
                distanceOffset = 1.25f;
                rotationOffset = -60f;
            }
            if (assignedNumber == 2)
            {
                distanceOffset = 1.25f;
                rotationOffset = 60f;
            }
            if (assignedNumber == 3)
            {
                distanceOffset = 2.0f;
                rotationOffset = 60;
            }
        }
        
         else if(leaderController.IsDefensiveFormation)
         {
            if (assignedNumber == 1)
            {
                distanceOffset = 2.5f;
                rotationOffset = -250f;

                lookOffset = new Vector3(leader.gameObject.transform.eulerAngles.x, leader.gameObject.transform.eulerAngles.y + 90f,
                        leader.gameObject.transform.eulerAngles.z);

                AlterRotation(lookOffset);
            }
            if (assignedNumber == 2)
            {
                distanceOffset = 2.5f;
                rotationOffset = 250f;

                lookOffset = new Vector3(leader.gameObject.transform.eulerAngles.x, leader.gameObject.transform.eulerAngles.y - 90f,
                        leader.gameObject.transform.eulerAngles.z);

                AlterRotation(lookOffset);
            }
            if (assignedNumber == 3)
            {
                distanceOffset = 3f;
                rotationOffset = -180;

                lookOffset = new Vector3(leader.gameObject.transform.eulerAngles.x, leader.gameObject.transform.eulerAngles.y - 180f,
                        leader.gameObject.transform.eulerAngles.z);

                AlterRotation(lookOffset);
            }
         }
        
        else if(leaderController.IsTwoAbreastFormation)
        {
           if (assignedNumber == 1)
           {
               distanceOffset = 0.75f;
               rotationOffset = -150f;

                lookOffset = new Vector3(leader.gameObject.transform.eulerAngles.x, leader.gameObject.transform.eulerAngles.y - 150f,
                        leader.gameObject.transform.eulerAngles.z);

                AlterRotation(lookOffset);
            }
           if (assignedNumber == 2)
           {
               distanceOffset = 1.5f;
               rotationOffset = 60f;
           }
           if (assignedNumber == 3)
           {
               distanceOffset = 1.5f;
               rotationOffset = 120;

                lookOffset = new Vector3(leader.gameObject.transform.eulerAngles.x, leader.gameObject.transform.eulerAngles.y + 135f,
                        leader.gameObject.transform.eulerAngles.z);

                AlterRotation(lookOffset);
            }
        }

        // Defualts to the last formation
        
        else
        {
            return;
        }
        
    }

    /// <summary>
    /// Based off the implementation given from the class textbook. It uses a rigidbody to move the agent.
    /// The radius of satisfaction is used to avoid the agent wiggling back and forth a specific spot.
    /// </summary>
    void RunKinematicArrive(Vector3 position)
    {
        // Checks to see if the bot has reached the desired destination
        if (Vector3.Distance(transform.position, position) > radiusOfSatisfaction)
        {
            isMoving = true;

            // Calculate vector to the position in the formation
            towards = position - transform.position;

            // Normalizes the vector and sets velocity
            towards.Normalize();
            towards *= moveSpeed;
            rb.velocity = towards;

            Debug.DrawLine(transform.position, position, Color.blue);


            // Face player along movement vector
            Quaternion targetRotation = Quaternion.LookRotation(towards);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        }
        else
        {
            isMoving = false;

            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }



    /// <summary>
    /// Based off the implementation found in the class textbook. Uses Raycasts to detect obstacles.
    /// Three obstacles are used in this implementation. One comes from the middle of the agent's body and the other two
    /// come from the agent's eyes, to act as whiskers. The whiskers are 3/4 the length of the raycast in the middle.
    /// </summary>
    void AvoidObstacles()
    {
        int layerMask = 8;

        // Inverts the bitmask -- To ignore every layer but layer 8
        layerMask = ~layerMask;

        Vector3 ray1 = transform.forward;
        ray1.Normalize();
        ray1 *= lookAhead;
        Vector3 ray2 = eye1.transform.forward;
        ray2.Normalize();
        ray2 *= lookAhead * 0.75f; // Whisker raycasts are shorter than the middle raycast
        Vector3 ray3 = eye2.transform.forward;
        ray3.Normalize();
        ray3 *= lookAhead * 0.75f; // Whisker raycasts are shorter than the middle raycast

        RaycastHit hit;

        // Draws the Rays
        Debug.DrawRay(transform.position, ray1, Color.red);
        Debug.DrawRay(transform.position, ray2, Color.red);
        Debug.DrawRay(transform.position, ray3, Color.red);

        // Finds the collision
        // Collision detected straigh ahead
        if (Physics.Raycast(transform.position, ray1, out hit, avoidDistance, layerMask))
        {
            if (hit.transform.gameObject.layer == 8)
            {
                Debug.Log("Obstacle detected ahead. Going to the Anchor");
                // Goes back and to the side
                formationManager.SetAnchorOffset(new Vector3(0f, 0f, -anchorOffset));
                stayInFormation = false;
            }
        }

        // Collision detected to the right
        if (Physics.Raycast(transform.position, ray2, out hit, avoidDistance, layerMask))
        {
            if (hit.transform.gameObject.layer == 8)
            {
                Debug.Log("Obstacle detected to the right. Going to the Anchor");
                // Goes to the right
                formationManager.SetAnchorOffset(new Vector3(-anchorOffset, 0f, 0f));
                stayInFormation = false;
            }
        }

        // Collision detected to the left
        if (Physics.Raycast(transform.position, ray3, out hit, avoidDistance, layerMask))
        {
            if (hit.transform.gameObject.layer == 8)
            {
                Debug.Log("Obstacle detected to the left. Going to the Anchor");
                // Goes to the left
                formationManager.SetAnchorOffset(new Vector3(anchorOffset, 0f, 0f));
                stayInFormation = false;
            }
        }
    }

    /// <summary>
    /// This implementation is based off the idea given in the textbook. It uses the Center of Mass between the four agents
    /// to create a position for the agents to go to avoid a collision; after getting close enough to the anchor the agent goes back
    /// into the formation. This helps the agents get around obstacles. The Center of Mass implementation is found in the FormationManager Class.
    /// </summary>
    void GoToAnchor()
    {
        ///Debug.Log("Follower: Collision, going to the anchor to reset and then going back into the formation...");

        Vector3 position = anchor.position;

        // Checks to see if the bot has reached the desired destination
        if (Vector3.Distance(transform.position, position) > radiusOfSatisfaction)
        {
            // Calculate vector to the position in the formation
            towards = position - transform.position;

            // Normalizes the vector and sets velocity
            towards.Normalize();
            towards *= moveSpeed;
            rb.velocity = towards;

            Debug.DrawLine(transform.position, position, Color.blue);


            // Face player along movement vector
            Quaternion targetRotation = Quaternion.LookRotation(towards);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        }
        else
        {
            //rb.velocity = Vector3.zero;
            //rb.angularVelocity = Vector3.zero;

            // The agent has reached the CoM, goes back to formation
            stayInFormation = true;
            formationManager.SetAnchorOffset(Vector3.zero);
        }
    }

    void AlterRotation(Vector3 offset)
    {
        if (!isMoving)
        {
            // Rotates the agent
            //Quaternion targetRotation = Quaternion.LookRotation(towards);
            Quaternion targetRotation = Quaternion.Euler(offset);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        }
    }
    #endregion
}
