using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBehaviour : MonoBehaviour
{
    // Definir Hashes de:
    // Parametros (Speed, Attack, Damage, Dead)
    // Estados (Base Layer.Idle, Attack Layer.Idle, Attack Layer.Attack)

    private Animator animator;
    private Rigidbody rigidBody;
    private BoxCollider box;

    public int maxHP;
	public float walkSpeed		= 1;		// Parametro que define la velocidad de "caminar"
	public float runSpeed		= 1;		// Parametro que define la velocidad de "correr"
	public float rotateSpeed	= 160;		// Parametro que define la velocidad de "girar"

	// Variables auxiliares
	float _angularSpeed			= 0;		// Velocidad de giro actual
	float _speed				= 0;		// Velocidad de traslacion actual
	float _originalColliderZ	= 0;		// Valora original de la posición 'z' del collider

	// Variables internas:
	int _lives = 3;							// Vidas restantes
	public bool paused = false;				// Indica si el player esta pausado (congelado). Que no responde al Input
    private bool isPressingRunningButton = false; // Indica si el jugador está pulsando el boton Run del Gamepad.

    //Variables de AudioSource

    private AudioSource attackAudio;
    private AudioSource receiveDamageAudio;
    private AudioSource deathAudio;

	void Start()
	{
        // Obtener los componentes Animator, Rigidbody y el valor original center.z del BoxCollider
        animator = gameObject.GetComponent<Animator>();
        rigidBody = gameObject.GetComponent<Rigidbody>();
        _originalColliderZ = gameObject.GetComponent<BoxCollider>().center.z;
        box = gameObject.GetComponent<BoxCollider>();

        //Obtenemos las referencias de los audios
        attackAudio = gameObject.GetComponentsInChildren<AudioSource>()[0];
        receiveDamageAudio = gameObject.GetComponentsInChildren<AudioSource>()[1];
        deathAudio = gameObject.GetComponentsInChildren<AudioSource>()[2];


    }

	// Aqui moveremos y giraremos la araña en funcion del Input
	void FixedUpdate()
	{
		// Si estoy en pausa no hacer nada (no moverme ni atacar)
		if (paused) return;

        // Calculo de velocidad lineal (_speed) y angular (_angularSpeed) en función del Input
        // Si camino/corro hacia delante delante: _speed = walkSpeed   /  _speed = runSpeed
        if (Input.GetKey(KeyCode.UpArrow) || CrossButton.GetInput(InputType.UP))
        {
            if (Input.GetKey(KeyCode.LeftShift) || isPressingRunningButton)
                _speed = runSpeed;
            else
                _speed = walkSpeed;
        }
        // Si camino/corro hacia delante detras: _speed = -walkSpeed   /  _speed = -runSpeed
        else if (Input.GetKey(KeyCode.DownArrow) || CrossButton.GetInput(InputType.DOWN))
        {
            if (Input.GetKey(KeyCode.LeftShift) ||isPressingRunningButton)
                _speed = -runSpeed;
            else
                _speed = -walkSpeed;
        } else
            // Si no me muevo: _speed = 0
            _speed = 0;



        // Si giro izquierda: _angularSpeed = -rotateSpeed;
        if (Input.GetKey(KeyCode.LeftArrow) || CrossButton.GetInput(InputType.LEFT))
            _angularSpeed = -rotateSpeed;
        // Si giro derecha: _angularSpeed = rotateSpeed;
        else if (Input.GetKey(KeyCode.RightArrow) || CrossButton.GetInput(InputType.RIGHT))
        {
            _angularSpeed = rotateSpeed;
        }
        // Si no giro : _angularSpeed = 0;
        else
            _angularSpeed = 0;

        // Actualizamos el parámetro "Speed" en función de _speed. Para activar la anicación de caminar/correr
        animator.SetFloat("Speed", _speed);

        // Movemov y rotamos el rigidbody (MovePosition y MoveRotation) en función de "_speed" y "_angularSpeed"
        rigidBody.MovePosition(transform.position + (transform.forward * _speed * Time.deltaTime));
        rigidBody.MoveRotation(rigidBody.rotation * Quaternion.Euler(0f, _angularSpeed * Time.deltaTime, 0f));

        // Mover el collider en función del parámetro "Distance" (necesario cuando atacamos)
        box.center = new Vector3(box.center.x, box.center.y, animator.GetFloat("Distance") * _originalColliderZ * 20);
    }

	// En este bucle solamente comprobaremos si el Input nos indica "atacar" y activaremos el trigger "Attack"
	private void Update()
	{
        // Si estoy en pausa no hacer nada (no moverme ni atacar)
        if (paused) return;

        // Si detecto Input tecla/boton ataque ==> Activo disparados 'Attack'
        if (Input.GetKeyDown(KeyCode.Space)){
            attack();
        }
        
    }

    public void attack()
    {
        if (!paused)
        {
            //Nos aseguramos que la animación de atacar ha finalizado y podemos volver a atacar.
            if (!animator.GetCurrentAnimatorStateInfo(1).IsName("Attack"))
            {
                if (GameManager.instance.soundEnabled)
                    attackAudio.Play();
                animator.SetTrigger("Attack");
            }
        }
    }

    //Llamamos a esta función cuando el usuario presiona o deja de pulsar el botón Run
    public void  setRunning(bool pointerEnter)
    {
        if (isPressingRunningButton != pointerEnter)
            isPressingRunningButton = pointerEnter;
    }

	// Función para resetear el Player
	public void reset()
	{
        //Reiniciar el numero de vidas
        _lives = maxHP;

        // Pausamos a Player
        paused = true;
        
        // Forzar estado Idle en las dos capas (Base Layer y Attack Layer): función Play() de Animator
        animator.Play("Idle", 0);
        animator.Play("Idle", 1);

        // Reseteo todos los triggers (Attack y Dead)
        animator.ResetTrigger("Attack");
        animator.ResetTrigger("Dead");

        // Posicionar el jugador en el (0,0,0) y rotación nula (Quaternion.identity)
        gameObject.transform.position = new Vector3(0, 0, 0);
        gameObject.transform.rotation = Quaternion.identity;
    }

	// Funcion recibir daño
	public void recieveDamage()
	{
        if (paused) return;

        // Restar una vida
        _lives--;
        UIManager.instance.updateSpiderHPinHUD(_lives);
        // Si no me quedan vidas notificar al GameManager (notifyPlayerDead) y disparar trigger "Dead"
        if (_lives <= 0)
        {
            if (GameManager.instance.soundEnabled)
                deathAudio.Play();
            GameManager.instance.notifyPlayerDead();
            animator.SetTrigger("Dead");
        }

        // Si aun me quedan vidas dispara el trigger TakeDamage
        if (_lives > 0)
        {
            if (GameManager.instance.soundEnabled)
                receiveDamageAudio.Play();
            animator.SetTrigger("Damage");
        }
	}

	private void OnCollisionEnter(Collision collision)
	{
        // Obtener estado actual de la capa Attack Layer
        // Si el estado es 'Attack' matamos al enemigo (mirar etiqueta)
        if (animator.GetCurrentAnimatorStateInfo(1).IsName("Attack"))
            if (collision.gameObject.tag == "Enemy")
                collision.gameObject.GetComponent<SkeletonBehaviour>().kill(); // Matar quizás no es lo mismo que destruir
	}
}
