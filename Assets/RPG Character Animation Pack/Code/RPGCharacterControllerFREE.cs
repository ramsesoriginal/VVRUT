using UnityEngine;
using System.Collections;

public enum Weapon{
	UNARMED = 0,
	RELAX = 8
}

//Script requires Rigidbody and NavMeshAgent components.
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(UnityEngine.AI.NavMeshAgent))]
public class RPGCharacterControllerFREE : MonoBehaviour{
	
	#region Variables

	//Components.
	Rigidbody rb;
	protected Animator animator;
	public GameObject target;
	[HideInInspector]
	public Vector3 targetDashDirection;
	public Camera sceneCamera;
	private UnityEngine.AI.NavMeshAgent agent;

	//Movement.
	[HideInInspector]
	public bool canMove = true;
	public float walkSpeed = 1.35f;
	float moveSpeed;
	public float runSpeed = 6f;
	float rotationSpeed = 40f;
	Vector3 inputVec;
	Vector3 newVelocity;
	public float slopeAmount = 0.5f;
	public bool onAllowableSlope;

	//Navmesh.
	public bool useNavMesh = false;
	private float navMeshSpeed;
	public Transform goal;

	//Jumping.
	public float gravity = -9.8f;
	[HideInInspector]
	public bool canJump;
	bool isJumping = false;
	[HideInInspector]
	public bool isGrounded;
	public float jumpSpeed = 12;
	public float doublejumpSpeed = 12;
	bool doublejumping = true;
	[HideInInspector]
	public bool canDoubleJump = false;
	[HideInInspector]
	public bool isDoubleJumping = false;
	bool doublejumped = false;
	bool isFalling;
	bool startFall;
	float fallingVelocity = -1f;
	float fallTimer = 0f;
	public float fallDelay = 0.2f;
	float distanceToGround;

	//Movement in the air.
	public float inAirSpeed = 8f;
	float maxVelocity = 2f;
	float minVelocity = -2f;

	//Rolling.
	public float rollSpeed = 8;
	bool isRolling = false;
	public float rollduration;

	//Weapon and Shield
	[HideInInspector]
	public Weapon weapon;
	int rightWeapon = 0;
	int leftWeapon = 0;
	[HideInInspector]
	public bool isRelax = false;

	//Strafing / Actions.
	[HideInInspector]
	public bool canAction = true;
	[HideInInspector]
	public bool isStrafing = false;
	[HideInInspector]
	public bool isDead = false;
	public float knockbackMultiplier = 1f;
	bool isKnockback;

	//Input variables.
	float inputHorizontal = 0f;
	float inputVertical = 0f;
	float inputDashVertical = 0f;
	float inputDashHorizontal = 0f;
	float inputBlock = 0f;
	bool inputLightHit;
	bool inputDeath;
	bool inputAttackR;
	bool inputAttackL;
	bool inputCastL;
	bool inputCastR;
	bool inputJump;

	#endregion

	#region Initialization and Inputs

	void Start(){
		//Find the Animator component.
		if(animator = GetComponentInChildren<Animator>()){
		}
		else{
			Debug.LogError("ERROR: There is no animator for character.");
			Destroy(this);
		}
		//Use MainCamera if no camera is selected.
		if(sceneCamera == null){
			sceneCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
			if(sceneCamera == null){
				Debug.LogError("ERROR: There is no camera in scene.");
				Destroy(this);
			}
		}
		rb = GetComponent<Rigidbody>();
		agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
		agent.enabled = false;
	}

	/// <summary>
	/// Input abstraction for easier asset updates using outside control schemes.
	/// </summary>
	void Inputs(){
		inputDashHorizontal = Input.GetAxisRaw("DashHorizontal");
		inputDashVertical = Input.GetAxisRaw("DashVertical");
		inputHorizontal = Input.GetAxisRaw("Horizontal");
		inputVertical = Input.GetAxisRaw("Vertical");
		inputLightHit = Input.GetButtonDown("LightHit");
		inputDeath = Input.GetButtonDown("Death");
		inputAttackL = Input.GetButtonDown("AttackL");
		inputAttackR = Input.GetButtonDown("AttackR");
		inputCastL = Input.GetButtonDown("CastL");
		inputCastR = Input.GetButtonDown("CastR");
		inputBlock = Input.GetAxisRaw("TargetBlock");
		inputJump = Input.GetButtonDown("Jump");
	}

	#endregion

	#region Updates

	void Update(){
		Inputs();
		if(canMove && !isDead && !useNavMesh){
			CameraRelativeMovement();
		} 
		Rolling();
		Jumping();
		if(inputLightHit && canAction && isGrounded){
			GetHit();
		}
		if(inputDeath && canAction && isGrounded){
			if(!isDead){
				Death();
			}
			else{
				Revive();
			}
		}
		if(inputAttackL && canAction && isGrounded){
			Attack(1);
		}
		if(inputAttackR && canAction && isGrounded){
			Attack(2);
		}
		if(inputCastL && canAction && isGrounded && !isStrafing){
			AttackKick(1);
		}
		if(inputCastR && canAction && isGrounded && !isStrafing){
			AttackKick(2);
		}
		//Strafing.
		if((Input.GetKey(KeyCode.LeftShift) || inputBlock > 0.1f) && canAction){  
			isStrafing = true;
			animator.SetBool("Strafing", true);
		}
		else{
			isStrafing = false;
			animator.SetBool("Strafing", false);
		}
		//Navmesh.
		if(useNavMesh){
			agent.enabled = true;
			navMeshSpeed = agent.velocity.magnitude;
		}
		else{
			agent.enabled = false;
		}
		//Navigate to click.
		if(Input.GetMouseButtonDown(0))
		{
			if(useNavMesh)
			{
				RaycastHit hit;
				if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 100)) {
					agent.destination = hit.point;
				}
			}
		}
		//Slow time.
		if(Input.GetKeyDown(KeyCode.T)){
			if(Time.timeScale != 1){
				Time.timeScale = 1;
			}
			else{
				Time.timeScale = 0.15f;
			}
		}
		//Pause.
		if(Input.GetKeyDown(KeyCode.P)){
			if(Time.timeScale != 1){
				Time.timeScale = 1;
			}
			else{
				Time.timeScale = 0f;
			}
		}
	}

	void FixedUpdate(){
		CheckForGrounded();
		//Apply gravity.
		rb.AddForce(0, gravity, 0, ForceMode.Acceleration);
		AirControl();
		//Check if character can move.
		if(canMove && !isDead){
			moveSpeed = UpdateMovement();  
		}
		//Check if falling.
		if(rb.velocity.y < fallingVelocity && useNavMesh != true){
			isFalling = true;
			animator.SetInteger("Jumping", 2);
			canJump = false;
		}
		else{
			isFalling = false;
		}
	}

	/// <summary>
	/// Get velocity of rigid body and pass the value to the animator to control the animations.
	/// </summary>
	void LateUpdate(){
		if(!useNavMesh){
			//Get local velocity of charcter
			float velocityXel = transform.InverseTransformDirection(rb.velocity).x;
			float velocityZel = transform.InverseTransformDirection(rb.velocity).z;
			//Update animator with movement values
			animator.SetFloat("Velocity X", velocityXel / runSpeed);
			animator.SetFloat("Velocity Z", velocityZel / runSpeed);
			//if character is alive and can move, set our animator
			if(!isDead && canMove){
				if(moveSpeed > 0){
					animator.SetBool("Moving", true);
				}
				else{
					animator.SetBool("Moving", false);
				}
			}
		}
		else{
			animator.SetFloat("Velocity X", agent.velocity.sqrMagnitude);
			animator.SetFloat("Velocity Z", agent.velocity.sqrMagnitude);
			if(navMeshSpeed > 0){
				animator.SetBool("Moving", true);
			}
			else{
				animator.SetBool("Moving", false);
			}
		}
	}

	#endregion

	#region UpdateMovement

	/// <summary>
	/// Movement based off camera facing.
	/// </summary>
	void CameraRelativeMovement(){
		//converts control input vectors into camera facing vectors.
		Transform cameraTransform = sceneCamera.transform;
		//Forward vector relative to the camera along the x-z plane.
		Vector3 forward = cameraTransform.TransformDirection(Vector3.forward);
		forward.y = 0;
		forward = forward.normalized;
		//Right vector relative to the camera always orthogonal to the forward vector.
		Vector3 right = new Vector3(forward.z, 0, -forward.x);
		if(!isRolling){
			targetDashDirection = inputDashHorizontal * right + inputDashVertical * -forward;
		}
		inputVec = inputHorizontal * right + inputVertical * forward;
	}

	/// <summary>
	/// Rotate character towards movement direction.
	/// </summary>
	void RotateTowardsMovementDir(){
		if(inputVec != Vector3.zero && !isStrafing && !isRolling){
			transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(inputVec), Time.deltaTime * rotationSpeed);
		}
	}

	/// <summary>
	/// Applies velocity to rigidbody to move the character, and controls rotation if not targetting.
	/// </summary>
	/// <returns>The movement.</returns>
	float UpdateMovement(){
		if(!useNavMesh){
			CameraRelativeMovement();
		}
		Vector3 motion = inputVec;
		if(isGrounded){
			//Reduce input for diagonal movement.
			if(motion.magnitude > 1){
				motion.Normalize();
			}
			if(canMove){
				//Set speed by walking / running.
				if(isStrafing){
					newVelocity = motion * walkSpeed;
				}
				else{
					newVelocity = motion * runSpeed;
				}
				//Rolling uses rolling speed and direction.
				if(isRolling){
					//Force the dash movement to 1.
					targetDashDirection.Normalize();
					newVelocity = rollSpeed * targetDashDirection;
				}
			}
		}
		else{
			//If falling, use momentum.
			newVelocity = rb.velocity;
		}
		if(!isStrafing || !canMove){
			RotateTowardsMovementDir();
		}
		if(isStrafing && !isRelax){
			//Make character face target.
			Quaternion targetRotation;
			Vector3 targetPos = target.transform.position;
			targetRotation = Quaternion.LookRotation(targetPos - new Vector3(transform.position.x, 0, transform.position.z));
			transform.eulerAngles = Vector3.up * Mathf.MoveTowardsAngle(transform.eulerAngles.y, targetRotation.eulerAngles.y, (rotationSpeed * Time.deltaTime) * rotationSpeed);
		}
		newVelocity.y = rb.velocity.y;
		rb.velocity = newVelocity;
		//Return movement value for Animator component.
		return inputVec.magnitude;
	}

	#endregion

	#region Jumping

	/// <summary>
	/// Checks if character is within a certain distance from the ground, and markes it IsGrounded.
	/// </summary>
	void CheckForGrounded(){
		RaycastHit hit;
		Vector3 offset = new Vector3(0, 0.1f, 0);
		if(Physics.Raycast((transform.position + offset), -Vector3.up, out hit, 100f)){
			distanceToGround = hit.distance;
			if(distanceToGround < slopeAmount){
				isGrounded = true;
				if(!isJumping){
					canJump = true;
				}
				startFall = false;
				doublejumped = false;
				canDoubleJump = false;
				isFalling = false;
				fallTimer = 0;
				if(!isJumping){
					animator.SetInteger("Jumping", 0);
				}
			}
			else{
				fallTimer += 0.009f;
				if(fallTimer >= fallDelay){
					isGrounded = false;
				}
			}
		}
	}

	void Jumping(){
		if(isGrounded){
			if(canJump && inputJump){
				StartCoroutine(_Jump());
			}
		}
		else{    
			canDoubleJump = true;
			canJump = false;
			if(isFalling){
				//Set the animation back to falling.
				animator.SetInteger("Jumping", 2);
				//Prevent from going into land animation while in air.
				if(!startFall){
					animator.SetTrigger("JumpTrigger");
					startFall = true;
				}
			}
			if(canDoubleJump && doublejumping && inputJump && !doublejumped && isFalling){
				//Apply the current movement to launch velocity.
				rb.velocity += doublejumpSpeed * Vector3.up;
				animator.SetInteger("Jumping", 3);
				doublejumped = true;
			}
		}
	}

	public IEnumerator _Jump(){
		isJumping = true;
		animator.SetInteger("Jumping", 1);
		animator.SetTrigger("JumpTrigger");
		//Apply the current movement to launch velocity.
		rb.velocity += jumpSpeed * Vector3.up;
		canJump = false;
		yield return new WaitForSeconds(.5f);
		isJumping = false;
	}

	/// <summary>
	/// Controls movement of character while in the air.
	/// </summary>
	void AirControl(){
		if(!isGrounded){
			CameraRelativeMovement();
			Vector3 motion = inputVec;
			motion *= (Mathf.Abs(inputVec.x) == 1 && Mathf.Abs(inputVec.z) == 1) ? 0.7f : 1;
			rb.AddForce(motion * inAirSpeed, ForceMode.Acceleration);
			//Limit the amount of velocity character can achieve.
			float velocityX = 0;
			float velocityZ = 0;
			if(rb.velocity.x > maxVelocity){
				velocityX = GetComponent<Rigidbody>().velocity.x - maxVelocity;
				if(velocityX < 0){
					velocityX = 0;
				}
				rb.AddForce(new Vector3(-velocityX, 0, 0), ForceMode.Acceleration);
			}
			if(rb.velocity.x < minVelocity){
				velocityX = rb.velocity.x - minVelocity;
				if(velocityX > 0){
					velocityX = 0;
				}
				rb.AddForce(new Vector3(-velocityX, 0, 0), ForceMode.Acceleration);
			}
			if(rb.velocity.z > maxVelocity){
				velocityZ = rb.velocity.z - maxVelocity;
				if(velocityZ < 0){
					velocityZ = 0;
				}
				rb.AddForce(new Vector3(0, 0, -velocityZ), ForceMode.Acceleration);
			}
			if(rb.velocity.z < minVelocity){
				velocityZ = rb.velocity.z - minVelocity;
				if(velocityZ > 0){
					velocityZ = 0;
				}
				rb.AddForce(new Vector3(0, 0, -velocityZ), ForceMode.Acceleration);
			}
		}
	}

	#endregion

	#region Actions

	//0 = No side.
	//1 = Left.
	//2 = Right.
	public void Attack(int attackSide){
		if(canAction && isGrounded){
			if(weapon == Weapon.UNARMED){
				int maxAttacks = 3;
				int attackNumber = 0;
				if(attackSide == 1 || attackSide == 3){
					attackNumber = Random.Range(3, maxAttacks);
				}
				else if(attackSide == 2){
					attackNumber = Random.Range(6, maxAttacks + 3);
				}
				if(attackSide != 3){
					animator.SetInteger("Action", attackNumber);
					if(leftWeapon == 12 || leftWeapon == 14 || rightWeapon == 13 || rightWeapon == 15){
						StartCoroutine(_LockMovementAndAttack(0, .75f));
					}
					else{
						StartCoroutine(_LockMovementAndAttack(0, .6f));
					}
				}
				else{
					StartCoroutine(_LockMovementAndAttack(0, .75f));
				}
			}
			animator.SetTrigger("AttackTrigger");
		}
	}

	public void AttackKick(int kickSide){
		if(isGrounded){
			animator.SetInteger("Action", kickSide);
			animator.SetTrigger("AttackKickTrigger");
			StartCoroutine(_LockMovementAndAttack(0, .8f));
		}
	}

	public void GetHit(){
		int hits = 5;
		int hitNumber = Random.Range(1, hits + 1);
		animator.SetInteger("Action", hitNumber);
		animator.SetTrigger("GetHitTrigger");
		StartCoroutine(_LockMovementAndAttack(.1f, .4f));
		//Apply directional knockback force.
		if(hitNumber <= 2){
			StartCoroutine(_Knockback(-transform.forward, 8, 4));
		}
		else if(hitNumber == 3){
			StartCoroutine(_Knockback(-transform.right, 8, 4));
		}
		else if(hitNumber == 4){
			StartCoroutine(_Knockback(transform.forward, 8, 4));
		}
		else if(hitNumber == 5){
			StartCoroutine(_Knockback(transform.right, 8, 4));
		}
	}

	IEnumerator _Knockback(Vector3 knockDirection, int knockBackAmount, int variableAmount){
		isKnockback = true;
		StartCoroutine(_KnockbackForce(knockDirection, knockBackAmount, variableAmount));
		yield return new WaitForSeconds(.1f);
		isKnockback = false;
	}

	IEnumerator _KnockbackForce(Vector3 knockDirection, int knockBackAmount, int variableAmount){
		while(isKnockback){
			rb.AddForce(knockDirection * ((knockBackAmount + Random.Range(-variableAmount, variableAmount)) * (knockbackMultiplier * 10)), ForceMode.Impulse);
			yield return null;
		}
	}

	#endregion

	#region Rolling

	void Rolling(){
		if(!isRolling && isGrounded){
			if(Input.GetAxis("DashVertical") > 0.5f || Input.GetAxis("DashVertical") < -0.5f || Input.GetAxis("DashHorizontal") > 0.5f || Input.GetAxis("DashHorizontal") < -0.5f){
				StartCoroutine(_DirectionalRoll());
			}
		}
	}

	public IEnumerator _DirectionalRoll(){
		//Check which way the dash is pressed relative to the character facing.
		float angle = Vector3.Angle(targetDashDirection, -transform.forward);
		float sign = Mathf.Sign(Vector3.Dot(transform.up, Vector3.Cross(targetDashDirection, transform.forward)));
		//Angle in [-179,180].
		float signed_angle = angle * sign;
		//Angle in 0-360.
		float angle360 = (signed_angle + 180) % 360;
		//Deternime the animation to play based on the angle.
		if(angle360 > 315 || angle360 < 45){
			StartCoroutine(_Roll(1));
		}
		if(angle360 > 45 && angle360 < 135){
			StartCoroutine(_Roll(2));
		}
		if(angle360 > 135 && angle360 < 225){
			StartCoroutine(_Roll(3));
		}
		if(angle360 > 225 && angle360 < 315){
			StartCoroutine(_Roll(4));
		}
		yield return null;
	}

	/// <summary>
	/// Character Roll.
	/// </summary>
	/// <param name="1">Forward.</param>
	/// <param name="2">Right.</param>
	/// <param name="3">Backward.</param>
	/// <param name="4">Left.</param>
	public IEnumerator _Roll(int rollNumber){
		if(weapon == Weapon.RELAX){
			weapon = Weapon.UNARMED;
			animator.SetInteger("Weapon", 0);
		}
		animator.SetInteger("Action", rollNumber);
		animator.SetTrigger("RollTrigger");
		isRolling = true;
		canAction = false;
		yield return new WaitForSeconds(rollduration);
		isRolling = false;
		canAction = true;
	}

	#endregion

	#region Misc

	public void Death(){
		animator.SetInteger("Action", 1);
		animator.SetTrigger("DeathTrigger");
		StartCoroutine(_LockMovementAndAttack(.1f, 1.5f));
		isDead = true;
		animator.SetBool("Moving", false);
		inputVec = new Vector3(0, 0, 0);
	}

	public void Revive(){
		animator.SetInteger("Action", 1);
		animator.SetTrigger("ReviveTrigger");
		isDead = false;
	}

	//Kkeep character from moveing while attacking, etc.
	public IEnumerator _LockMovementAndAttack(float delayTime, float lockTime){
		yield return new WaitForSeconds(delayTime);
		canAction = false;
		canMove = false;
		animator.SetBool("Moving", false);
		rb.velocity = Vector3.zero;
		rb.angularVelocity = Vector3.zero;
		inputVec = new Vector3(0, 0, 0);
		animator.applyRootMotion = true;
		yield return new WaitForSeconds(lockTime);
		canAction = true;
		canMove = true;
		animator.applyRootMotion = false;
	}

	//Placeholder functions for Animation events.
	public void Hit(){
	}

	public void Shoot(){
	}

	public void FootR(){
	}

	public void FootL(){
	}

	public void Land(){
	}

	public void Jump(){
	}

	#endregion
}