using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PlayerController : MonoBehaviourPunCallbacks, IPunObservable
{
    [HideInInspector]
    public int ID;

    [Header("Info")]
    public float speed;
    public float jumpForce;

    public GameObject hatOBJ;

    [HideInInspector]
    public float curHatTime;

    [Header("Components")]
    public Rigidbody rb;
    public Player photonPlayer;

    [PunRPC]
    public void Initialize(Player player)
    {
        photonPlayer = player;
        ID = player.ActorNumber;

        GameManager.instance.players[ID - 1] = this;

        // give the first player the hat
        if (ID == 1)
        {
            GameManager.instance.GiveHat(ID, true);
        }

        // if this is isnt our local player disable the physics
        //because that controlled by the other player
        if (!photonView.IsMine)
        {
            rb.isKinematic = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (curHatTime >= GameManager.instance.timeToWin && !GameManager.instance.gameEnded || PhotonNetwork.CurrentRoom.PlayerCount == 1)
            {
                GameManager.instance.gameEnded = true;
                GameManager.instance.photonView.RPC("WinGame", RpcTarget.All, ID);
            }
        }

        if (photonView.IsMine)
        {
            Move();

            if (Input.GetKeyDown(KeyCode.Space))
            {
                TryJump();
            }

            //track amount of time we have held the hat
            if (hatOBJ.activeInHierarchy)
            {
                curHatTime += Time.deltaTime;
            }
            else if (hatOBJ.activeInHierarchy == false)
            {
                curHatTime = 0f;
            }
        }
    }

    void Move()
    {
        float xMove = Input.GetAxis("Horizontal") * speed;
        float zMove = Input.GetAxis("Vertical") * speed;

        rb.velocity = new Vector3(xMove, rb.velocity.y, zMove);
    }

    void TryJump()
    {
        Ray ray = new Ray(transform.position, Vector3.down);

        if (Physics.Raycast(ray,0.7f))
        {
            rb.AddForce(Vector3.up * jumpForce,ForceMode.Impulse);
        }
    }

    public void SetHat(bool hasHat)
    {
        hatOBJ.SetActive(hasHat);
    }

    void OnCollisionEnter(Collision collision)
    {
        //makes sure we only check our personal collisions
        if (!photonView.IsMine)
        {
            return;
        }


        //did we hit another player
        if (collision.gameObject.CompareTag("Player"))
        {
            //do they have the hat 
            if (GameManager.instance.GetPlayer(collision.gameObject).ID == GameManager.instance.playerWithHat)
            {
                //can we even get the hat
                if (GameManager.instance.CanGetHat())
                {
                    //give us the hat
                    GameManager.instance.photonView.RPC("GiveHat", RpcTarget.All, ID, false);
                }
            }
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream,PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(curHatTime);

        }
        else if (stream.IsReading)
        {
            curHatTime = (float)stream.ReceiveNext();
        }
    }
}
