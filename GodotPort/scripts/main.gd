extends Node3D

const PlayerScript = preload("res://scripts/player.gd")
const BotScript = preload("res://scripts/bot.gd")
const SafeZoneScript = preload("res://scripts/safe_zone.gd")
const BOT_COUNT := 49

var player: CharacterBody3D
var menu_layer: Control
var hud_layer: Control
var health_label: Label
var ammo_label: Label
var status_label: Label
var zone_label: Label
var safe_zone_controller: Node3D
var zone_ui_timer := 0.0
var bot_label: Label
var fire_button: Button
var touch_move_id := -1
var touch_look_id := -1
var move_origin := Vector2.ZERO
var virtual_move := Vector2.ZERO
var move_pad: Panel
var move_knob: Panel
var game_over_panel: PanelContainer
var medkit_count := 0
var inventory_label: Label
var match_started := false
var kills := 0
var rng := RandomNumberGenerator.new()

func _ready() -> void:
    rng.randomize()
    _setup_input()
    _build_world()
    _build_menu()
    _build_hud()
    hud_layer.hide()
    # Start the playable scene immediately on Android so the first screen is the 3D island, not a text-only menu.
    call_deferred("start_match")

func _setup_input() -> void:
    _add_key_action("move_left", KEY_A)
    _add_key_action("move_right", KEY_D)
    _add_key_action("move_forward", KEY_W)
    _add_key_action("move_back", KEY_S)
    _add_key_action("sprint", KEY_SHIFT)
    _add_key_action("jump", KEY_SPACE)
    _add_key_action("reload", KEY_R)
    _add_key_action("fire", KEY_F)

func _add_key_action(action: String, key: Key) -> void:
    if not InputMap.has_action(action):
        InputMap.add_action(action)
    var event := InputEventKey.new()
    event.physical_keycode = key
    InputMap.action_add_event(action, event)

func _build_world() -> void:
    var environment := WorldEnvironment.new()
    var env := Environment.new()
    env.background_mode = Environment.BG_COLOR
    env.background_color = Color(0.48, 0.68, 0.82)
    env.ambient_light_source = Environment.AMBIENT_SOURCE_COLOR
    env.ambient_light_color = Color(0.72, 0.78, 0.82)
    env.ambient_light_energy = 0.8
    environment.environment = env
    add_child(environment)
    var sun := DirectionalLight3D.new()
    sun.rotation_degrees = Vector3(-52, -28, 0)
    sun.light_energy = 1.1
    add_child(sun)
    _make_box("IslandGround", Vector3(0, -0.6, 0), Vector3(120, 1.2, 120), Color(0.24, 0.39, 0.22), true)
    _make_box("CentralRoad", Vector3(0, 0.02, 0), Vector3(8, 0.08, 105), Color(0.25, 0.25, 0.23), false)
    _make_box("CrossRoad", Vector3(0, 0.04, 0), Vector3(105, 0.08, 7), Color(0.28, 0.27, 0.24), false)
    _build_districts()
    _build_trees()
    player = PlayerScript.new()
    player.name = "Player"
    player.position = Vector3(0, 1.1, 8)
    _build_player_visual(player)
    add_child(player)
    player.health_changed.connect(_on_health_changed)
    player.ammo_changed.connect(_on_ammo_changed)
    player.died.connect(_on_player_died)

func _make_material(color: Color) -> StandardMaterial3D:
    var material := StandardMaterial3D.new()
    material.albedo_color = color
    material.roughness = 0.92
    return material

func _make_box(label: String, pos: Vector3, size: Vector3, color: Color, collision: bool) -> Node3D:
    var body: Node3D
    if collision:
        body = StaticBody3D.new()
        var shape_node := CollisionShape3D.new()
        var shape := BoxShape3D.new()
        shape.size = size
        shape_node.shape = shape
        body.add_child(shape_node)
    else:
        body = Node3D.new()
    body.name = label
    body.position = pos
    var mesh_instance := MeshInstance3D.new()
    var mesh := BoxMesh.new()
    mesh.size = size
    mesh_instance.mesh = mesh
    mesh_instance.material_override = _make_material(color)
    body.add_child(mesh_instance)
    add_child(body)
    return body

func _make_actor_body(actor: CharacterBody3D, color: Color, bot: bool) -> void:
    # One capsule collider keeps movement inexpensive; visuals are a simple low-poly humanoid.
    var collision := CollisionShape3D.new()
    var capsule := CapsuleShape3D.new()
    capsule.radius = 0.42 if not bot else 0.43
    capsule.height = 1.85
    collision.shape = capsule
    actor.add_child(collision)

    var body_material := _make_material(color)
    var skin_material := _make_material(Color(0.16, 0.19, 0.23) if bot else Color(0.78, 0.55, 0.38))
    var dark_material := _make_material(Color(0.055, 0.065, 0.08))

    var torso := MeshInstance3D.new()
    var torso_mesh := BoxMesh.new()
    torso_mesh.size = Vector3(0.68, 0.78, 0.38)
    torso.mesh = torso_mesh
    torso.material_override = body_material
    torso.position = Vector3(0, 0.25, 0)
    actor.add_child(torso)

    var head := MeshInstance3D.new()
    var head_mesh := SphereMesh.new()
    head_mesh.radius = 0.25
    head_mesh.height = 0.5
    head.mesh = head_mesh
    head.material_override = dark_material if bot else skin_material
    head.position = Vector3(0, 0.88, 0)
    actor.add_child(head)

    if bot:
        var rifle := MeshInstance3D.new()
        var rifle_mesh := BoxMesh.new()
        rifle_mesh.size = Vector3(0.12, 0.13, 0.92)
        rifle.mesh = rifle_mesh
        rifle.material_override = dark_material
        rifle.position = Vector3(0.36, 0.12, -0.43)
        actor.add_child(rifle)

    for side in [-1.0, 1.0]:
        var arm := MeshInstance3D.new()
        var arm_mesh := CapsuleMesh.new()
        arm_mesh.radius = 0.13
        arm_mesh.height = 0.78
        arm.mesh = arm_mesh
        arm.material_override = body_material
        arm.position = Vector3(side * 0.46, 0.22, 0)
        arm.rotation_degrees.z = -side * 12.0
        actor.add_child(arm)

        var leg := MeshInstance3D.new()
        var leg_mesh := CapsuleMesh.new()
        leg_mesh.radius = 0.15
        leg_mesh.height = 0.78
        leg.mesh = leg_mesh
        leg.material_override = dark_material
        leg.position = Vector3(side * 0.19, -0.48, 0)
        actor.add_child(leg)

func _build_player_visual(actor: CharacterBody3D) -> void:
    _make_actor_body(actor, Color(0.12, 0.38, 0.72), false)
    var pivot := Node3D.new()
    pivot.name = "CameraPivot"
    pivot.position.y = 1.25
    actor.add_child(pivot)
    var arm := SpringArm3D.new()
    arm.name = "SpringArm3D"
    arm.spring_length = 5.0
    arm.margin = 0.25
    pivot.add_child(arm)
    var camera := Camera3D.new()
    camera.name = "Camera3D"
    camera.position.z = 0.0
    camera.current = true
    arm.add_child(camera)

func _build_districts() -> void:
    var names := ["SHIPYARD", "BULLSEYE", "OBSERVATORY", "PLANTATION", "MILL", "CAPE TOWN", "HANGAR", "CLOCK TOWER", "FACTORY", "PEAK", "POCHINOK", "SENTOSA"]
    var positions := [Vector3(-42,0,-38),Vector3(-14,0,-40),Vector3(18,0,-40),Vector3(42,0,-32),Vector3(-42,0,0),Vector3(-14,0,0),Vector3(18,0,0),Vector3(42,0,2),Vector3(-42,0,34),Vector3(-14,0,38),Vector3(18,0,36),Vector3(42,0,36)]
    for i in range(names.size()):
        var center: Vector3 = positions[i]
        for j in range(3):
            var offset := Vector3(-3.0 + float(j) * 3.2, 1.6, 0 if j == 1 else -4.0)
            var building := _make_box(names[i] + "_House_" + str(j), center + offset, Vector3(4.2, 3.2, 4.0), Color(0.58, 0.51, 0.39), true)
            _make_box(names[i] + "_Roof_" + str(j), center + offset + Vector3(0, 1.8, 0), Vector3(4.6, 0.35, 4.4), Color(0.27, 0.19, 0.15), false)
        var label := Label3D.new()
        label.text = names[i]
        label.position = center + Vector3(0, 5.0, 0)
        label.font_size = 42
        label.outline_size = 8
        label.modulate = Color(1, 1, 1)
        label.billboard = BaseMaterial3D.BILLBOARD_ENABLED
        add_child(label)

func _build_trees() -> void:
    for i in range(36):
        var angle := TAU * float(i) / 36.0
        var radius := rng.randf_range(48.0, 56.0)
        var p := Vector3(cos(angle) * radius, 0, sin(angle) * radius)
        var trunk := _make_box("TreeTrunk", p + Vector3(0, 1.1, 0), Vector3(0.55, 2.2, 0.55), Color(0.29, 0.18, 0.10), false)
        var crown := MeshInstance3D.new()
        var sphere := SphereMesh.new()
        sphere.radius = 1.8
        sphere.height = 3.6
        crown.mesh = sphere
        crown.position = p + Vector3(0, 3.1, 0)
        crown.material_override = _make_material(Color(0.12, 0.34, 0.14))
        add_child(crown)

func _spawn_bots() -> void:
    # Spread the 49 bots across the island and keep a clear buffer around the player.
    var spawn_points: Array[Vector3] = []
    for i in range(BOT_COUNT):
        var candidate := Vector3.ZERO
        var accepted := false
        for attempt in range(80):
            var angle := rng.randf_range(0.0, TAU)
            var radius := rng.randf_range(25.0, 49.0)
            candidate = Vector3(cos(angle) * radius, 1.0, sin(angle) * radius)
            var clear := candidate.distance_to(player.position) >= 18.0
            if clear:
                for existing in spawn_points:
                    if candidate.distance_to(existing) < 5.0:
                        clear = false
                        break
            if clear:
                accepted = true
                break
        if not accepted:
            var fallback_angle := TAU * float(i) / float(BOT_COUNT)
            candidate = Vector3(cos(fallback_angle) * 43.0, 1.0, sin(fallback_angle) * 43.0)
        spawn_points.append(candidate)

    for i in range(BOT_COUNT):
        var bot: CharacterBody3D = BotScript.new()
        bot.name = "VillainBot_%02d" % (i + 1)
        add_child(bot)
        bot.position = spawn_points[i]
        bot.target = player
        bot.died.connect(_on_bot_died)
        bot.move_speed = rng.randf_range(3.4, 4.1)
        _make_actor_body(bot, Color(0.10, 0.10, 0.13), true)
        _add_bot_visor(bot)

func _add_bot_visor(bot: Node3D) -> void:
    var visor := MeshInstance3D.new()
    var mesh := BoxMesh.new()
    mesh.size = Vector3(0.48, 0.13, 0.08)
    visor.mesh = mesh
    visor.position = Vector3(0, 0.45, -0.39)
    visor.material_override = _make_material(Color(0.9, 0.04, 0.03))
    bot.add_child(visor)

func _build_menu() -> void:
    var menu_canvas := CanvasLayer.new()
    add_child(menu_canvas)
    menu_layer = Control.new()
    menu_layer.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
    menu_canvas.add_child(menu_layer)
    var shade := ColorRect.new()
    shade.color = Color(0.025, 0.045, 0.065, 0.92)
    shade.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
    menu_layer.add_child(shade)
    var panel := PanelContainer.new()
    panel.custom_minimum_size = Vector2(480, 420)
    panel.set_anchors_preset(Control.PRESET_CENTER)
    panel.position = Vector2(-240, -210)
    menu_layer.add_child(panel)
    var column := VBoxContainer.new()
    column.add_theme_constant_override("separation", 18)
    panel.add_child(column)
    var title := Label.new()
    title.text = "FREE FIRE 2017\nCLASSIC REMASTERED"
    title.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
    title.add_theme_font_size_override("font_size", 34)
    column.add_child(title)
    var subtitle := Label.new()
    subtitle.text = "OFFLINE BATTLE ROYALE • GODOT PORT"
    subtitle.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
    column.add_child(subtitle)
    var start := Button.new()
    start.text = "OFFLINE SOLO  •  49 BOTS"
    start.custom_minimum_size.y = 64
    start.pressed.connect(start_match)
    column.add_child(start)
    var note := Label.new()
    note.text = "Classic-style island prototype\nWASD move • Shift sprint • Space jump\nF fire • R reload • drag screen to look"
    note.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
    column.add_child(note)

func _build_hud() -> void:
    var hud_canvas := CanvasLayer.new()
    add_child(hud_canvas)
    hud_layer = Control.new()
    hud_layer.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
    hud_canvas.add_child(hud_layer)
    health_label = _make_hud_label("HP: 200 / 200", Vector2(24, 18), Vector2(300, 48), 24)
    ammo_label = _make_hud_label("RIFLE  30 / 120", Vector2(24, 70), Vector2(300, 42), 21)
    bot_label = _make_hud_label("ENEMIES: 49", Vector2(0, 18), Vector2(300, 48), 24)
    bot_label.anchor_left = 1
    bot_label.anchor_right = 1
    bot_label.position.x = -320
    status_label = _make_hud_label("SURVIVE", Vector2(0, 68), Vector2(300, 42), 20)
    status_label.anchor_left = 0.5
    status_label.anchor_right = 0.5
    status_label.position.x = -150
    zone_label = _make_hud_label("ZONE CLOSES IN 00:45", Vector2(0, 108), Vector2(340, 36), 16)
    zone_label.anchor_left = 0.5
    zone_label.anchor_right = 0.5
    zone_label.position.x = -170
    # Keep the touch controls separated so they remain usable in landscape on phones.
    _make_mobile_button("FIRE", Vector2(-155, -150), Vector2(130, 110), _fire_pressed, _fire_released)
    _make_mobile_button("RELOAD", Vector2(-290, -230), Vector2(110, 60), func(): player.reload_weapon())
    _make_mobile_button("JUMP", Vector2(-290, -155), Vector2(110, 60), func(): _jump_player())
    _make_mobile_button("SPRINT", Vector2(-290, -80), Vector2(110, 60), func(): player.sprinting = not player.sprinting)
    _make_mobile_button("MEDKIT", Vector2(-425, -80), Vector2(115, 60), _use_medkit)
    inventory_label = _make_hud_label("MEDKITS: 0", Vector2(24, 116), Vector2(260, 38), 19)
    _make_move_pad()

func _make_move_pad() -> void:
    # Dynamic virtual joystick: touch movement is already tracked in _unhandled_input.
    move_pad = Panel.new()
    move_pad.name = "VirtualJoystick"
    move_pad.anchor_top = 1
    move_pad.anchor_bottom = 1
    move_pad.position = Vector2(35, -185)
    move_pad.size = Vector2(150, 150)
    move_pad.mouse_filter = Control.MOUSE_FILTER_IGNORE
    var style := StyleBoxFlat.new()
    style.bg_color = Color(0.04, 0.08, 0.12, 0.38)
    style.border_color = Color(0.85, 0.92, 1.0, 0.65)
    style.set_border_width_all(3)
    style.set_corner_radius_all(75)
    move_pad.add_theme_stylebox_override("panel", style)
    hud_layer.add_child(move_pad)

    move_knob = Panel.new()
    move_knob.name = "JoystickKnob"
    move_knob.size = Vector2(58, 58)
    move_knob.position = Vector2(46, 46)
    move_knob.mouse_filter = Control.MOUSE_FILTER_IGNORE
    var knob_style := StyleBoxFlat.new()
    knob_style.bg_color = Color(0.35, 0.72, 1.0, 0.78)
    knob_style.border_color = Color.WHITE
    knob_style.set_border_width_all(2)
    knob_style.set_corner_radius_all(29)
    move_knob.add_theme_stylebox_override("panel", knob_style)
    move_pad.add_child(move_knob)

func _make_hud_label(text_value: String, pos: Vector2, size: Vector2, font_size: int) -> Label:
    var label := Label.new()
    label.text = text_value
    label.position = pos
    label.size = size
    label.add_theme_font_size_override("font_size", font_size)
    label.add_theme_color_override("font_color", Color.WHITE)
    hud_layer.add_child(label)
    return label

func _make_mobile_button(text_value: String, pos: Vector2, size: Vector2, down: Callable, up: Callable = Callable()) -> void:
    var button := Button.new()
    button.text = text_value
    button.size = size
    button.position = pos
    button.anchor_left = 1
    button.anchor_right = 1
    button.anchor_top = 1
    button.anchor_bottom = 1
    button.add_theme_font_size_override("font_size", 20)
    button.button_down.connect(down)
    if up.is_valid():
        button.button_up.connect(up)
    hud_layer.add_child(button)

func _spawn_loot() -> void:
    # Lightweight proximity pickups: green medkits and yellow rifle-ammo crates.
    var pickup_points := [Vector3(-34, 0.7, -25), Vector3(-8, 0.7, -28), Vector3(25, 0.7, -24), Vector3(35, 0.7, 12), Vector3(-30, 0.7, 24), Vector3(8, 0.7, 28), Vector3(0, 0.7, -8), Vector3(0, 0.7, 18)]
    for i in range(pickup_points.size()):
        var is_medkit := i % 2 == 0
        var pickup := Area3D.new()
        pickup.name = "MedkitPickup" if is_medkit else "AmmoPickup"
        pickup.position = pickup_points[i]
        pickup.collision_layer = 0
        pickup.collision_mask = 1
        var shape_node := CollisionShape3D.new()
        var sphere := SphereShape3D.new()
        sphere.radius = 1.35
        shape_node.shape = sphere
        pickup.add_child(shape_node)
        var visual := MeshInstance3D.new()
        var mesh := BoxMesh.new()
        mesh.size = Vector3(0.65, 0.55, 0.65)
        visual.mesh = mesh
        visual.material_override = _make_material(Color(0.12, 0.82, 0.28) if is_medkit else Color(0.95, 0.72, 0.12))
        pickup.add_child(visual)
        add_child(pickup)
        pickup.body_entered.connect(func(body: Node3D):
            if body != player:
                return
            if is_medkit:
                medkit_count += 1
            else:
                player.reserve_ammo += 60
                player.ammo_changed.emit(player.ammo, player.reserve_ammo)
            if inventory_label:
                inventory_label.text = "MEDKITS: %d" % medkit_count
            pickup.queue_free()
        )

func _use_medkit() -> void:
    if medkit_count <= 0 or not is_instance_valid(player) or player.health >= player.MAX_HEALTH:
        return
    medkit_count -= 1
    player.heal(70.0)
    if inventory_label:
        inventory_label.text = "MEDKITS: %d" % medkit_count

func _fire_pressed() -> void:
    if player:
        player.firing = true

func _fire_released() -> void:
    if player:
        player.firing = false

func _jump_player() -> void:
    if player and player.is_on_floor():
        player.velocity.y = 5.2

func start_match() -> void:
    if match_started:
        return
    match_started = true
    menu_layer.hide()
    hud_layer.show()
    _spawn_bots()
    _spawn_loot()
    # One human plus 49 bots gives the intended 50-participant offline test match.
    var safe_zone: Node3D = Node3D.new()
    safe_zone.name = "SafeZone"
    safe_zone.set_script(SafeZoneScript)
    safe_zone.set("player", player)
    add_child(safe_zone)
    safe_zone_controller = safe_zone
    _update_zone_label()
    _update_bot_count()

func _process(delta: float) -> void:
    if match_started and is_instance_valid(player):
        _update_bot_count()
        zone_ui_timer += delta
        if zone_ui_timer >= 0.25:
            zone_ui_timer = 0.0
            _update_zone_label()
        if player.health <= 0:
            status_label.text = "ELIMINATED  •  KILLS: %d" % kills

func _update_zone_label() -> void:
    if not is_instance_valid(safe_zone_controller) or not safe_zone_controller.has_method("get_status_text"):
        return
    zone_label.text = str(safe_zone_controller.call("get_status_text"))

func _update_bot_count() -> void:
    var count := get_tree().get_nodes_in_group("bots").size()
    bot_label.text = "ENEMIES: %d" % count
    if count == 0:
        status_label.text = "BOOYAH!  •  KILLS: %d" % kills

func _on_health_changed(current: float, maximum: float) -> void:
    health_label.text = "HP: %d / %d" % [roundi(current), roundi(maximum)]

func _on_ammo_changed(current: int, reserve: int) -> void:
    ammo_label.text = "RIFLE  %d / %d" % [current, reserve]

func _on_player_died() -> void:
    status_label.text = "ELIMINATED"
    _show_game_over()

func _show_game_over() -> void:
    if game_over_panel:
        game_over_panel.show()
        return
    game_over_panel = PanelContainer.new()
    game_over_panel.set_anchors_preset(Control.PRESET_CENTER)
    game_over_panel.position = Vector2(-190, -125)
    game_over_panel.custom_minimum_size = Vector2(380, 250)
    hud_layer.add_child(game_over_panel)
    var column := VBoxContainer.new()
    column.add_theme_constant_override("separation", 16)
    game_over_panel.add_child(column)
    var title := Label.new()
    title.text = "MATCH OVER"
    title.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
    title.add_theme_font_size_override("font_size", 32)
    column.add_child(title)
    var result := Label.new()
    result.text = "KILLS: %d" % kills
    result.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
    column.add_child(result)
    var restart := Button.new()
    restart.text = "PLAY AGAIN"
    restart.custom_minimum_size.y = 58
    restart.pressed.connect(func(): get_tree().reload_current_scene())
    column.add_child(restart)

func _on_bot_died() -> void:
    kills += 1

func _unhandled_input(event: InputEvent) -> void:
    if not match_started or not is_instance_valid(player):
        return
    if event is InputEventScreenTouch:
        if event.pressed:
            if event.position.x < get_viewport().get_visible_rect().size.x * 0.42 and touch_move_id == -1:
                touch_move_id = event.index
                move_origin = event.position
                if move_knob:
                    move_knob.position = Vector2(46, 46)
            elif touch_look_id == -1:
                touch_look_id = event.index
        else:
            if event.index == touch_move_id:
                touch_move_id = -1
                virtual_move = Vector2.ZERO
                player.set_move(Vector2.ZERO)
                if move_knob:
                    move_knob.position = Vector2(46, 46)
            if event.index == touch_look_id:
                touch_look_id = -1
    elif event is InputEventScreenDrag:
        if event.index == touch_move_id:
            virtual_move = ((event.position - move_origin) / 75.0).limit_length(1.0)
            player.set_move(virtual_move)
            if move_knob:
                move_knob.position = Vector2(46, 46) + virtual_move * 42.0
        elif event.index == touch_look_id:
            player.look_by(event.relative)
    elif event is InputEventMouseMotion and Input.is_mouse_button_pressed(MOUSE_BUTTON_RIGHT):
        player.look_by(event.relative)
    elif event is InputEventMouseButton and event.button_index == MOUSE_BUTTON_RIGHT:
        Input.set_mouse_mode(Input.MOUSE_MODE_CAPTURED if event.pressed else Input.MOUSE_MODE_VISIBLE)
