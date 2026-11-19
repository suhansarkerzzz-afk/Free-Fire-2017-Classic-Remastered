extends CharacterBody3D

signal health_changed(current: float, maximum: float)
signal ammo_changed(current: int, reserve: int)
signal died

const MAX_HEALTH := 200.0
const MOVE_SPEED := 5.4
const SPRINT_SPEED := 8.2
const JUMP_VELOCITY := 5.2
const GRAVITY := 18.0
const MAGAZINE_SIZE := 30

const AIRDROP_GROUNDED := 0
const AIRDROP_IN_PLANE := 1
const AIRDROP_FREEFALL := 2
const AIRDROP_PARACHUTE := 3
const AIRDROP_GRAVITY := 18.0
const FREEFALL_TERMINAL_SPEED := 18.0
const PARACHUTE_DESCENT_SPEED := 4.6
const AIRDROP_STEER_SPEED := 4.8
const PARACHUTE_STEER_SPEED := 3.6

var health := MAX_HEALTH
var ammo := MAGAZINE_SIZE
var reserve_ammo := 120
var move_input := Vector2.ZERO
var sprinting := false
var firing := false
var fire_cooldown := 0.0
var reload_timer := 0.0
var is_dead := false
var is_in_vehicle := false
var airdrop_state := AIRDROP_GROUNDED
var parachute_visual: Node3D

@onready var pivot: Node3D = $CameraPivot
@onready var spring_arm: SpringArm3D = $CameraPivot/SpringArm3D
@onready var camera: Camera3D = $CameraPivot/SpringArm3D/Camera3D

func _ready() -> void:
    add_to_group("player")
    _build_parachute_visual()
    health_changed.emit(health, MAX_HEALTH)
    ammo_changed.emit(ammo, reserve_ammo)
    camera.current = true

func _build_parachute_visual() -> void:
    parachute_visual = Node3D.new()
    parachute_visual.name = "ParachuteVisual"
    parachute_visual.visible = false
    add_child(parachute_visual)

    var canopy_material := StandardMaterial3D.new()
    canopy_material.albedo_color = Color(0.82, 0.12, 0.08)
    canopy_material.roughness = 0.88

    var canopy := MeshInstance3D.new()
    var canopy_mesh := CylinderMesh.new()
    canopy_mesh.top_radius = 0.32
    canopy_mesh.bottom_radius = 2.75
    canopy_mesh.height = 0.82
    canopy_mesh.radial_segments = 14
    canopy.mesh = canopy_mesh
    canopy.material_override = canopy_material
    canopy.position = Vector3(0, 5.0, 0.25)
    parachute_visual.add_child(canopy)

    var cord_material := StandardMaterial3D.new()
    cord_material.albedo_color = Color(0.92, 0.91, 0.82)
    cord_material.shading_mode = BaseMaterial3D.SHADING_MODE_UNSHADED

    var cord_mesh := ImmediateMesh.new()
    cord_mesh.surface_begin(Mesh.PRIMITIVE_LINES, cord_material)
    cord_mesh.surface_add_vertex(Vector3(-2.35, 4.62, 0.25))
    cord_mesh.surface_add_vertex(Vector3(-0.34, 1.08, 0.0))
    cord_mesh.surface_add_vertex(Vector3(2.35, 4.62, 0.25))
    cord_mesh.surface_add_vertex(Vector3(0.34, 1.08, 0.0))
    cord_mesh.surface_add_vertex(Vector3(0.0, 4.62, -2.10))
    cord_mesh.surface_add_vertex(Vector3(0.0, 1.08, -0.34))
    cord_mesh.surface_add_vertex(Vector3(0.0, 4.62, 2.10))
    cord_mesh.surface_add_vertex(Vector3(0.0, 1.08, 0.34))
    cord_mesh.surface_end()

    var cords := MeshInstance3D.new()
    cords.name = "ParachuteLines"
    cords.mesh = cord_mesh
    parachute_visual.add_child(cords)

func board_airdrop_plane(board_position: Vector3) -> void:
    if is_dead or is_in_vehicle:
        return
    airdrop_state = AIRDROP_IN_PLANE
    global_position = board_position
    velocity = Vector3.ZERO
    move_input = Vector2.ZERO
    firing = false
    if is_instance_valid(parachute_visual):
        parachute_visual.hide()

func force_jump_from_plane() -> void:
    if airdrop_state != AIRDROP_IN_PLANE:
        return
    airdrop_state = AIRDROP_FREEFALL
    velocity = Vector3(0.0, -2.0, 0.0)

func handle_jump_action() -> void:
    if is_dead or is_in_vehicle:
        return
    match airdrop_state:
        AIRDROP_IN_PLANE:
            force_jump_from_plane()
        AIRDROP_FREEFALL:
            deploy_parachute()
        AIRDROP_PARACHUTE:
            return
        _:
            if is_on_floor():
                velocity.y = JUMP_VELOCITY

func deploy_parachute() -> void:
    if airdrop_state != AIRDROP_FREEFALL:
        return
    airdrop_state = AIRDROP_PARACHUTE
    velocity.y = -PARACHUTE_DESCENT_SPEED
    if is_instance_valid(parachute_visual):
        parachute_visual.show()

func _physics_airdrop(delta: float) -> void:
    var input_vector := move_input
    if not OS.has_feature("mobile"):
        input_vector = Input.get_vector("move_left", "move_right", "move_forward", "move_back")

    var forward := -pivot.global_basis.z
    var right := pivot.global_basis.x
    var direction := right * input_vector.x + forward * -input_vector.y
    direction.y = 0.0
    direction = direction.normalized()

    var horizontal_speed := AIRDROP_STEER_SPEED if airdrop_state == AIRDROP_FREEFALL else PARACHUTE_STEER_SPEED
    velocity.x = direction.x * horizontal_speed
    velocity.z = direction.z * horizontal_speed

    if airdrop_state == AIRDROP_FREEFALL:
        velocity.y = maxf(velocity.y - AIRDROP_GRAVITY * delta, -FREEFALL_TERMINAL_SPEED)
    else:
        velocity.y = move_toward(velocity.y, -PARACHUTE_DESCENT_SPEED, AIRDROP_GRAVITY * delta)

    move_and_slide()

    if direction.length_squared() > 0.01:
        rotation.y = lerp_angle(rotation.y, atan2(-direction.x, -direction.z), delta * 5.0)

    if is_on_floor():
        airdrop_state = AIRDROP_GROUNDED
        velocity.y = -0.5
        if is_instance_valid(parachute_visual):
            parachute_visual.hide()

func set_move(value: Vector2) -> void:
    move_input = value.limit_length(1.0)

func look_by(delta_pixels: Vector2) -> void:
    pivot.rotate_y(-delta_pixels.x * 0.004)
    pivot.rotation.x = clampf(pivot.rotation.x - delta_pixels.y * 0.003, deg_to_rad(-48), deg_to_rad(35))

func _physics_process(delta: float) -> void:
    if is_dead:
        return
    if is_in_vehicle:
        velocity = Vector3.ZERO
        return
    if airdrop_state == AIRDROP_IN_PLANE:
        if not OS.has_feature("mobile") and Input.is_action_just_pressed("jump"):
            force_jump_from_plane()
        velocity = Vector3.ZERO
        return
    if airdrop_state == AIRDROP_FREEFALL or airdrop_state == AIRDROP_PARACHUTE:
        if not OS.has_feature("mobile") and Input.is_action_just_pressed("jump"):
            handle_jump_action()
        firing = false
        _physics_airdrop(delta)
        return
    fire_cooldown = maxf(0.0, fire_cooldown - delta)
    if reload_timer > 0.0:
        reload_timer -= delta
        if reload_timer <= 0.0:
            var loaded := mini(MAGAZINE_SIZE - ammo, reserve_ammo)
            ammo += loaded
            reserve_ammo -= loaded
            ammo_changed.emit(ammo, reserve_ammo)
    if firing and reload_timer <= 0.0 and fire_cooldown <= 0.0:
        shoot()
    var input_vector := move_input
    if not OS.has_feature("mobile"):
        input_vector = Input.get_vector("move_left", "move_right", "move_forward", "move_back")
        sprinting = Input.is_action_pressed("sprint")
        if Input.is_action_just_pressed("jump") and is_on_floor():
            velocity.y = JUMP_VELOCITY
        if Input.is_action_just_pressed("reload"):
            reload_weapon()
        if Input.is_action_just_pressed("fire"):
            firing = true
        if Input.is_action_just_released("fire"):
            firing = false
    var forward := -pivot.global_basis.z
    var right := pivot.global_basis.x
    var direction := (right * input_vector.x + forward * -input_vector.y)
    direction.y = 0.0
    direction = direction.normalized()
    var speed := SPRINT_SPEED if sprinting else MOVE_SPEED
    velocity.x = direction.x * speed
    velocity.z = direction.z * speed
    if not is_on_floor():
        velocity.y -= GRAVITY * delta
    elif velocity.y < 0.0:
        velocity.y = -0.5
    move_and_slide()
    if direction.length_squared() > 0.01:
        var face := Vector3(direction.x, 0, direction.z)
        rotation.y = lerp_angle(rotation.y, atan2(-face.x, -face.z), delta * 10.0)

func shoot() -> void:
    if is_dead or airdrop_state != AIRDROP_GROUNDED:
        return
    if ammo <= 0:
        reload_weapon()
        return
    ammo -= 1
    fire_cooldown = 0.18
    ammo_changed.emit(ammo, reserve_ammo)
    var from := camera.global_position
    var to := from + (-camera.global_basis.z * 65.0)
    var query := PhysicsRayQueryParameters3D.create(from, to)
    query.exclude = [get_rid()]
    var hit := get_world_3d().direct_space_state.intersect_ray(query)
    if not hit.is_empty():
        var target: Object = hit.get("collider")
        if target and target.has_method("take_damage"):
            target.take_damage(30.0)

func reload_weapon() -> void:
    if reload_timer > 0.0 or ammo >= MAGAZINE_SIZE or reserve_ammo <= 0:
        return
    reload_timer = 1.5

func take_damage(amount: float) -> void:
    if is_dead:
        return
    health = maxf(0.0, health - amount)
    health_changed.emit(health, MAX_HEALTH)
    if health <= 0.0:
        is_dead = true
        if is_instance_valid(parachute_visual):
            parachute_visual.hide()
        died.emit()

func heal(amount: float) -> void:
    if not is_dead:
        health = minf(MAX_HEALTH, health + amount)
        health_changed.emit(health, MAX_HEALTH)
