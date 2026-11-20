extends Node3D

const ROUTE_START := Vector3(-58.0, 56.0, 0.0)
const ROUTE_END_X := 58.0
const ROUTE_SPEED := 18.0
const PASSENGER_OFFSET := Vector3(0.0, -2.0, 0.85)
const AIRDROP_IN_PLANE := 1

var passenger: CharacterBody3D
var route_active := false

func _ready() -> void:
    _build_plane_visual()

func begin_route(player_node: CharacterBody3D) -> void:
    if not is_instance_valid(player_node):
        return

    passenger = player_node
    global_position = ROUTE_START
    rotation.y = -PI * 0.5
    route_active = true

    if passenger.has_method("board_airdrop_plane"):
        passenger.call("board_airdrop_plane", _get_passenger_position())
    else:
        route_active = false
        push_warning("Airdrop passenger does not support plane boarding.")

func _get_passenger_position() -> Vector3:
    # PASSENGER_OFFSET is in the plane's local space, behind and below the fuselage.
    return global_position + global_basis * PASSENGER_OFFSET

func _physics_process(delta: float) -> void:
    if not route_active:
        return

    if not is_instance_valid(passenger) or bool(passenger.get("is_dead")):
        route_active = false
        queue_free()
        return

    global_position.x = minf(global_position.x + ROUTE_SPEED * delta, ROUTE_END_X)

    if int(passenger.get("airdrop_state")) == AIRDROP_IN_PLANE:
        # Keep the passenger offset in the plane's rotated local space throughout flight.
        passenger.global_position = _get_passenger_position()

    if global_position.x >= ROUTE_END_X:
        if int(passenger.get("airdrop_state")) == AIRDROP_IN_PLANE:
            passenger.call("force_jump_from_plane")
        route_active = false
        queue_free()

func _build_plane_visual() -> void:
    var fuselage_material := _make_material(Color(0.34, 0.39, 0.31))
    var wing_material := _make_material(Color(0.25, 0.30, 0.24))
    var cockpit_material := _make_material(Color(0.12, 0.25, 0.30), true)

    _add_box("Fuselage", Vector3(1.25, 1.05, 9.2), Vector3(0, 0, 0), fuselage_material)
    _add_box("Nose", Vector3(0.92, 0.78, 1.35), Vector3(0, 0.02, -4.9), fuselage_material)
    _add_box("MainWing", Vector3(13.5, 0.22, 2.1), Vector3(0, 0.12, 0.15), wing_material)
    _add_box("TailWing", Vector3(4.8, 0.18, 1.25), Vector3(0, 0.52, 3.65), wing_material)
    _add_box("TailFin", Vector3(0.20, 1.25, 1.55), Vector3(0, 1.08, 3.55), wing_material)
    _add_box("Cockpit", Vector3(0.92, 0.58, 1.45), Vector3(0, 0.68, -1.45), cockpit_material)

    for side in [-1.0, 1.0]:
        _add_box(
            "EnginePod",
            Vector3(0.62, 0.58, 2.1),
            Vector3(side * 3.4, -0.30, -0.10),
            fuselage_material
        )

func _add_box(label: String, size: Vector3, local_position: Vector3, material: Material) -> void:
    var visual := MeshInstance3D.new()
    visual.name = label
    var mesh := BoxMesh.new()
    mesh.size = size
    visual.mesh = mesh
    visual.material_override = material
    visual.position = local_position
    add_child(visual)

func _make_material(color: Color, transparent: bool = false) -> StandardMaterial3D:
    var material := StandardMaterial3D.new()
    material.albedo_color = color
    material.roughness = 0.82
    if transparent:
        material.transparency = BaseMaterial3D.TRANSPARENCY_ALPHA
    return material
