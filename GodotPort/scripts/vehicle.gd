extends CharacterBody3D

signal health_changed(current: int, maximum: int)

const MAX_HEALTH := 500
const COLLISION_DAMAGE_MIN_SPEED := 5.0
const COLLISION_DAMAGE_PER_SPEED := 6.0
const MAX_COLLISION_DAMAGE := 60
const COLLISION_DAMAGE_COOLDOWN := 0.85

const MAX_FORWARD_SPEED := 13.0
const MAX_REVERSE_SPEED := 5.0
const ACCELERATION := 8.5
const DECELERATION := 4.5
const STEERING_RATE := 1.55
const GRAVITY := 18.0

var driver: CharacterBody3D
var is_occupied := false
var current_speed := 0.0
var health := MAX_HEALTH
var is_destroyed := false
var paint_material: StandardMaterial3D
var windshield_crack_material: StandardMaterial3D
var windshield_crack_nodes: Array[MeshInstance3D] = []
var collision_damage_cooldown := 0.0
var saved_driver_collision_layer := 1
var saved_driver_collision_mask := 1
var wheel_spin_nodes: Array[Node3D] = []
var front_wheel_steering_nodes: Array[Node3D] = []

func _ready() -> void:
    _build_vehicle_visual()

func _build_vehicle_visual() -> void:
    var collider := CollisionShape3D.new()
    var body_shape := BoxShape3D.new()
    body_shape.size = Vector3(2.0, 1.0, 3.45)
    collider.shape = body_shape
    collider.position = Vector3(0, 0.62, 0)
    add_child(collider)

    var paint := _make_material(Color(0.18, 0.31, 0.22))
    paint_material = paint
    var dark_paint := _make_material(Color(0.07, 0.09, 0.08))
    var glass := _make_material(Color(0.18, 0.42, 0.48, 0.82), true)
    var rubber := _make_material(Color(0.025, 0.03, 0.032))
    var metal := _make_material(Color(0.38, 0.4, 0.38), false, 0.55)
    var lamp := _make_material(Color(1.0, 0.82, 0.42), true)

    _add_box("JeepBody", Vector3(1.9, 0.55, 3.25), Vector3(0, 0.76, 0), paint)
    _add_box("FrontHood", Vector3(1.82, 0.22, 0.92), Vector3(0, 1.08, -1.05), paint)
    _add_box("Cabin", Vector3(1.58, 0.78, 1.62), Vector3(0, 1.34, 0.18), dark_paint)
    _add_box("Roof", Vector3(1.68, 0.13, 1.78), Vector3(0, 1.79, 0.18), paint)
    _add_box("Windshield", Vector3(1.42, 0.43, 0.055), Vector3(0, 1.43, -0.66), glass)
    _add_windshield_crack_overlays()
    _add_box("FrontBumper", Vector3(2.02, 0.16, 0.18), Vector3(0, 0.48, -1.72), metal)
    _add_box("RearBumper", Vector3(2.02, 0.16, 0.18), Vector3(0, 0.48, 1.72), metal)

    for side in [-1.0, 1.0]:
        var headlight := MeshInstance3D.new()
        var headlight_mesh := SphereMesh.new()
        headlight_mesh.radius = 0.12
        headlight_mesh.height = 0.24
        headlight.mesh = headlight_mesh
        headlight.material_override = lamp
        headlight.position = Vector3(side * 0.66, 0.88, -1.67)
        add_child(headlight)

        for axle_z in [-1.0, 1.0]:
            # The steering pivot turns only the front axle; the child pivot spins
            # the wheel around its axle as the Jeep moves.
            var steering_pivot := Node3D.new()
            steering_pivot.name = "FrontWheelSteering" if axle_z < 0.0 else "RearWheelMount"
            steering_pivot.position = Vector3(side * 0.99, 0.37, axle_z * 1.08)
            add_child(steering_pivot)
            if axle_z < 0.0:
                front_wheel_steering_nodes.append(steering_pivot)

            var spin_pivot := Node3D.new()
            spin_pivot.name = "WheelSpin"
            steering_pivot.add_child(spin_pivot)
            wheel_spin_nodes.append(spin_pivot)

            var wheel := MeshInstance3D.new()
            var wheel_mesh := CylinderMesh.new()
            wheel_mesh.top_radius = 0.37
            wheel_mesh.bottom_radius = 0.37
            wheel_mesh.height = 0.24
            wheel_mesh.radial_segments = 10
            wheel.mesh = wheel_mesh
            wheel.material_override = rubber
            wheel.rotation_degrees.z = 90.0
            spin_pivot.add_child(wheel)

            var hub := MeshInstance3D.new()
            var hub_mesh := CylinderMesh.new()
            hub_mesh.top_radius = 0.14
            hub_mesh.bottom_radius = 0.14
            hub_mesh.height = 0.25
            hub_mesh.radial_segments = 8
            hub.mesh = hub_mesh
            hub.material_override = metal
            hub.rotation_degrees.z = 90.0
            spin_pivot.add_child(hub)

func _make_material(color: Color, transparent: bool = false, metallic: float = 0.0) -> StandardMaterial3D:
    var material := StandardMaterial3D.new()
    material.albedo_color = color
    material.roughness = 0.78
    material.metallic = metallic
    if transparent:
        material.transparency = BaseMaterial3D.TRANSPARENCY_ALPHA
    return material

func _add_box(label: String, size: Vector3, local_position: Vector3, material: Material) -> void:
    var visual := MeshInstance3D.new()
    visual.name = label
    var mesh := BoxMesh.new()
    mesh.size = size
    visual.mesh = mesh
    visual.material_override = material
    visual.position = local_position
    add_child(visual)

func _add_windshield_crack_overlays() -> void:
    windshield_crack_material = _make_material(Color(0.055, 0.065, 0.06, 0.92), true)
    _add_windshield_crack(Vector3(0.02, 1.45, -0.70), Vector3(0.022, 0.52, 0.012), deg_to_rad(24.0))
    _add_windshield_crack(Vector3(0.13, 1.62, -0.70), Vector3(0.018, 0.30, 0.012), deg_to_rad(-48.0))
    _add_windshield_crack(Vector3(-0.12, 1.30, -0.70), Vector3(0.018, 0.28, 0.012), deg_to_rad(-42.0))
    _add_windshield_crack(Vector3(-0.22, 1.53, -0.70), Vector3(0.016, 0.22, 0.012), deg_to_rad(58.0))


func _add_windshield_crack(local_position: Vector3, size: Vector3, angle: float) -> void:
    var crack := MeshInstance3D.new()
    var crack_mesh := BoxMesh.new()
    crack_mesh.size = size
    crack.mesh = crack_mesh
    crack.material_override = windshield_crack_material
    crack.position = local_position
    crack.rotation.z = angle
    crack.visible = false
    add_child(crack)
    windshield_crack_nodes.append(crack)


func _update_damage_visuals() -> void:
    var damage_ratio := 1.0 - float(health) / float(MAX_HEALTH)
    if is_instance_valid(paint_material):
        paint_material.albedo_color = Color(0.18, 0.31, 0.22).lerp(Color(0.07, 0.09, 0.07), damage_ratio * 0.75)
        paint_material.roughness = lerpf(0.78, 0.96, damage_ratio)

    var show_cracks := health <= MAX_HEALTH / 2
    for crack in windshield_crack_nodes:
        if is_instance_valid(crack):
            crack.visible = show_cracks


func enter_vehicle(player_node: CharacterBody3D) -> bool:
    if is_destroyed or is_occupied or not is_instance_valid(player_node):
        return false

    driver = player_node
    saved_driver_collision_layer = driver.collision_layer
    saved_driver_collision_mask = driver.collision_mask
    driver.collision_layer = 0
    driver.collision_mask = 0
    driver.set("is_in_vehicle", true)
    driver.set("move_input", Vector2.ZERO)
    driver.set("firing", false)
    driver.velocity = Vector3.ZERO
    for child in driver.get_children():
        if child is MeshInstance3D:
            child.visible = false
    var camera_arm := driver.get_node_or_null("CameraPivot/SpringArm3D") as SpringArm3D
    if is_instance_valid(camera_arm):
        camera_arm.add_excluded_object(get_rid())
    is_occupied = true
    _sync_driver_to_seat()
    return true

func exit_vehicle() -> void:
    if not is_occupied or not is_instance_valid(driver):
        is_occupied = false
        driver = null
        current_speed = 0.0
        return

    var exit_position := _find_safe_exit_position(driver)
    driver.global_position = exit_position
    driver.global_rotation = Vector3(0.0, global_rotation.y, 0.0)
    driver.set("is_in_vehicle", false)
    driver.collision_layer = saved_driver_collision_layer
    driver.collision_mask = saved_driver_collision_mask
    for child in driver.get_children():
        if child is MeshInstance3D:
            child.visible = true
    var camera_arm := driver.get_node_or_null("CameraPivot/SpringArm3D") as SpringArm3D
    if is_instance_valid(camera_arm):
        camera_arm.clear_excluded_objects()
    driver.velocity = Vector3.ZERO

    driver = null
    is_occupied = false
    current_speed = 0.0
    velocity = Vector3.ZERO

func _find_safe_exit_position(player_node: CharacterBody3D) -> Vector3:
    # Try clear positions around the Jeep before restoring the player's collision.
    # This avoids placing the player inside a wall, prop or the vehicle collider.
    var collision_shape := player_node.get_node_or_null("CollisionShape3D") as CollisionShape3D
    var overhead_fallback := global_position + Vector3.UP * 2.35
    if not is_instance_valid(collision_shape) or collision_shape.shape == null:
        return global_position + global_basis.x * 1.9 + Vector3.UP * 1.15

    var candidate_offsets: Array[Vector3] = [
        global_basis.x * 1.9 + Vector3.UP * 1.15,
        -global_basis.x * 1.9 + Vector3.UP * 1.15,
        -global_basis.z * 2.35 + Vector3.UP * 1.15,
        global_basis.z * 2.35 + Vector3.UP * 1.15,
        Vector3.UP * 2.35
    ]
    var space_state := get_world_3d().direct_space_state
    for offset in candidate_offsets:
        var candidate_position := global_position + offset
        var query := PhysicsShapeQueryParameters3D.new()
        query.shape = collision_shape.shape
        var candidate_transform := collision_shape.global_transform
        candidate_transform.origin = candidate_position
        query.transform = candidate_transform
        query.collision_mask = saved_driver_collision_mask
        query.exclude = [player_node.get_rid(), get_rid()]
        query.margin = 0.04
        if space_state.intersect_shape(query, 1).is_empty():
            return candidate_position

    # The overhead fallback clears the Jeep's low body collider if every side is blocked.
    return overhead_fallback

func _physics_process(delta: float) -> void:
    if is_destroyed or not is_occupied or not is_instance_valid(driver):
        velocity = Vector3.ZERO
        return

    var input_vector := Vector2.ZERO
    if OS.has_feature("mobile"):
        var move_value: Variant = driver.get("move_input")
        if move_value is Vector2:
            input_vector = move_value
    else:
        input_vector = Input.get_vector("move_left", "move_right", "move_forward", "move_back")

    var throttle := clampf(-input_vector.y, -1.0, 1.0)
    if absf(throttle) > 0.08:
        var target_speed := MAX_FORWARD_SPEED if throttle > 0.0 else -MAX_REVERSE_SPEED
        current_speed = move_toward(current_speed, target_speed * absf(throttle), ACCELERATION * delta)
    else:
        current_speed = move_toward(current_speed, 0.0, DECELERATION * delta)

    var steering := clampf(input_vector.x, -1.0, 1.0)
    if absf(current_speed) > 0.4:
        var speed_factor := clampf(absf(current_speed) / MAX_FORWARD_SPEED, 0.0, 1.0)
        rotate_y(-steering * signf(current_speed) * STEERING_RATE * speed_factor * delta)

    var wheel_steer_angle := -steering * 0.48
    for front_wheel in front_wheel_steering_nodes:
        front_wheel.rotation.y = lerp_angle(front_wheel.rotation.y, wheel_steer_angle, minf(1.0, delta * 9.0))

    var wheel_rotation := current_speed * delta / 0.37
    for wheel_spin in wheel_spin_nodes:
        wheel_spin.rotate_x(wheel_rotation)

    var forward := -global_basis.z
    velocity.x = forward.x * current_speed
    velocity.z = forward.z * current_speed
    if not is_on_floor():
        velocity.y -= GRAVITY * delta
    else:
        velocity.y = -0.5

    collision_damage_cooldown = maxf(0.0, collision_damage_cooldown - delta)
    var incoming_velocity := velocity
    move_and_slide()
    _apply_collision_damage(incoming_velocity)
    _sync_driver_to_seat()

func _apply_collision_damage(incoming_velocity: Vector3) -> void:
    if is_destroyed or collision_damage_cooldown > 0.0:
        return

    var horizontal_velocity := Vector2(incoming_velocity.x, incoming_velocity.z)
    var impact_speed := 0.0
    for index in range(get_slide_collision_count()):
        var collision := get_slide_collision(index)
        var normal_3d := collision.get_normal()
        var normal_2d := Vector2(normal_3d.x, normal_3d.z)
        if normal_2d.length_squared() <= 0.25:
            continue
        var speed_into_surface := -horizontal_velocity.dot(normal_2d.normalized())
        impact_speed = maxf(impact_speed, speed_into_surface)

    if impact_speed < COLLISION_DAMAGE_MIN_SPEED:
        return

    var damage := roundi((impact_speed - COLLISION_DAMAGE_MIN_SPEED) * COLLISION_DAMAGE_PER_SPEED)
    apply_damage(clampi(damage, 1, MAX_COLLISION_DAMAGE))
    collision_damage_cooldown = COLLISION_DAMAGE_COOLDOWN

func apply_damage(amount: int) -> void:
    if is_destroyed or amount <= 0:
        return

    health = maxi(0, health - amount)
    _update_damage_visuals()
    health_changed.emit(health, MAX_HEALTH)
    if health == 0:
        is_destroyed = true
        current_speed = 0.0
        velocity = Vector3.ZERO
        if is_occupied:
            exit_vehicle()

func get_health() -> int:
    return health

func get_max_health() -> int:
    return MAX_HEALTH

func _sync_driver_to_seat() -> void:
    if not is_instance_valid(driver):
        return
    driver.global_transform = global_transform * Transform3D(Basis.IDENTITY, Vector3(0, 1.12, 0.08))

func get_speed_kph() -> int:
    return roundi(absf(current_speed) * 3.6)
