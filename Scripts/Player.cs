using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] float speed;
    public GameObject[] weapons;
    Weapon equipWeapon;
    int equipWeaponIndex;
    public bool[] hasWeapons;

    [Header("카메라")]
    [Space(10f)]
    public Camera followCamera;

    [Header("현재 아이템")]
    [Space(10f)]
    public int ammo;
    public int coin;
    public int health;
    public int hasGrenades;
    public GameObject[] grenades;

    [Header("최대 아이템")][Space(10f)]
    public int maxAmmo;
    public int maxCoin;
    public int maxHealth;
    public int maxHasGrenades;

    float hAxis;
    float vAxis;
    bool wDown;
    bool jDown;
    bool iDown;
    bool sDown1;
    bool sDown2;
    bool sDown3;
    bool fDown;
    bool rDown;

    float fireDelay;

    bool isJump;
    bool isDodge;
    bool isSwap;
    bool isFireReady = true;
    bool isReload;
    bool isBorder;

    Vector3 moveVec;
    Vector3 dodgeVec;

    Rigidbody rigid;
    Animator anim;
    GameObject nearObject;

    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();
    }

    void FixedUpdate()
    {
        FreezeRotation();
        StopToWall();
    }

    void Update()
    {
        GetInput();
        Move();
        Turn();
        Jump();
        Dodge();
        Interaction();
        Swap();
        Attack();
        Reload();
    }

    void GetInput()
    {
        hAxis = Input.GetAxisRaw("Horizontal");
        vAxis = Input.GetAxisRaw("Vertical");
        wDown = Input.GetButton("Walk"); //방향이 아니므로 Button 사용
        jDown = Input.GetButtonDown("Jump");
        iDown = Input.GetButtonDown("Interaction");
        sDown1 = Input.GetButtonDown("Swap1");
        sDown2 = Input.GetButtonDown("Swap2");
        sDown3 = Input.GetButtonDown("Swap3");
        fDown = Input.GetButton("Fire1");
        rDown = Input.GetButtonDown("Reload");
    }

    void Reload()
    {
        if (equipWeapon == null) return;
        if (equipWeapon.type == Weapon.Type.Melee) return;
        if (ammo == 0) return;
        if (rDown && !isDodge && !isJump && !isSwap && isFireReady)
        {
            anim.SetTrigger("DoReload");
            isReload = true;

            Invoke("ReloadOut", 3f);
        }
    }

    void ReloadOut()
    {
        int reAmmo = ammo < equipWeapon.maxAmmo ? ammo : equipWeapon.maxAmmo;
        equipWeapon.curAmmo = reAmmo;
        ammo -= reAmmo;
        isReload = false;
    }

    void Attack()
    {
        if (equipWeapon == null)
        {
            return;
        }

        if (isReload)
        {
            return;
        }

        fireDelay += Time.deltaTime;
        isFireReady = equipWeapon.rate < fireDelay;

        if (fDown && isFireReady && !isDodge && !isSwap)
        {
            equipWeapon.Use();
            anim.SetTrigger(equipWeapon.type == Weapon.Type.Melee ?  "DoSwing" : "DoShot");
            fireDelay = 0;
        }
    }

    void Move()
    {
        moveVec = new Vector3(hAxis, 0, vAxis).normalized;

        if (isDodge)
        {
            moveVec = dodgeVec;
        }

        if (isSwap || !isFireReady || isReload)
        {
            moveVec = Vector3.zero;
        }

        if (!isBorder)
        {
            transform.position += moveVec * speed * Time.deltaTime * (wDown ? 0.3f : 1f);
        }

        anim.SetBool("IsRun", moveVec != Vector3.zero);
        anim.SetBool("IsWalk", wDown);
    }

    void Turn()
    {
        //키보드 회전
        transform.LookAt(transform.position + moveVec);

        //마우스 회전
        if (fDown)
        {
            Ray ray = followCamera.ScreenPointToRay(Input.mousePosition); //스크린에서 월드로 Ray를 쏘는 함수
            RaycastHit rayHit;
            if (Physics.Raycast(ray, out rayHit, 100)) //out은 ruturn처럼 반환값을 주어진 변수에 저장하는 키워드
            {
                Vector3 nextVec = rayHit.point - transform.position;
                nextVec.y = 0;
                transform.LookAt(transform.position + nextVec);
            }
        }
    }

    void Jump()
    {
        if (jDown && moveVec == Vector3.zero && !isJump && !isDodge)
        {
            rigid.AddForce(Vector3.up * 10, ForceMode.Impulse);
            anim.SetBool("IsJump", true);
            anim.SetTrigger("DoJump");
            isJump = true;
        }
    }

    void Dodge()
    {
        if (jDown && moveVec != Vector3.zero && !isJump && !isDodge)
        {
            speed *= 1.2f;
            dodgeVec = moveVec;
            anim.SetTrigger("DoDodge");
            isDodge = true;

            Invoke("DodgeOut", 0.7f);
        }
    }

    void DodgeOut()
    {
        speed *= (5/6f);
        isDodge = false;
    }

    void Swap()
    {
        if (sDown1 && (!hasWeapons[0] || equipWeaponIndex == 0)) return;
        if (sDown2 && (!hasWeapons[1] || equipWeaponIndex == 1)) return;
        if (sDown3 && (!hasWeapons[2] || equipWeaponIndex == 2)) return;

        int weaponIndex = -1;
        weaponIndex = sDown1 ? 0 : sDown2 ? 1 : sDown3 ? 2 : -1;

        if ((sDown1 || sDown2 || sDown3) && !isJump && !isDodge)
        {
            if (equipWeapon != null)
            {
                equipWeapon.gameObject.SetActive(false);
            }
            
            equipWeaponIndex = weaponIndex;
            equipWeapon = weapons[weaponIndex].GetComponent<Weapon>();
            equipWeapon.gameObject.SetActive(true);

            anim.SetTrigger("DoSwap");
            isSwap = true;
            Invoke("SwapOut", 0.3f);
        }
    }

    void SwapOut()
    {
        isSwap = false;
    }

    void Interaction()
    {
        if (iDown && nearObject != null && !isJump && !isDodge)
        {
            if (nearObject.tag == "Weapon")
            {
                Item item = nearObject.GetComponent<Item>();
                int weaponIndex = item.value;
                hasWeapons[weaponIndex] = true;

                Destroy(nearObject);
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Floor")
        {
            anim.SetBool("IsJump", false);
            isJump = false;
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.tag == "Weapon")
        {
            nearObject = other.gameObject;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Item")
        {
            Item item = other.GetComponent<Item>();
            switch (item.type)
            {
                case Item.Type.Ammo:
                    ammo += item.value;
                    if (ammo > maxAmmo)
                    {
                        ammo = maxAmmo;
                    }
                    break;
                case Item.Type.Coin:
                    coin += item.value;
                    if (coin > maxCoin)
                    {
                        coin = maxCoin;
                    }
                    break;
                case Item.Type.Grenade:
                    grenades[hasGrenades].SetActive(true);
                    hasGrenades += item.value;
                    if (hasGrenades > maxHasGrenades)
                    {
                        hasGrenades = maxHasGrenades;
                    }
                    break;
                case Item.Type.Heart:
                    health += item.value;
                    if (health > maxHealth)
                    {
                        health = maxHealth;
                    }
                    break;
                default:
                    break;
            }

            Destroy(other.gameObject);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Weapon")
        {
            nearObject = null;
        }
    }

    void FreezeRotation()
    {
        rigid.angularVelocity = Vector3.zero; 
    }

    void StopToWall()
    {
        //시작위치, 레이방향과 길이, 색
        Debug.DrawRay(transform.position, transform.forward*5, Color.green );
        isBorder = Physics.Raycast(transform.position, moveVec, 5, LayerMask.GetMask("Wall"));
    }
}
