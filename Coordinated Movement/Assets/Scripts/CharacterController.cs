using UnityEngine;

public class CharacterController : MonoBehaviour
{
    #region Variables
    // Obstacle Avoidance vars
    // Eyes are used to spawn the whiskey raycasts (aka the two outside rayscasts)
    [SerializeField] private GameObject eye1;
    [SerializeField] private GameObject eye2;
    [SerializeField] private float avoidDistance;
    [SerializeField] private float lookAhead;
    public GameObject obstaclePrefab;

    // Kinematic Arrival vars
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject destination;
    [SerializeField] private Camera cam;
    private Rigidbody rb;
    private float turnSpeed;
    private float moveSpeed;
    private float radiusOfSatisfaction;
    private bool move;
    private bool targetExist;
    private bool targetReached;

    // Formation vars
    [SerializeField] private bool isFourFingerFormation;
    [SerializeField] private bool isWallFormation;
    [SerializeField] private bool isDefensiveFormation;
    [SerializeField] private bool isTwoAbreastFormation;

    [SerializeField] private FormationManager formationManager;
    public Transform anchor;
    private bool goToAnchor;
    [SerializeField] private float anchorOffset;
    ///[SerializeField] private float anchorRadiusOfSatisfaction;
    #endregion

    #region Properties
    public bool IsFourFingerFormation
    {
        get { return this.isFourFingerFormation; }
        set { this.isFourFingerFormation = value; }
    }

    public bool IsWallFormation
    {
        get { return this.isWallFormation; }
        set { this.isWallFormation = value; }
    }
    public bool IsDefensiveFormation
    {
        get { return this.isDefensiveFormation; }
        set { this.isDefensiveFormation = value; }
    }

    public bool IsTwoAbreastFormation
    {
        get { return this.isTwoAbreastFormation; }
        set { this.isTwoAbreastFormation = value; }
    }
    #endregion

    #region Unity Methods
    // Start is called before the first frame update
    void Start()
    {
        formationManager = anchor.GetComponentInParent<FormationManager>();
        rb = player.GetComponent<Rigidbody>();

        move = false;
        targetExist = false;
        isFourFingerFormation = true;

        turnSpeed = 2.5f;
        moveSpeed = 5f;
        radiusOfSatisfaction = 1f;
    }

    // Update is called once per frame
    void Update()
    {
        InputController();
        AvoidObstacles();
    }
    #endregion

    #region Custom Methods
    void InputController()
    {
        if (Input.GetMouseButtonDown(0) && !targetExist)
        {
            move = true;
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                Instantiate(destination, hit.point + new Vector3(0f, 0.5f, 0f), Quaternion.identity);
                targetExist = true;
            }
        }

        // Left Click → Player Moves
        if (Input.GetMouseButtonDown(0) && targetExist)
        {
            move = true;
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                destination.transform.position = hit.point + new Vector3(0f, 0.5f, 0f);
                targetReached = false;
            }
        }

        // Right Click → Creates Obstacles
        if (Input.GetMouseButtonDown(1))
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                Instantiate(obstaclePrefab, hit.point + (hit.transform.position + new Vector3(0f, 0.5f, 0f)), Quaternion.identity);
            }
        }

        // If move is true → Runs the Kinematic Arrival Algorithm
        if (move)
        {
            RunKinematicArrivalAlgorithm();
            formationManager.SetAnchorOffset(Vector3.zero);
        }
        else
        {
            if(goToAnchor)
            {
                GoToAnchor();
            }
        }
    }

    /// <summary>
    /// Based off the implementation given from the class textbook. It uses a rigidbody to move the agent.
    /// The radius of satisfaction is used to avoid the agent wiggling back and forth a specific spot.
    /// </summary>
    void RunKinematicArrivalAlgorithm()
    {
        ///Debug.Log("Agent moving....");

        //Calcutlates the direction the agent needs to rotate to face towards the target
        Vector3 towards = destination.transform.position - transform.position;
      
        if (towards.magnitude > radiusOfSatisfaction)
        {
            ///Debug.Log("Towards Magnitude: " + towards.magnitude);
            //Normalizes the Vector3 to get the direction
            towards.Normalize();
            towards *= moveSpeed;

            //Moves player
            rb.velocity = towards;

            // Rotates the player
            Quaternion targetRotation = Quaternion.LookRotation(towards);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        }
        else
        {
            transform.Translate(Vector3.zero);
            //Stops the agent -- Had to put this in bc the agent kept sliding towards the target after it stopped moving bc its a rb
            rb.velocity = Vector3.zero;
            ///Debug.Log("Destination has been reached...");
            move = false;
            targetReached = true;
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
        //Vector3 ray2 = transform.forward + new Vector3(0.5f, 0f, 0f);
        Vector3 ray2 = eye1.transform.forward;
        ray2.Normalize();
        ray2 *= lookAhead * 0.75f; // Whisker raycasts are shorter than the middle raycast
        //Vector3 ray3 = transform.forward + new Vector3(-0.5f, 0f, 0f);
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
            if(hit.transform.gameObject.layer == 8)
            {
                Debug.Log("Obstacle detected ahead. Going to the Anchor");
                // Goes back and to the side
                formationManager.SetAnchorOffset(new Vector3(-anchorOffset, 0f, -anchorOffset));
                move = false;
                goToAnchor = true;
            } 
        }

        // Collision detected to the right
        if(Physics.Raycast(transform.position, ray2, out hit, avoidDistance, layerMask))
        {
            if (hit.transform.gameObject.layer == 8)
            {
                Debug.Log("Obstacle detected to the right. Going to the Anchor");
                // Goes to the right
                formationManager.SetAnchorOffset(new Vector3(-anchorOffset, 0f, 0f));
                move = false;
                goToAnchor = true;
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
                move = false;
                goToAnchor = true;
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
        ///.Log("Collision -- Leader: Leader has been removed from the formation \nGoing to the anchor to reset and then going back into the formation...");

        Vector3 position = anchor.position;

        // Checks to see if the bot has reached the desired destination
        if (Vector3.Distance(transform.position, position) > radiusOfSatisfaction)
        {
            // Calculate vector to the position in the formation
            Vector3 towards = position - transform.position;

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
            move = true;
            goToAnchor = false;
        }
    }

    public bool GetGoToAnchor()
    {
        return goToAnchor;
    }

    public Vector3 GetDestination()
    {
        return destination.transform.position;
    }
    #endregion
}
