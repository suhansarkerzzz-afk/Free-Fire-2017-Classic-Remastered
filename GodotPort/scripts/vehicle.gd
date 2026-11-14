extends CharacterBody3D

const MAX_FORWARD_SPEED := 13.0
const MAX_REVERSE_SPEED := 5.0
const ACCELERATION := 8.5
const DECELERATION := 4.5
const STEERING_RATE := 1.55
const GRAVITY := 18.0

var driver: CharacterBody3D
var is_occupied := false
var current_speed := 0.0
var saved_driver_collision_layer := 1
var saved_driver_collision_mask := 1

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

        for axle_z in [-1.08, 1.08]:
            var wheel := MeshInstance3D.new()
            var wheel_mesh := CylinderMesh.new()
            wheel_mesh.top_radius = 0.37
            wheel_mesh.bottom_radius = 0.37
            wheel_mesh.height = 0.24
            wheel_mesh.radial_segments = 10
            wheel.mesh = wheel_mesh
            wheel.material_override = rubber
            wheel.position = Vector3(side * 0.99, 0.37, axle_z)
            wheel.rotation_degrees.z = 90.0
            add_child(wheel)

            var hub := MeshInstance3D.new()
            var hub_mesh := CylinderMesh.new()
            hub_mesh.top_radius = 0.14
            hub_mesh.bottom_radius = 0.14
            hub_mesh.height = 0.25
            hub_mesh.radial_segments = 8
            hub.mesh = hub_mesh
            hub.material_override = metal
            hub.position = wheel.position
            hub.rotation_degrees.z = 90.0
            add_child(hub)

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

func enter_vehicle(player_node: CharacterBody3D) -> bool:
    if is_occupied or not is_instance_valid(player_node):
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

    var exit_position := global_position + global_basis.x * 1.8 + Vector3.UP * 1.0
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
        camera_arm.remove_excluded_object(get_rid())
    driver.velocity = Vector3.ZERO

    driver = null
    is_occupied = false
    current_speed = 0.0
    velocity = Vector3.ZERO

func _physics_process(delta: float) -> void:
    if not is_occupied or not is_instance_valid(driver):
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

    var forward := -global_basis.z
    velocity.x = forward.x * current_speed
    velocity.z = forward.z * current_speed
    if not is_on_floor():
        velocity.y -= GRAVITY * delta
    else:
        velocity.y = -0.5

    move_and_slide()
    _sync_driver_to_seat()

func _sync_driver_to_seat() -> void:
    if not is_instance_valid(driver):
        return
    driver.global_transform = global_transform * Transform3D(Basis.IDENTITY, Vector3(0, 1.12, 0.08))

func get_speed_kph() -> int:
    return roundi(absf(current_speed) * 3.6)
