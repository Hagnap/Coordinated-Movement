using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FormationManager : MonoBehaviour
{
    #region Variables
    [SerializeField] private GameObject leader;
    [SerializeField] private GameObject memberOne;
    [SerializeField] private GameObject memberTwo;
    [SerializeField] private GameObject memberThree;

    [SerializeField] private bool isFourFinger;

    [SerializeField] private Vector3 offset;

    private Vector3 centerOfMass;
    #endregion

    #region Properties
    public bool IsFourFinger
    { 
        get { return this.isFourFinger; }
        set { this.isFourFinger = value; }
    }
    #endregion

    #region Unity Methods
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        // Sets the position for the anchor based off the CoM and then adds an offset to it
        centerOfMass = CenterOfMassPosition();
        transform.position = centerOfMass + offset;
    }
    #endregion

    #region Custom Methods

    /// <summary>
    /// This method is used to calculate the Center of Mass with respect to the agents in the formation.
    /// </summary>
    /// <returns>Returns a Vector3 storing the position of the formation's anchor.</returns>
    public Vector3 CenterOfMassPosition()
    {
        Vector3 position = Vector3.zero;

        // Sum of all the positions
        position = (leader.transform.position + memberOne.transform.position + memberTwo.transform.position + memberThree.transform.position);

        // Divides by 4 to get the average position aka center of mass
        position /= 4f;

        return position;
    }

    public void SetAnchorOffset(Vector3 val)
    {
        offset = val;
    }
    #endregion
}
