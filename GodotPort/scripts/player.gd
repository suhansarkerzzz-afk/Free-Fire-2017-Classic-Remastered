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

@onready var pivot: Node3D = $CameraPivot
@onready var spring_arm: SpringArm3D = $CameraPivot/SpringArm3D
@onready var camera: Camera3D = $CameraPivot/SpringArm3D/Camera3D

func _ready() -> void:
    add_to_group("player")
    health_changed.emit(health, MAX_HEALTH)
    ammo_changed.emit(ammo, reserve_ammo)
    camera.current = true

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
        died.emit()

func heal(amount: float) -> void:
    if not is_dead:
        health = minf(MAX_HEALTH, health + amount)
        health_changed.emit(health, MAX_HEALTH)
