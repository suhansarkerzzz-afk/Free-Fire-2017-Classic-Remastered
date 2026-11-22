extends Node3D
## Lightweight cosmetic companion prototype.
## It follows the player visually; it has no navigation, collision, combat or gameplay buffs.

const FOLLOW_OFFSET := Vector3(1.15, -1.0, 1.15)
const FOLLOW_SMOOTHING := 7.0
const TELEPORT_DISTANCE := 7.0

static func get_pet_count() -> int:
    return 3

static func get_pet_name(index: int) -> String:
    match clampi(index, 0, get_pet_count() - 1):
        0:
            return "FOX COMPANION"
        1:
            return "ROBOT PUP"
        _:
            return "SCOUT DRONE"

var target: CharacterBody3D
var pet_index := 0
var visual_root: Node3D
var idle_time := 0.0
var material_cache: Dictionary = {}

func _ready() -> void:
    name = "PetCompanion"
    pet_index = clampi(pet_index, 0, get_pet_count() - 1)
    visual_root = Node3D.new()
    visual_root.name = "PetVisual"
    add_child(visual_root)
    _build_pet_visual()

    if is_instance_valid(target):
        global_position = _get_follow_position()
        show()
    else:
        hide()

func _process(delta: float) -> void:
    if not is_instance_valid(target):
        hide()
        return

    var airborne_state := int(target.get("airdrop_state"))
    var should_hide := bool(target.get("is_dead")) or bool(target.get("is_in_vehicle")) or airborne_state != 0
    if should_hide:
        hide()
        return

    show()
    var desired_position := _get_follow_position()
    if global_position.distance_squared_to(desired_position) > TELEPORT_DISTANCE * TELEPORT_DISTANCE:
        global_position = desired_position
    else:
        var follow_weight := 1.0 - exp(-FOLLOW_SMOOTHING * delta)
        global_position = global_position.lerp(desired_position, follow_weight)

    var look_target := target.global_position
    look_target.y = global_position.y
    if global_position.distance_squared_to(look_target) > 0.01:
        look_at(look_target, Vector3.UP)

    idle_time += delta
    var bob_height := 0.10 if pet_index == 2 else 0.035
    visual_root.position.y = sin(idle_time * 3.2) * bob_height
    if pet_index == 2:
        visual_root.rotation.y += delta * 0.7

func _get_follow_position() -> Vector3:
    return target.global_position + target.global_basis * FOLLOW_OFFSET

func _build_pet_visual() -> void:
    match pet_index:
        0:
            _build_fox()
        1:
            _build_robot_pup()
        _:
            _build_scout_drone()

func _build_fox() -> void:
    var orange := Color(0.82, 0.34, 0.10)
    var dark_orange := Color(0.42, 0.16, 0.07)
    var cream := Color(0.94, 0.84, 0.65)
    var dark := Color(0.07, 0.06, 0.05)
    _add_sphere("FoxBody", Vector3(0, 0.40, 0.08), Vector3(0.58, 0.38, 0.76), orange)
    _add_sphere("FoxChest", Vector3(0, 0.36, -0.34), Vector3(0.32, 0.30, 0.22), cream)
    _add_sphere("FoxHead", Vector3(0, 0.72, -0.42), Vector3(0.43, 0.42, 0.42), orange)
    _add_sphere("FoxMuzzle", Vector3(0, 0.62, -0.70), Vector3(0.25, 0.16, 0.20), cream)
    _add_box("FoxEarLeft", Vector3(0.15, 0.25, 0.12), Vector3(-0.22, 1.00, -0.38), dark_orange, Vector3(0, 0, deg_to_rad(12.0)))
    _add_box("FoxEarRight", Vector3(0.15, 0.25, 0.12), Vector3(0.22, 1.00, -0.38), dark_orange, Vector3(0, 0, deg_to_rad(-12.0)))
    _add_sphere("FoxEyeLeft", Vector3(-0.14, 0.76, -0.78), Vector3(0.055, 0.065, 0.035), dark)
    _add_sphere("FoxEyeRight", Vector3(0.14, 0.76, -0.78), Vector3(0.055, 0.065, 0.035), dark)
    _add_sphere("FoxTail", Vector3(0, 0.48, 0.57), Vector3(0.27, 0.26, 0.48), orange)
    _add_sphere("FoxTailTip", Vector3(0, 0.55, 0.88), Vector3(0.19, 0.19, 0.22), cream)
    for side in [-1.0, 1.0]:
        for forward in [-0.25, 0.30]:
            _add_box("FoxLeg", Vector3(0.15, 0.27, 0.16), Vector3(side * 0.19, 0.16, forward), dark_orange)

func _build_robot_pup() -> void:
    var blue := Color(0.20, 0.30, 0.40)
    var dark_blue := Color(0.07, 0.12, 0.18)
    var silver := Color(0.50, 0.61, 0.66)
    var cyan := Color(0.12, 0.88, 0.96)
    _add_sphere("RobotBody", Vector3(0, 0.42, 0.08), Vector3(0.62, 0.42, 0.72), blue, 0.35)
    _add_box("RobotChest", Vector3(0.34, 0.24, 0.10), Vector3(0, 0.43, -0.32), silver)
    _add_sphere("RobotHead", Vector3(0, 0.76, -0.42), Vector3(0.44, 0.40, 0.40), dark_blue, 0.45)
    _add_box("RobotEarLeft", Vector3(0.14, 0.27, 0.16), Vector3(-0.27, 0.99, -0.36), blue)
    _add_box("RobotEarRight", Vector3(0.14, 0.27, 0.16), Vector3(0.27, 0.99, -0.36), blue)
    _add_sphere("RobotEyeLeft", Vector3(-0.14, 0.79, -0.77), Vector3(0.075, 0.075, 0.045), cyan, 0.2)
    _add_sphere("RobotEyeRight", Vector3(0.14, 0.79, -0.77), Vector3(0.075, 0.075, 0.045), cyan, 0.2)
    _add_sphere("RobotTail", Vector3(0, 0.61, 0.54), Vector3(0.13, 0.13, 0.22), silver, 0.4)
    for side in [-1.0, 1.0]:
        for forward in [-0.25, 0.30]:
            _add_box("RobotLeg", Vector3(0.16, 0.27, 0.17), Vector3(side * 0.20, 0.16, forward), dark_blue, Vector3.ZERO, 0.25)

func _build_scout_drone() -> void:
    var body := Color(0.13, 0.20, 0.27)
    var trim := Color(0.39, 0.48, 0.54)
    var glow := Color(0.12, 0.82, 0.96)
    _add_sphere("DroneCore", Vector3(0, 0.62, 0), Vector3(0.48, 0.38, 0.48), body, 0.45)
    _add_sphere("DroneLens", Vector3(0, 0.62, -0.24), Vector3(0.18, 0.18, 0.10), glow, 0.25)
    for side in [-1.0, 1.0]:
        for forward in [-1.0, 1.0]:
            var arm_position := Vector3(side * 0.43, 0.66, forward * 0.38)
            _add_box("DroneArm", Vector3(0.62, 0.08, 0.10), arm_position, trim, Vector3(0, -side * forward * 0.45, 0))
            _add_sphere("DroneRotor", arm_position + Vector3(side * 0.22, 0.10, forward * 0.18), Vector3(0.22, 0.045, 0.22), body, 0.35)
            _add_sphere("DroneLight", arm_position + Vector3(0, -0.08, 0), Vector3(0.08, 0.08, 0.08), glow)
    _add_box("DroneTail", Vector3(0.12, 0.10, 0.42), Vector3(0, 0.53, 0.43), trim)

func _add_sphere(label: String, local_position: Vector3, dimensions: Vector3, color: Color, metallic: float = 0.0) -> void:
    var visual := MeshInstance3D.new()
    visual.name = label
    var mesh := SphereMesh.new()
    mesh.radial_segments = 12
    mesh.rings = 8
    visual.mesh = mesh
    visual.scale = dimensions
    visual.position = local_position
    visual.material_override = _make_material(color, metallic)
    visual_root.add_child(visual)

func _add_box(label: String, dimensions: Vector3, local_position: Vector3, color: Color, rotation_value: Vector3 = Vector3.ZERO, metallic: float = 0.0) -> void:
    var visual := MeshInstance3D.new()
    visual.name = label
    var mesh := BoxMesh.new()
    mesh.size = dimensions
    visual.mesh = mesh
    visual.position = local_position
    visual.rotation = rotation_value
    visual.material_override = _make_material(color, metallic)
    visual_root.add_child(visual)

func _make_material(color: Color, metallic: float = 0.0) -> StandardMaterial3D:
    var cache_key := "%d:%d" % [color.to_rgba32(), roundi(metallic * 100.0)]
    var cached := material_cache.get(cache_key) as StandardMaterial3D
    if cached != null:
        return cached

    var material := StandardMaterial3D.new()
    material.albedo_color = color
    material.roughness = 0.82
    material.metallic = metallic
    material_cache[cache_key] = material
    return material
