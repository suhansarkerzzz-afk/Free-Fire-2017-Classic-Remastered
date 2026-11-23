extends Node3D
## Lightweight procedural rifle and third-person holding animation.
## Uses primitive meshes only to keep the Android prototype inexpensive.

const RELOAD_DURATION := 1.5

var owner_player: CharacterBody3D
var weapon_pivot: Node3D
var magazine_pivot: Node3D
var muzzle_flash: MeshInstance3D
var magazine_home_position := Vector3.ZERO
var muzzle_flash_time := 0.0
var recoil_strength := 0.0
var reload_elapsed := -1.0
var animation_time := 0.0

func _ready() -> void:
    _build_weapon()

func _build_weapon() -> void:
    weapon_pivot = Node3D.new()
    weapon_pivot.name = "RiflePivot"
    add_child(weapon_pivot)

    var receiver_material := _make_material(Color(0.075, 0.085, 0.09), 0.28)
    var handguard_material := _make_material(Color(0.16, 0.18, 0.18), 0.18)
    var metal_material := _make_material(Color(0.34, 0.36, 0.35), 0.62)
    var grip_material := _make_material(Color(0.045, 0.052, 0.05), 0.0)

    _add_box("RifleReceiver", Vector3(0.18, 0.16, 0.72), Vector3(0.0, 0.0, -0.36), receiver_material)
    _add_box("RifleHandguard", Vector3(0.15, 0.14, 0.42), Vector3(0.0, 0.0, -0.72), handguard_material)
    _add_box("RifleBarrel", Vector3(0.055, 0.055, 0.40), Vector3(0.0, 0.015, -1.10), metal_material)
    _add_box("RifleStock", Vector3(0.16, 0.14, 0.27), Vector3(0.0, 0.015, 0.13), grip_material)
    _add_box("RifleGrip", Vector3(0.105, 0.22, 0.14), Vector3(0.0, -0.16, -0.12), grip_material, Vector3(deg_to_rad(-8.0), 0.0, 0.0))
    _add_box("RifleOpticBase", Vector3(0.12, 0.055, 0.17), Vector3(0.0, 0.105, -0.42), metal_material)
    _add_box("RifleOptic", Vector3(0.105, 0.10, 0.12), Vector3(0.0, 0.18, -0.42), receiver_material)
    _add_box("RifleFrontSight", Vector3(0.045, 0.10, 0.055), Vector3(0.0, 0.10, -0.94), metal_material)

    magazine_pivot = Node3D.new()
    magazine_pivot.name = "MagazinePivot"
    magazine_pivot.position = Vector3(0.0, -0.09, -0.40)
    weapon_pivot.add_child(magazine_pivot)
    magazine_home_position = magazine_pivot.position
    var magazine := MeshInstance3D.new()
    magazine.name = "RifleMagazine"
    var magazine_mesh := BoxMesh.new()
    magazine_mesh.size = Vector3(0.13, 0.25, 0.16)
    magazine.mesh = magazine_mesh
    magazine.position = Vector3(0.0, -0.10, 0.0)
    magazine.material_override = grip_material
    magazine_pivot.add_child(magazine)

    var flash_material := StandardMaterial3D.new()
    flash_material.albedo_color = Color(1.0, 0.48, 0.08)
    flash_material.shading_mode = BaseMaterial3D.SHADING_MODE_UNSHADED
    muzzle_flash = MeshInstance3D.new()
    muzzle_flash.name = "MuzzleFlash"
    var flash_mesh := SphereMesh.new()
    flash_mesh.radius = 0.085
    flash_mesh.height = 0.22
    flash_mesh.radial_segments = 8
    muzzle_flash.mesh = flash_mesh
    muzzle_flash.scale = Vector3(0.8, 0.8, 1.4)
    muzzle_flash.position = Vector3(0.0, 0.015, -1.32)
    muzzle_flash.material_override = flash_material
    muzzle_flash.hide()
    weapon_pivot.add_child(muzzle_flash)

func _add_box(label: String, size: Vector3, local_position: Vector3, material: Material, local_rotation: Vector3 = Vector3.ZERO) -> void:
    var visual := MeshInstance3D.new()
    visual.name = label
    var mesh := BoxMesh.new()
    mesh.size = size
    visual.mesh = mesh
    visual.position = local_position
    visual.rotation = local_rotation
    visual.material_override = material
    weapon_pivot.add_child(visual)

func _make_material(color: Color, metallic: float) -> StandardMaterial3D:
    var material := StandardMaterial3D.new()
    material.albedo_color = color
    material.roughness = 0.78
    material.metallic = metallic
    return material

func play_fire() -> void:
    recoil_strength = minf(1.0, recoil_strength + 0.85)
    muzzle_flash_time = 0.055
    if is_instance_valid(muzzle_flash):
        muzzle_flash.show()

func play_reload() -> void:
    reload_elapsed = 0.0
    muzzle_flash_time = 0.0
    if is_instance_valid(muzzle_flash):
        muzzle_flash.hide()

func _process(delta: float) -> void:
    if not is_instance_valid(owner_player):
        hide()
        return

    var should_hide := bool(owner_player.get("is_dead")) or bool(owner_player.get("is_in_vehicle")) or int(owner_player.get("airdrop_state")) != 0
    visible = not should_hide
    if should_hide:
        return

    animation_time += delta
    recoil_strength = move_toward(recoil_strength, 0.0, delta * 8.0)
    muzzle_flash_time = maxf(0.0, muzzle_flash_time - delta)
    if muzzle_flash_time <= 0.0 and is_instance_valid(muzzle_flash):
        muzzle_flash.hide()

    var horizontal_speed := Vector2(owner_player.velocity.x, owner_player.velocity.z).length()
    var movement_ratio := clampf(horizontal_speed / 8.2, 0.0, 1.0)
    var reload_pose := 0.0
    if reload_elapsed >= 0.0:
        reload_elapsed += delta
        var reload_progress := clampf(reload_elapsed / RELOAD_DURATION, 0.0, 1.0)
        reload_pose = sin(reload_progress * PI)
        if reload_progress >= 1.0:
            reload_elapsed = -1.0

    var walk_sway := sin(animation_time * 10.0) * 0.018 * movement_ratio
    weapon_pivot.position = Vector3(0.0, walk_sway - reload_pose * 0.08, recoil_strength * 0.10 + reload_pose * 0.04)
    weapon_pivot.rotation = Vector3(-recoil_strength * 0.18 + reload_pose * 0.62, 0.0, sin(animation_time * 5.0) * 0.018 * movement_ratio)
    magazine_pivot.position = magazine_home_position + Vector3(0.0, -reload_pose * 0.16, 0.0)
