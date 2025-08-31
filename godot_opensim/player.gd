extends CharacterBody3D

const SPEED = 5.0
const MOUSE_SENSITIVITY = 0.002

@onready var camera = $SpringArm3D/Camera3D
@onready var spring_arm = $SpringArm3D

func _enter_tree():
	if is_multiplayer_authority():
		camera.current = true
		Input.set_mouse_mode(Input.MOUSE_MODE_CAPTURED)

func _unhandled_input(event):
	if not is_multiplayer_authority():
		return

	if event is InputEventMouseButton and event.button_index == MOUSE_BUTTON_LEFT and Input.get_mouse_mode() == Input.MOUSE_MODE_VISIBLE:
		Input.set_mouse_mode(Input.MOUSE_MODE_CAPTURED)
	elif event is InputEventKey and event.keycode == KEY_ESCAPE and Input.get_mouse_mode() == Input.MOUSE_MODE_CAPTURED:
		Input.set_mouse_mode(Input.MOUSE_MODE_VISIBLE)

	if event is InputEventMouseMotion and Input.get_mouse_mode() == Input.MOUSE_MODE_CAPTURED:
		rotate_y(-event.relative.x * MOUSE_SENSITIVITY)
		spring_arm.rotate_x(-event.relative.y * MOUSE_SENSITIVITY)
		spring_arm.rotation.x = clamp(spring_arm.rotation.x, -PI/2, PI/2)

func _physics_process(delta):
	# Only process movement for the local player.
	if not is_multiplayer_authority():
		return

	var input_dir = Input.get_vector("move_left", "move_right", "move_forward", "move_backward")
	var direction = (transform.basis * Vector3(input_dir.x, 0, input_dir.y)).normalized()
	if direction:
		velocity.x = direction.x * SPEED
		velocity.z = direction.z * SPEED
	else:
		velocity.x = move_toward(velocity.x, 0, SPEED)
		velocity.z = move_toward(velocity.z, 0, SPEED)

	move_and_slide()
