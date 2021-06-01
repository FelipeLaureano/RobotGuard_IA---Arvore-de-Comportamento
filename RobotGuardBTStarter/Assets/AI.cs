using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using Panda;

public class AI : MonoBehaviour
{
    public Transform player;
    public Transform bulletSpawn;
    public Slider healthBar;   
    public GameObject bulletPrefab;

    NavMeshAgent agent;
    public Vector3 destination; // The movement destination.
    public Vector3 target;      // The position to aim to.
    float health = 100.0f; //vida
    float rotSpeed = 5.0f;

    float visibleRange = 80.0f;
    float shotRange = 40.0f;

    void Start()
    {
        agent = this.GetComponent<NavMeshAgent>();
        agent.stoppingDistance = shotRange - 5; //for a little buffer
        InvokeRepeating("UpdateHealth",5,0.5f);
    }

    void Update()
    {
        Vector3 healthBarPos = Camera.main.WorldToScreenPoint(this.transform.position);
        healthBar.value = (int)health;
        healthBar.transform.position = healthBarPos + new Vector3(0,60,0);
    }

    void UpdateHealth()
    {
       if(health < 100)
        health ++;
    }

    void OnCollisionEnter(Collision col)
    {
        if(col.gameObject.tag == "bullet")
        {
            health -= 10;
        }
    }

    [Task]
    public void PickRandomDestination() //funçao para pegar ponto aleatorio no mapa
    {
        Vector3 dest = new Vector3(Random.Range(-100, 100), 0, Random.Range(-100, 100));
        agent.SetDestination(dest);
        Task.current.Succeed();//Chamar metodo pra dentro do codigo
    }

    [Task]
    public void PickDestination(int x, int z) //função para pegar um ponto no mapa e recebe valores do PandaBT
    {
        Vector3 dest = new Vector3(x, 0, z); //y=0 pois robo se mantém no chão
        agent.SetDestination(dest);
        Task.current.Succeed();//Chamar o retorno
    }

    [Task]
    public void MoveToDestination() //função para se mover até o destino
    {
        if (Task.isInspected)
        {
            Task.current.debugInfo = string.Format("t={0:0.00}", Time.time); //timer - contador para testar se esta funcionando
        }
        if(agent.remainingDistance <= agent.stoppingDistance && !agent.pathPending)
        {
            Task.current.Succeed();//Chamar metodo pra dentro do codigo
        }
    }

    [Task]
    public void TargetPlayer()
    {
        target = player.transform.position; //Achar a posição do player
        Task.current.Succeed(); //Retorno de informação
    }

    [Task]
    public bool Fire()
    {
        GameObject bullet = GameObject.Instantiate(bulletPrefab,
            bulletSpawn.transform.position, bulletSpawn.transform.rotation); //instancia da bala

        bullet.GetComponent<Rigidbody>().AddForce(bullet.transform.forward * 2000); //aceleração da bala

        return true;
    }

    [Task]
    public void LookAtTarget() //Alteração de angulo e posição para achar o alvo
    {
        Vector3 direction = target - this.transform.position;

        this.transform.rotation = Quaternion.Slerp(this.transform.rotation,
            Quaternion.LookRotation(direction), Time.deltaTime * rotSpeed);

        if (Task.isInspected)
        {
            Task.current.debugInfo = string.Format("angle={0}",
                Vector3.Angle(this.transform.forward, direction));
        }
        if(Vector3.Angle(this.transform.forward, direction) < 5.0f)
        {
            Task.current.Succeed();
        }
    }

    [Task]
    bool SeePlayer() //método para verificar se robo consegue ver o player e só depois continuar outros comportamentos
    {
        Vector3 distance = player.transform.position - this.transform.position; //vetor entre player
        RaycastHit hit;
        bool seeWall = false;
        Debug.DrawRay(this.transform.position, distance, Color.red); //verifica  se esta olhando para direção certa
        if(Physics.Raycast(this.transform.position, distance, out hit))
        {
            if(hit.collider.gameObject.tag == "wall") //verificar se é parede
            {
                seeWall = true;
            }
        }

        if (Task.isInspected)
        {
            Task.current.debugInfo = string.Format("wall={0}", seeWall); //apenas para teste
        }
        if(distance.magnitude < visibleRange && !seeWall)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    [Task]
    bool Turn(float angle) //método para "virar para olhar"
    {
        //Fazer movimentação angular:
        var p = this.transform.position + Quaternion.AngleAxis(angle, Vector3.up) * this.transform.forward;
        target = p;
        return true;
    }

    [Task]
    public bool IsHealthLessThan(float health) //passa parametro de vida
    {
        return this.health < health; //retorno da quantidade de vida
    }

    [Task]
    public bool Explode() //método para destruir personagem e barra de vida
    {
        Destroy(healthBar.gameObject);
        Destroy(this.gameObject);
        return true;
    }
}

