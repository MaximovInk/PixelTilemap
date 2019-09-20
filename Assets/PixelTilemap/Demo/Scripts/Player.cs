namespace MaximovInk.PixelTilemap
{
    using UnityEngine;

    public class Player : MonoBehaviour
    {
        public Vector2 speedThresould = new Vector2(20,10);

        private Vector2 move;
        
        public Rigidbody2D rb;

        public Vector2 Speed = new Vector2(10, 20);

        public float distance = 2;

        public LayerMask ground;

        private bool facing_right = true;

        private void Start()
        {
            FindObjectOfType<CameraFollow>().target = transform;
        }

        private void Update()
        {
            move = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        }

       
        private void FixedUpdate()
        {
            if (move.x > 0 && !facing_right || move.x < 0 && facing_right)
                Flip();


                bool jump = false;

                if (move.y > 0)
                {
                    RaycastHit2D ray = Physics2D.Raycast(new Vector2(transform.position.x, transform.position.y), Vector3.down, distance, ground);

                    if (ray)
                    {
                        jump = true;
                    }

                }

                rb.velocity = new Vector2(move.x * Speed.x, jump ? Speed.y : rb.velocity.y); 
            



            rb.velocity = new Vector2(Mathf.Clamp(rb.velocity.x, -speedThresould.x, speedThresould.x), Mathf.Clamp(rb.velocity.y,-speedThresould.y,speedThresould.y));
        }

        protected void Flip()
        {
            facing_right = !facing_right;
            transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
        }
    }
}
