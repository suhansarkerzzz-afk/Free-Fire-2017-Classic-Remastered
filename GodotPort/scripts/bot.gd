extends CharacterBody3D

signal died

var target: Node3D
var health := 200.0
var attack_timer := 0.0
var ranged_timer := 0.0
var move_speed := 4.0
var dead := false
var strafe_phase := 0.0

const GRAVITY := 18.0
const SEPARATION_RADIUS := 2.8
const SEPARATION_STRENGTH := 2.2
const SEPARATION_REFRESH_INTERVAL := 0.18
const MAX_SIMULTANEOUS_ATTACKERS := 3

var separation_refresh_timer := 0.0
var cached_separation := Vector3.ZERO

func _ready() -> void:
    add_to_group("bots")
    strafe_phase = randf_range(0.0, TAU)
    # Stagger neighbour scans so all 49 bots do not run the O(n) scan together.
    separation_refresh_timer = randf_range(0.0, SEPARATION_REFRESH_INTERVAL)

func _physics_process(delta: float) -> void:
    if dead or not is_instance_valid(target):
        velocity = Vector3.ZERO
        return

    attack_timer = maxf(0.0, attack_timer - delta)
    ranged_timer = maxf(0.0, ranged_timer - delta)
    var offset := target.global_position - global_position
    offset.y = 0.0
    var distance := offset.length()
    var direction := Vector3.ZERO

    if distance > 2.7:
        direction = offset.normalized()
    else:
        velocity.x = move_toward(velocity.x, 0.0, 14.0 * delta)
        velocity.z = move_toward(velocity.z, 0.0, 14.0 * delta)

    # Refresh neighbour separation a few times per second instead of scanning all
    # bots on every physics frame. The staggered timer spreads this work across frames.
    separation_refresh_timer -= delta
    if separation_refresh_timer <= 0.0:
        cached_separation = _get_separation()
        separation_refresh_timer = SEPARATION_REFRESH_INTERVAL + randf_range(0.0, 0.04)
    var separation := cached_separation
    if direction != Vector3.ZERO:
        var side := Vector3(-direction.z, 0.0, direction.x) * sin(Time.get_ticks_msec() * 0.0015 + strafe_phase) * 0.10
        var movement := (direction + side + separation * SEPARATION_STRENGTH).normalized()
        velocity.x = movement.x * move_speed
        velocity.z = movement.z * move_speed
        rotation.y = lerp_angle(rotation.y, atan2(-movement.x, -movement.z), delta * 7.0)
    elif separation.length_squared() > 0.01:
        velocity.x = separation.x * move_speed * 0.65
        velocity.z = separation.z * move_speed * 0.65

    # Bots use a simple hitscan rifle at medium range; walls and other actors block shots.
    if distance > 7.0 and distance <= 28.0 and ranged_timer <= 0.0:
        _fire_rifle_at_target()
        ranged_timer = randf_range(1.25, 1.8)

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

func _fire_rifle_at_target() -> void:
    if not is_instance_valid(target) or not target.has_method("take_damage"):
        return
    var origin := global_position + Vector3(0, 0.72, 0)
    var aim_point := target.global_position + Vector3(0, 0.85, 0)
    var query := PhysicsRayQueryParameters3D.create(origin, aim_point)
    query.exclude = [get_rid()]
    var hit := get_world_3d().direct_space_state.intersect_ray(query)
    if not hit.is_empty() and hit.get("collider") == target:
        target.take_damage(randf_range(5.0, 8.0))

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
