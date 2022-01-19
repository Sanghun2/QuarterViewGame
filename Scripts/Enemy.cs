using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    public enum Type { A,B,C,D};
    public Type enemyType;

    public int score;
    public GameObject[] coins;

    public int maxHealth;
    public int curHealth;
    public Transform target;
    public BoxCollider meleeArea;
    public GameObject bullet;

    public bool isDead;
    public bool isChase;
    public bool isAttack;

    internal Rigidbody rigid;
    internal BoxCollider boxCollider;
    internal MeshRenderer[] meshes;
    internal NavMeshAgent nav;
    internal Animator anim;

    public GameManager gameManager;

    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();
        meshes = GetComponentsInChildren<MeshRenderer>();
        nav = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>();

        if (enemyType != Type.D)
        {
            Invoke("ChaseStart", 2);
        }
    }

    void FixedUpdate()
    {
        Targeting();
        FreezeVelocity();
    }

    void Update()
    {
        if (nav.enabled && enemyType != Type.D)
        {
            nav.SetDestination(target.position);
            nav.isStopped = !isChase;
        }
    }

    void Targeting()
    {
        if (!isDead && enemyType != Type.D)
        {
            float targetRadius = 0;
            float targetRange = 0;

            switch (enemyType)
            {
                case Type.A:
                    targetRadius = 1.5f;
                    targetRange = 3f;
                    break;
                case Type.B:
                    targetRadius = 1f;
                    targetRange = 12f;
                    break;
                case Type.C:
                    targetRadius = .5f;
                    targetRange = 40f;
                    break;
                default:
                    break;
            }

            RaycastHit[] rayHits = Physics.SphereCastAll(transform.position,
                targetRadius,
                Vector3.forward,
                targetRange,
                LayerMask.GetMask("Player"));

            if (rayHits.Length > 0 && !isAttack)
            {
                StartCoroutine(Attack());
            }
        }
    }

    IEnumerator Attack()
    {
        isChase = false;
        isAttack = true;
        anim.SetBool("IsAttack", true);

        switch (enemyType)
        {
            case Type.A:
                yield return new WaitForSeconds(0.2f);
                meleeArea.enabled = true;

                yield return new WaitForSeconds(1f);
                meleeArea.enabled = false;

                yield return new WaitForSeconds(1f);
                break;
            case Type.B:
                yield return new WaitForSeconds(0.1f);
                rigid.AddForce(transform.forward*20, ForceMode.Impulse);
                meleeArea.enabled = true;

                yield return new WaitForSeconds(1f);
                rigid.velocity = Vector3.zero;
                meleeArea.enabled = false;

                yield return new WaitForSeconds(2f);
                break;
            case Type.C:
                yield return new WaitForSeconds(0.5f);
                GameObject instantBullet = Instantiate(bullet, transform.position, transform.rotation);
                Rigidbody rigidBullet = instantBullet.GetComponent<Rigidbody>();
                rigidBullet.velocity = transform.forward * 20;
                yield return new WaitForSeconds(2f);
                break;
            default:
                break;
        }
        

        isChase = true;
        isAttack = false;
        anim.SetBool("IsAttack", false);
    }

    void ChaseStart()
    {
        isChase = true;
        anim.SetBool("IsWalk", true);
    }

    void FreezeVelocity()
    {
        if (isChase)
        {
            rigid.velocity = Vector3.zero;
            rigid.angularVelocity = Vector3.zero;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Melee")
        {
            Weapon weapon = other.GetComponent<Weapon>();
            curHealth -= weapon.damage;
            Vector3 reactVec = transform.position - other.transform.position;
            if (!isDead)
            {
                StartCoroutine(OnDamge(reactVec));
            }
        }
        else if (other.tag == "Bullet")
        {
            Bullet bullet = other.GetComponent<Bullet>();
            curHealth -= bullet.damage;
            Vector3 reactVec = transform.position - other.transform.position;
            Destroy(other.gameObject);
            if (!isDead)
            {
                StartCoroutine(OnDamge(reactVec));
            }
        }
    }

    public void HitByGrenade(Vector3 explosionPos)
    {
        curHealth -= 100;
        Vector3 reacVec = transform.position - explosionPos;
        StartCoroutine(OnDamge(reacVec, true));
    }

    IEnumerator OnDamge(Vector3 reactVec, bool isGrenade = false)
    {
        foreach (var mesh in meshes)
        {
            mesh.material.color = Color.red;
        }
        
        yield return new WaitForSeconds(0.1f);

        if (curHealth > 0)
        {
            foreach (var mesh in meshes)
            {
                mesh.material.color = Color.white;
            }
        }
        else
        {
            foreach (var mesh in meshes)
            {
                mesh.material.color = Color.gray;
            }
            gameObject.layer = 12;
            isChase = false;
            nav.enabled = false;
            anim.SetTrigger("doDie");

            Player player = target.GetComponent<Player>();
            player.score += score;
            int ranCoin = Random.Range(0, 3);
            Instantiate(coins[ranCoin], transform.position, Quaternion.identity);

            switch (enemyType)
            {
                case Type.A:
                    gameManager.enemyCntA--;
                    break;
                case Type.B:
                    gameManager.enemyCntB--;
                    break;
                case Type.C:
                    gameManager.enemyCntC--;
                    break;
                case Type.D:
                    gameManager.enemyCntD--;
                    break;
                default:
                    break;
            }

            if (isGrenade)
            {
                reactVec = reactVec.normalized;
                reactVec += Vector3.up*3;

                rigid.freezeRotation = false;
                rigid.AddForce(reactVec * 5, ForceMode.Impulse);
                rigid.AddTorque(reactVec * 15, ForceMode.Impulse);
            }
            else
            {
                reactVec = reactVec.normalized;
                reactVec += Vector3.up;
                rigid.AddForce(reactVec * 5, ForceMode.Impulse);
            }
            
            isDead = true;
            StopAllCoroutines();
            Destroy(gameObject, 4f);
        }
    }
}
