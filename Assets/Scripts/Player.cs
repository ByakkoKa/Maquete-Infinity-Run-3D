using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{

    public float minSpeed = 10f;
    public float maxSpeed = 30f;
    public float laneSpeed;
    public float jumpLenght;
    public float jumpHeight;
    public float slideLenght;
    public int maxLife = 3;
    public int currentLife;
    public float invencibleTime;
    public GameObject model;


    private float speed;
    private Animator anim;
    private Rigidbody rb;
    private BoxCollider boxCollider;
    private int currentLane = 1;
    private Vector3 verticalTargetPosition;
    private bool jumping = false;
    private float jumpStart;
    private bool sliding = false;
    private float slideStart;
    private Vector3 boxColliderSize;
    private Vector3 boxColliderCenter;
    private bool isSwipping = false;
    private Vector2 startingTouch;
    private bool invencible = false;

    


    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();
        boxCollider = GetComponent<BoxCollider>();
        boxColliderSize = boxCollider.size;
        boxColliderCenter = boxCollider.center;
        currentLife = maxLife;
        speed = minSpeed;


    }

    // Update is called once per frame
    void Update()
    {

        //Inputs de teste no teclado
        if (Input.GetKeyDown(KeyCode.LeftArrow)) {

            ChangeLane(-1);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            ChangeLane(1);
           
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            Jump();
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            Slide();
        }

        if (jumping)
        {
            float ratio = (transform.position.z - jumpStart) / jumpLenght;
            if (ratio >= 1f)
            {
                jumping = false;
                anim.SetBool("isGround", true);
            }
            else
            {
                verticalTargetPosition.y = Mathf.Sin(ratio * Mathf.PI) * jumpHeight;
            }
        }
        else
        {
           verticalTargetPosition.y = Mathf.MoveTowards(verticalTargetPosition.y, 0, 5 * Time.deltaTime);
        }

        if (sliding)
        {
            float ratio = (transform.position.z - slideStart) / slideLenght;
            if(ratio >= 1f)
            {
                sliding = false;
                anim.SetBool("slide", false);
                boxCollider.size = boxColliderSize;
                boxCollider.center = boxColliderCenter;
            }

        }
 
        //Inputs para touch e mouse
        if(Input.touchCount == 1)
        {
            if (isSwipping)
            {
                Vector2 diff = Input.GetTouch(0).position - startingTouch;
                diff = new Vector2(diff.x / Screen.width, diff.y / Screen.width);
                if (diff.magnitude > 0.01f)
                {
                    if (Mathf.Abs(diff.y) > Mathf.Abs(diff.x))
                    {
                        if (diff.y < 0)
                        {
                            Slide();
                        }
                        else
                        {
                            Jump();
                        }
                    }
                    else
                    {
                        if (diff.x < 0)
                        {
                            ChangeLane(-1);
                        }
                        else
                        {
                            ChangeLane(1);
                        }
                    }

                    isSwipping = false;

                }
            }

            if (Input.GetTouch(0).phase == TouchPhase.Began)
            {
                startingTouch = Input.GetTouch(0).position;
                isSwipping = true;
            }
            else if (Input.GetTouch(0).phase == TouchPhase.Ended)
            {
                isSwipping = false;
            }

        }

        

        Vector3 targetPosition = new Vector3(verticalTargetPosition.x, verticalTargetPosition.y, transform.position.z);
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, laneSpeed * Time.deltaTime);

    }

    private void FixedUpdate()
    {
        rb.velocity = Vector3.forward * speed;
        anim.SetFloat("velocity", rb.velocity.z);
    }

    void ChangeLane(int direction)
    {
        int targetLane = currentLane + direction;
        if(targetLane < 0 || targetLane > 2)
        {
            return;
        }
        currentLane = targetLane;
        verticalTargetPosition = new Vector3((currentLane - 1), 0, 0);

    }

    void Jump()
    {
        if (!jumping)
        {
            jumpStart = transform.position.z;
            anim.SetFloat("actionSpeed", speed / jumpLenght);
            anim.SetBool("isGround", false);
            jumping = true;
        }
    }

    void Slide()
    {
        if(!jumping && !sliding)
        {
            slideStart = transform.position.z;
            anim.SetFloat("actionSpeed", speed / slideLenght);
            anim.SetBool("slide", true);
            Vector3 newSize = boxCollider.size;
            newSize.y = newSize.y / 2;
            boxCollider.size = newSize;
            Vector3 newPosition = boxCollider.center;
            newPosition.y = newPosition.y / 2;
            boxCollider.center = newPosition;
            sliding = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (invencible)
        {
            return;
        }
        if (other.CompareTag("Obstacle"))
        {
            currentLife--;
            anim.SetTrigger("hit");
            speed = 0;
            if(currentLife <= 0)
            {
                //Game Over
            }
            else
            {
                StartCoroutine(Blinking(invencibleTime));
            }
        }
    }

    IEnumerator Blinking(float time)
    {
        invencible = true;
        float timer = 0;
        float currentBlink = 1;
        float lastBlink = 0;
        float blinkPeriod = 0.1f;
        bool enabled = false;
        yield return new WaitForSeconds(1f);
        speed = minSpeed;
        while (timer < time && invencible)
        {
            model.SetActive(enabled);

            yield return null;
            timer += Time.deltaTime;
            lastBlink += Time.deltaTime;
            if (blinkPeriod < lastBlink)
            {
                lastBlink = 0;
                currentBlink--;
                enabled = !enabled;
            }
        }
        model.SetActive(true);
        invencible = false;
    }
}
