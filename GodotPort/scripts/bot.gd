extends CharacterBody3D

var target: Node3D
var health := 200.0
var attack_timer := 0.0
var move_speed := 4.0
var dead := false
var strafe_phase := 0.0

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
    if distance > 2.8:
        var direction := offset.normalized()
        var side := Vector3(-direction.z, 0, direction.x) * sin(Time.get_ticks_msec() * 0.0015 + strafe_phase) * 0.16
        var movement := (direction + side).normalized()
        velocity.x = movement.x * move_speed
        velocity.z = movement.z * move_speed
        rotation.y = lerp_angle(rotation.y, atan2(-movement.x, -movement.z), delta * 7.0)
    else:
        velocity.x = move_toward(velocity.x, 0.0, 14.0 * delta)
        velocity.z = move_toward(velocity.z, 0.0, 14.0 * delta)
        if attack_timer <= 0.0 and target.has_method("take_damage"):
            target.take_damage(8.0)
            attack_timer = 0.85
    if not is_on_floor():
        velocity.y -= 18.0 * delta
    else:
        velocity.y = -0.5
    move_and_slide()

func take_damage(amount: float) -> void:
    if dead:
        return
    health -= amount
    if health <= 0.0:
        dead = true
        queue_free()
