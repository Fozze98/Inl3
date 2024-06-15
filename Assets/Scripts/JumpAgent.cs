using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.VisualScripting;

public class JumpAgent : Agent
{
    public Transform targetTransform;
    public Rigidbody2D rb;

    [Header("Jump Variables")]
    public float jumpForce = 1f;
    public bool isJumping = false;
    public float groundCheckRange = 0.5f;
    public LayerMask groundLayer;
    public Transform spawnPosition;
    public List<GameObject> rewardTrigger = new List<GameObject>();
    public List<GameObject> punishmentZones = new List<GameObject>();
    public List<GameObject> platforms = new List<GameObject>();


    [Header("Visual Testing Materials")]
    public Material platformReached_m;
    public Material defaultMaterial_m;

    public override void OnEpisodeBegin()
    {
        transform.localPosition = spawnPosition.localPosition;
        EnableAllRewardTriggers();
        targetTransform.localPosition = rewardTrigger[0].GetComponentInParent<Transform>().localPosition;
    }


    private void EnableAllRewardTriggers()
    {
        foreach(GameObject trigger in rewardTrigger)
        {
            trigger.gameObject.SetActive(true);
            trigger.GetComponentInParent<SpriteRenderer>().color = Color.white;
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(targetTransform.localPosition);
    }
   

    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveX = actions.ContinuousActions[0];

        float movementspeed = 5f;
        transform.localPosition += new Vector3(moveX, 0, 0) * Time.deltaTime * movementspeed;

        int jump = actions.DiscreteActions[0];
        
        if(jump == 1 && IsGrounded())
        {
            Jump();
        }
       
    }

    //Denna metod Heuristic används endast för testning av movement koden och har ingen användning för agenten. Denna kod används endast om du själv...
    //... vill styra agenten för att testa att den kan röra sig som man planerat. 
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        
        ActionSegment<float> continouosActions = actionsOut.ContinuousActions;
        continouosActions[0] = Input.GetAxisRaw("Horizontal");
        continouosActions[1] = Input.GetAxisRaw("Vertical");
  
        ActionSegment<int> descreteActions = actionsOut.DiscreteActions;
        if (Input.GetKey(KeyCode.Space) && IsGrounded()) {
            descreteActions[0] = 1;
            Jump();
        }
        Debug.Log(descreteActions[0]);
        
    }  
    

    private void Jump()
    {
        if(rb == null)
        {
            Debug.Log("Missing Rigidbody2D");
            return;
        }

        isJumping = true;
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);

    }

    bool IsGrounded()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, groundCheckRange, groundLayer);
        
        if(hit.collider != null)
        {
            isJumping = false;
            return true;
        }

        return false;
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.TryGetComponent<goal>(out goal goal))
        {
            SetReward(+1f);
            EndEpisode();
        }

        if (other.TryGetComponent<rewardtrigger>(out rewardtrigger reward))
        {
            AddReward(+1);
            SetNewTargetTransform(other.gameObject.name);
            other.gameObject.SetActive(false);
            other.GetComponentInParent<SpriteRenderer>().color = Color.green;

            Debug.Log("I reached: " + other.name);
        }

        if (other.TryGetComponent<wall>(out wall wall))
        {
            SetReward(-1f);
            Debug.Log("I Hit a punishment Zone! ID: " + other.name);
            EndEpisode();
        }
        
    }

    private void SetNewTargetTransform(string id)
    {
        if(id == rewardTrigger[0].gameObject.name) //Om vi hittat plattform#1
        {
            targetTransform.localPosition = rewardTrigger[1].transform.localPosition; //sätter vi targetTransform till plattform#2
        }
        if (id == rewardTrigger[1].gameObject.name) //Om vi hittat plattform#2
        {
            targetTransform.localPosition = rewardTrigger[2].transform.localPosition;//sätter vi targetTransform till plattform#3
           
        }
        if (id == rewardTrigger[2].gameObject.name) //Om vi hittat plattform#3
        {
            targetTransform.localPosition = rewardTrigger[3].transform.localPosition;//sätter vi targetTransform till plattform#4
            
        }
        if (id == rewardTrigger[3].gameObject.name) //Om vi hittat plattform#4
        {
            targetTransform.localPosition = rewardTrigger[4].transform.localPosition;//sätter vi targetTransform till plattform#5
        }
       
    }


    private void activatePunishmentZones(string id) 
    {
        if(id == rewardTrigger[0].gameObject.name)
        {
            punishmentZones[0].gameObject.SetActive(true);
           // Debug.Log("PUNISHEMNT ZONE 1 ACTIVATED");
        }

        if (id == rewardTrigger[1].gameObject.name)
        {
            punishmentZones[1].gameObject.SetActive(true);
           // Debug.Log("PUNISHEMNT ZONE 2 ACTIVATED");
        }

        if (id == rewardTrigger[2].gameObject.name)
        {
            punishmentZones[2].gameObject.SetActive(true);
            //Debug.Log("PUNISHEMNT ZONE 3 ACTIVATED");
        }

        if (id == rewardTrigger[3].gameObject.name)
        {
            punishmentZones[3].gameObject.SetActive(true);
           // Debug.Log("PUNISHEMNT ZONE 4 ACTIVATED");
        }


    }

}
