using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerMove : MonoBehaviour
{
    Rigidbody2D rigid;
    GameManager gm;
    SpriteRenderer sprite;
    GameManager manager;

    public GameObject restartBtn;
    //public GameObject text;
    public TextMeshProUGUI winText;
    public float walkSpeed;
    bool facingRight = true;
    public float h;

    [Header("JumpItems")]
    //public GameObject item1;
    //public GameObject item2;
    //public GameObject item3;

    [Header("GroundCheck")]
    public Vector2 boxSize;
    public float castDistance;
    public LayerMask groundLayer;
    
    [Header("Dash")]
    public TrailRenderer trail;
    bool canDash = true;
    bool isDashing;
    public float dashingPower = 24f;
    public float dashingTime = 0.2f;
    public float dashingCooldown = 1f;
    

    [Header("CoyoteTime")]
    public float coyoteTime = 0.2f;
    public float coyoteTimeCounter;

    [Header("JumpBuffer")]
    public float jumpBufferTime = 0.2f;
    public float jumpBufferCounter;

    [Header("Jump")]
    //public int jump = 2;
    //public int jumpCount;
    public float jumpPower = 8f;
    public int boostPower;

    [Header("Wall")]
    bool isWallSliding;
    public float wallSlidingSpeed = 2f;
    public Transform wallCheck;
    public LayerMask wallLayer;

    bool isWallJumping;
    public float wallJumpingDirection;
    public float wallJumpingTime = 0.2f;
    public float wallJumpingCounter;
    public Vector2 wallJumpingPower = new Vector2(8f, 16f);
    
    

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();

        //jumpCount = jump;
    }

    // Update is called once per frame
    void Update()
    {
        Jump();

        if (isDashing)
        {
            return;
        }

        if(Input.GetButton("Dash") && canDash)
        {
            StartCoroutine(Dash());
        }

       
        
        

        WallSlide();
        WallJump();
        

        if (!isWallJumping)
        {
            
            Flip();
        }
        

    }

    void FixedUpdate()
    {

        if (isDashing)
        {
            return;
        }

        h = Input.GetAxisRaw("Horizontal");

        if (!isWallJumping)
        {
            rigid.velocity = new Vector2(h * walkSpeed, rigid.velocity.y);
        }
        





    }

    void Flip()
    {
        if (facingRight && h < 0 || !facingRight && h > 0)
        {
            facingRight = !facingRight;
            Vector3 localScale = transform.localScale;
            localScale.x *= -1f;
            transform.localScale = localScale;
        }
    }

    public bool IsGround()
    {
        if (Physics2D.BoxCast(transform.position, boxSize, 0, -transform.up, castDistance, groundLayer))
        {
            
            return true;
        }
        else
        {
            return false;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position - transform.up * castDistance, boxSize);

    }

    //jump
    void Jump()
    {
        ////coyote
        //if (IsGround())
        //{
        //    coyoteTimeCounter = coyoteTime;
        //    //jumpCount = jump;
            
        //}
        //else
        //{
        //    coyoteTimeCounter -= Time.deltaTime;
        //}
        ////////jump buffer
        ////if (Input.GetButtonDown("Jump"))
        ////{
        ////    jumpBufferCounter = jumpBufferTime;
        ////}
        ////else
        ////{
        ////    jumpBufferCounter -= Time.deltaTime;
        ////}

        //jump
        if (IsGround() && Input.GetButtonDown("Jump"))
        {
            rigid.velocity = Vector2.up * jumpPower;
        }

        //if (coyoteTimeCounter < 0f && Input.GetButtonDown("Jump"))
        //{
        //    rigid.velocity = (Vector2.up * jumpPower);
        //    //jumpCount--;
        //    coyoteTimeCounter = 0f;
        //    Debug.Log("Jump");
        //}

        if (Input.GetButtonDown("Jump") && wallJumpingCounter > 0f)
        {
            isWallJumping = true;
            rigid.velocity = new Vector2(wallJumpingDirection * wallJumpingPower.x, wallJumpingPower.y);
            wallJumpingCounter = 0f;

            Invoke(nameof(CancelWallJump), wallJumpingTime + 0.1f);
        }
    }

    void WallJump()
    {
        if (isWallSliding)
        {
            isWallJumping = false;
            wallJumpingDirection = -transform.localScale.x;
            wallJumpingCounter = wallJumpingTime;

            if (transform.localScale.x != wallJumpingDirection)
            {
                facingRight = !facingRight;
                Vector3 ls = transform.localScale;
                ls.x *= -1;
                transform.localScale = ls;
            }

            CancelInvoke(nameof(CancelWallJump));
        }
        else if(wallJumpingCounter > 0f)
        {
            wallJumpingCounter -= Time.deltaTime;
        }
    }

    void CancelWallJump()
    {
        isWallJumping = false;
    }
   
    //dash
    private IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;
        float originalGravity = rigid.gravityScale;
        rigid.gravityScale = 0f;
        rigid.velocity = new Vector2(transform.localScale.x * dashingPower, 0f);
        trail.emitting = true;
        yield return new WaitForSeconds(dashingTime);
        trail.emitting = false;
        rigid.gravityScale = originalGravity;
        isDashing = false;
        yield return new WaitForSeconds(dashingCooldown);
        canDash = true;
    }

    //respawn items
    IEnumerator RespawnItems(Collider2D collision, int time)
    {
        yield return new WaitForSeconds(time);

        collision.gameObject.SetActive(true);

    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "JumpItem")
        {
            //jump reset
            float originalGravity = rigid.gravityScale;
            //jumpCount = 2;
            
            rigid.gravityScale = originalGravity;
            //jumpCount--;
            
            //deactivate
            collision.gameObject.SetActive(false);

            //respawn
            StartCoroutine(RespawnItems(collision, 1));
        }

        if(collision.gameObject.tag == "Finish")
        {
            restartBtn.gameObject.SetActive(true);
            winText.gameObject.SetActive(true);
            Time.timeScale = 0f;
        }

        if(collision.gameObject.tag == "Enemy")
        {
            Time.timeScale = 0f;

            winText.gameObject.SetActive(true);

            winText.text = "Lose";

            restartBtn.gameObject.SetActive(true);
        }

        
    }

    public void VelocityZero()
    {
        rigid.velocity = Vector2.zero;
    }

    bool IsWall()
    {
        return Physics2D.OverlapCircle(wallCheck.position, 0.2f, wallLayer);
    }

    void WallSlide()
    {
        if(IsWall() && !IsGround() && h != 0)
        {
            isWallSliding = true;
            rigid.velocity = new Vector2(rigid.velocity.x, Mathf.Clamp(rigid.velocity.y, -wallSlidingSpeed, float.MaxValue));
        }
        else
        {
            isWallSliding = false;
        }
    }

   
}



















//public bool isGrounded()
//{
//    if (Physics2D.CircleCast(transform.position, circleCastFloat, Vector2.down, circleCastDis))
//        return true;
//    else
//        return false;
//}

/* ////coyote time
if (isGround())
{
   coyoteTimeCounter = coyoteTime;
   jumpCount = jump;
}
else
{
   coyoteTimeCounter -= Time.deltaTime;
}

//jump buffer
if (Input.GetButtonDown("Jump"))
{
   jumpCount--;
   jumpBufferCounter = jumpBufferTime;
}

else
{
   jumpBufferCounter -= Time.deltaTime;
}

//jump
if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f && jumpCount > 0)
{
   rigid.velocity = Vector2.up * jumpPower;
   jumpBufferCounter = 0f;

}


if (Input.GetButtonUp("Jump") && rigid.velocity.y > 0f)
{
   rigid.velocity = new Vector2(rigid.velocity.x, rigid.velocity.y * 0.5f);
   coyoteTimeCounter = 0f;

}*/