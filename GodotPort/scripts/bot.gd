extends CharacterBody3D

signal died

var target: Node3D
var health := 200.0
var attack_timer := 0.0
var move_speed := 4.0
var dead := false
var strafe_phase := 0.0

const GRAVITY := 18.0
const SEPARATION_RADIUS := 2.8
const SEPARATION_STRENGTH := 2.2
const MAX_SIMULTANEOUS_ATTACKERS := 3

func _ready() -> void:
    add_to_group("bots")
    strafe_phase = randf_range(0.0, TAU)

func _physics_process(delta: float) -> void:
    if dead or not is_instance_valid(target):
        velocity = Vector3.ZERO
        return

    attack_timer = maxf(0.0, attack_timer - delta)
    var offset := target.global_position - global_position
    offset.y = 0.0
    var distance := offset.length()
    var direction := Vector3.ZERO

    if distance > 2.7:
        direction = offset.normalized()
    else:
        velocity.x = move_toward(velocity.x, 0.0, 14.0 * delta)
        velocity.z = move_toward(velocity.z, 0.0, 14.0 * delta)

    # Push bots away from nearby teammates so they do not form one solid pile.
    var separation := _get_separation()
    if direction != Vector3.ZERO:
        var side := Vector3(-direction.z, 0.0, direction.x) * sin(Time.get_ticks_msec() * 0.0015 + strafe_phase) * 0.10
        var movement := (direction + side + separation * SEPARATION_STRENGTH).normalized()
        velocity.x = movement.x * move_speed
        velocity.z = movement.z * move_speed
        rotation.y = lerp_angle(rotation.y, atan2(-movement.x, -movement.z), delta * 7.0)
    elif separation.length_squared() > 0.01:
        velocity.x = separation.x * move_speed * 0.65
        velocity.z = separation.z * move_speed * 0.65

    # Limit melee pressure: only the three nearest/oldest nearby bots can hit at once.
    if distance <= 3.15 and attack_timer <= 0.0 and _is_attack_slot_available():
        if target.has_method("take_damage"):
            target.take_damage(8.0)
        attack_timer = 0.85

    if not is_on_floor():
        velocity.y -= GRAVITY * delta
    else:
        velocity.y = -0.5
    move_and_slide()

func _get_separation() -> Vector3:
    var push := Vector3.ZERO
    var neighbours := get_tree().get_nodes_in_group("bots")
    for other in neighbours:
        if other == self or not is_instance_valid(other):
            continue
        var away: Vector3 = global_position - other.global_position
        away.y = 0.0
        var distance := away.length()
        if distance > 0.01 and distance < SEPARATION_RADIUS:
            push += away.normalized() * ((SEPARATION_RADIUS - distance) / SEPARATION_RADIUS)
    return push.limit_length(1.0)

func _is_attack_slot_available() -> bool:
    var ahead := 0
    for other in get_tree().get_nodes_in_group("bots"):
        if other == self or not is_instance_valid(other):
            continue
        if other.get_instance_id() < get_instance_id():
            var gap: Vector3 = other.global_position - global_position
            gap.y = 0.0
            if gap.length() <= 3.15:
                ahead += 1
                if ahead >= MAX_SIMULTANEOUS_ATTACKERS:
                    return false
    return true

func take_damage(amount: float) -> void:
    if dead:
        return
    health -= amount
    if health <= 0.0:
        dead = true
        died.emit()
        queue_free()
