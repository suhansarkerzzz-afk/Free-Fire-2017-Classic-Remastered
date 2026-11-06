extends Node3D

# Offline battle-royale safe zone. The first phase gives players time to loot;
# the boundary then contracts and periodically damages actors left outside it.
@export var start_radius: float = 52.0
@export var end_radius: float = 8.0
@export var wait_seconds: float = 45.0
@export var shrink_duration: float = 180.0
@export var damage_interval: float = 2.0
@export var damage_per_tick: float = 8.0

const SEGMENTS := 96
const RING_WIDTH := 0.42

var player: Node3D
var elapsed := 0.0
var damage_elapsed := 0.0
var current_radius := 52.0
var boundary_mesh: ImmediateMesh
var boundary_visual: MeshInstance3D
var boundary_material: StandardMaterial3D

func _ready() -> void:
    current_radius = start_radius
    _create_boundary()
    _rebuild_boundary()

func _process(delta: float) -> void:
    elapsed += delta
    if elapsed < wait_seconds:
        return

    var duration := maxf(shrink_duration, 0.01)
    var progress := clampf((elapsed - wait_seconds) / duration, 0.0, 1.0)
    var next_radius := lerpf(start_radius, end_radius, progress)
    if absf(next_radius - current_radius) >= 0.05:
        current_radius = next_radius
        _rebuild_boundary()

    damage_elapsed += delta
    if damage_elapsed >= damage_interval:
        damage_elapsed = 0.0
        _damage_outside_zone()

func _create_boundary() -> void:
    boundary_mesh = ImmediateMesh.new()
    boundary_visual = MeshInstance3D.new()
    boundary_visual.name = "SafeZoneBoundary"
    boundary_visual.mesh = boundary_mesh
    boundary_visual.position.y = 0.12
    boundary_visual.cast_shadow = GeometryInstance3D.SHADOW_CASTING_SETTING_OFF

    boundary_material = StandardMaterial3D.new()
    boundary_material.shading_mode = BaseMaterial3D.SHADING_MODE_UNSHADED
    boundary_material.transparency = BaseMaterial3D.TRANSPARENCY_ALPHA
    boundary_material.albedo_color = Color(0.12, 0.62, 1.0, 0.82)
    boundary_material.cull_mode = BaseMaterial3D.CULL_DISABLED
    add_child(boundary_visual)

func _rebuild_boundary() -> void:
    if boundary_mesh == null:
        return

    boundary_mesh.clear_surfaces()
    boundary_mesh.surface_begin(Mesh.PRIMITIVE_TRIANGLES, boundary_material)
    var inner_radius := maxf(0.0, current_radius - RING_WIDTH)

    for i in range(SEGMENTS):
        var angle_a := TAU * float(i) / float(SEGMENTS)
        var angle_b := TAU * float(i + 1) / float(SEGMENTS)
        var inner_a := Vector3(cos(angle_a) * inner_radius, 0.0, sin(angle_a) * inner_radius)
        var outer_a := Vector3(cos(angle_a) * current_radius, 0.0, sin(angle_a) * current_radius)
        var inner_b := Vector3(cos(angle_b) * inner_radius, 0.0, sin(angle_b) * inner_radius)
        var outer_b := Vector3(cos(angle_b) * current_radius, 0.0, sin(angle_b) * current_radius)

        boundary_mesh.surface_add_vertex(inner_a)
        boundary_mesh.surface_add_vertex(outer_a)
        boundary_mesh.surface_add_vertex(outer_b)
        boundary_mesh.surface_add_vertex(inner_a)
        boundary_mesh.surface_add_vertex(outer_b)
        boundary_mesh.surface_add_vertex(inner_b)

    boundary_mesh.surface_end()

func _damage_outside_zone() -> void:
    for bot in get_tree().get_nodes_in_group("bots"):
        _damage_actor_if_outside(bot)

    if is_instance_valid(player):
        _damage_actor_if_outside(player)

func _damage_actor_if_outside(actor: Node3D) -> void:
    if not is_instance_valid(actor) or not actor.has_method("take_damage"):
        return

    var offset := actor.global_position - global_position
    offset.y = 0.0
    if offset.length() > current_radius:
        actor.call("take_damage", damage_per_tick)

func get_current_radius() -> float:
    return current_radius
