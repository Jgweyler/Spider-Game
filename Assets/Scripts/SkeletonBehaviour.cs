using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkeletonBehaviour : MonoBehaviour
{
    // Definir Hashes de:
    // Parametros (Attack, Dead, Distance)
    // Estados (Attack, Idle)

    private Animator animator;
    private BoxCollider box;

	// Variables auxiliares 
	PlayerBehaviour _player		= null;     //Puntero a Player (establecido por método 'setPlayer')
	bool _dead					= false;	// Indica si ya he sido eliminado
	float _originalColliderZ	= 0;        // Valora original de la posición 'z' del collider
	float _timeToAttack			= 0;        // Periodo de ataque

    //Variables de audio

    private AudioSource attackAudio;
    private AudioSource deadAudio;

	public void setPlayer(PlayerBehaviour player)
	{
		_player = player;
	}
    void Awake()
    {
        animator = gameObject.GetComponent<Animator>();
    }
	void Start ()
	{
        // Obtener los componentes Animator y el valor original center.z del BoxCollider
        
        _originalColliderZ = gameObject.GetComponent<BoxCollider>().center.z;
        box = gameObject.GetComponent<BoxCollider>();

        attackAudio = GetComponentsInChildren<AudioSource>()[0];
        deadAudio = GetComponentsInChildren<AudioSource>()[1];

    }

    void FixedUpdate ()
	{
        // Si estoy muerto ==> No hago nada
        if (_dead) return;

        Vector3 distance = _player.gameObject.transform.position - transform.position;
        float distance_f = Vector3.SqrMagnitude(distance);

        // Si Player esta a menos de 1m de mi y no estoy muerto:
        if (distance_f <= 1 && !_dead)
        {
            transform.LookAt(_player.gameObject.transform);
            // - Si ha pasado 1s o más desde el ultimo ataque ==> attack()
            if (_timeToAttack >= 2.22)
                attack();
        }
        // Desplazar el collider en 'z' un multiplo del parametro Distance
        box.center = new Vector3(box.center.x, box.center.y, animator.GetFloat("Distance") * _originalColliderZ * 20);
        _timeToAttack += Time.deltaTime;
    }

	public void attack()
	{
        if (GameManager.instance.soundEnabled)
            attackAudio.PlayDelayed(0.7f);
        // Activo el trigger "Attack"
        animator.SetTrigger("Attack");
        _timeToAttack = 0;
	}

	public void kill()
	{
        if (GameManager.instance.soundEnabled)
            deadAudio.Play();
        // Guardo que estoy muerto, disparo trigger "Dead" y desactivo el collider
        _dead = true;
        animator.SetTrigger("Dead");
        gameObject.GetComponent<BoxCollider>().enabled = false;

        // Notifico al GameManager que he sido eliminado
        GameManager.instance.notifyEnemyKilled(this);
	}

	// Funcion para resetear el collider (activado por defecto), la variable donde almaceno si he muerto y forzar el estado "Idle" en Animator
	public void reset()
	{
        gameObject.GetComponent<BoxCollider>().enabled = true;
        _dead = false;
        animator.Play("Idle", 0);
    }

	private void OnCollisionEnter(Collision collision)
	{
        // Obtener el estado actual
        // Si el estado es 'Attack' y el parametro Distance es > 0 atacamos a Player (comprobar etiqueta).
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Attack") && animator.GetFloat("Distance") > 0)
        {
            if (collision.gameObject.tag == "Player")
                _player.recieveDamage();
        }

        // La Distancia >0 es para acotar el ataque sólo al momento que mueve la espada (no toda la animación).
    }
}
