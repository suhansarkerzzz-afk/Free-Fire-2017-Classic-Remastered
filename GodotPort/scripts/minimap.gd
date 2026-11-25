extends Control

# Lightweight top-down HUD map for the current 120 m x 120 m procedural island.
# Redraw at 5 Hz rather than every frame to keep the 2 GB profile inexpensive.
const WORLD_HALF_EXTENT := 60.0
const REFRESH_INTERVAL := 0.2

const DISTRICT_NAMES := [
    "SHIPYARD",
    "BULLSEYE",
    "OBSERVATORY",
    "PLANTATION",
    "MILL",
    "CAPE TOWN",
    "HANGAR",
    "CLOCK TOWER",
    "FACTORY",
    "PEAK",
    "POCHINOK",
    "SENTOSA"
]

# Keep these positions in sync with main.gd::_build_districts().
const DISTRICT_POSITIONS := [
    Vector2(-42.0, -38.0),
    Vector2(-14.0, -40.0),
    Vector2(18.0, -40.0),
    Vector2(42.0, -32.0),
    Vector2(-42.0, 0.0),
    Vector2(-14.0, 0.0),
    Vector2(18.0, 0.0),
    Vector2(42.0, 2.0),
    Vector2(-42.0, 34.0),
    Vector2(-14.0, 38.0),
    Vector2(18.0, 36.0),
    Vector2(42.0, 36.0)
]

const PLANE_ROUTE_START := Vector3(-58.0, 0.0, 0.0)
const PLANE_ROUTE_END := Vector3(58.0, 0.0, 0.0)

var player: Node3D
var safe_zone: Node3D
var plane: Node3D
var map_font: Font
var refresh_elapsed := 0.0

func _ready() -> void:
    mouse_filter = Control.MOUSE_FILTER_IGNORE
    clip_contents = true
    map_font = ThemeDB.fallback_font
    queue_redraw()

func set_world_refs(player_node: Node3D, safe_zone_node: Node3D) -> void:
    player = player_node
    safe_zone = safe_zone_node
    queue_redraw()

func set_plane(plane_node: Node3D) -> void:
    plane = plane_node
    queue_redraw()

func _process(delta: float) -> void:
    if not is_visible_in_tree():
        return
    refresh_elapsed += delta
    if refresh_elapsed >= REFRESH_INTERVAL:
        refresh_elapsed = 0.0
        queue_redraw()

func _draw() -> void:
    if size.x < 1.0 or size.y < 1.0:
        return

    draw_rect(Rect2(Vector2.ZERO, size), Color(0.025, 0.045, 0.055, 0.94), true)
    draw_rect(Rect2(Vector2.ZERO, size), Color(0.72, 0.82, 0.72, 0.95), false, 2.0)

    if map_font != null:
        draw_string(
            map_font,
            Vector2(8.0, 15.0),
            "ISLAND MAP  •  N ↑",
            HORIZONTAL_ALIGNMENT_LEFT,
            -1.0,
            10,
            Color(0.94, 0.97, 0.88)
        )

    var map_side := minf(size.x - 16.0, size.y - 30.0)
    if map_side <= 0.0:
        return
    var map_rect := Rect2(
        Vector2((size.x - map_side) * 0.5, 22.0),
        Vector2(map_side, map_side)
    )

    draw_rect(map_rect, Color(0.15, 0.29, 0.18, 0.98), true)
    for grid_index in range(1, 4):
        var fraction := float(grid_index) * 0.25
        var grid_x := lerpf(map_rect.position.x, map_rect.end.x, fraction)
        var grid_y := lerpf(map_rect.position.y, map_rect.end.y, fraction)
        draw_line(
            Vector2(grid_x, map_rect.position.y),
            Vector2(grid_x, map_rect.end.y),
            Color(0.72, 0.82, 0.65, 0.16),
            1.0
        )
        draw_line(
            Vector2(map_rect.position.x, grid_y),
            Vector2(map_rect.end.x, grid_y),
            Color(0.72, 0.82, 0.65, 0.16),
            1.0
        )

    # The existing island uses a central north-south road and a crossroad.
    var road_center := _world_to_map(Vector3.ZERO, map_rect)
    draw_line(
        Vector2(road_center.x, map_rect.position.y),
        Vector2(road_center.x, map_rect.end.y),
        Color(0.24, 0.25, 0.22, 0.9),
        3.0
    )
    draw_line(
        Vector2(map_rect.position.x, road_center.y),
        Vector2(map_rect.end.x, road_center.y),
        Color(0.24, 0.25, 0.22, 0.9),
        3.0
    )

    # Show the full transport-plane route even before the plane begins moving.
    var route_start := _world_to_map(PLANE_ROUTE_START, map_rect)
    var route_end := _world_to_map(PLANE_ROUTE_END, map_rect)
    draw_line(route_start, route_end, Color(1.0, 0.62, 0.20, 0.88), 1.5)

    if is_instance_valid(safe_zone):
        var zone_radius := float(safe_zone.get("current_radius"))
        var radius_pixels := zone_radius / (WORLD_HALF_EXTENT * 2.0) * map_rect.size.x
        draw_arc(
            _world_to_map(safe_zone.global_position, map_rect),
            radius_pixels,
            0.0,
            TAU,
            48,
            Color(0.22, 0.78, 1.0, 0.95),
            1.6,
            true
        )

    var font_size := clampi(roundi(size.x / 27.0), 6, 8)
    for index in range(DISTRICT_NAMES.size()):
        var district := DISTRICT_POSITIONS[index]
        var marker := _world_to_map(Vector3(district.x, 0.0, district.y), map_rect)
        draw_circle(marker, 2.0, Color(0.96, 0.91, 0.70, 1.0))
        if map_font != null:
            var district_name: String = DISTRICT_NAMES[index]
            var text_size := map_font.get_string_size(
                district_name,
                HORIZONTAL_ALIGNMENT_LEFT,
                -1.0,
                font_size
            )
            var label_x := clampf(
                marker.x - text_size.x * 0.5,
                map_rect.position.x + 1.0,
                map_rect.end.x - text_size.x - 1.0
            )
            var label_y := marker.y + font_size + 3.0
            if label_y > map_rect.end.y - 2.0:
                label_y = marker.y - 3.0
            draw_string(
                map_font,
                Vector2(label_x, label_y),
                district_name,
                HORIZONTAL_ALIGNMENT_LEFT,
                -1.0,
                font_size,
                Color(0.96, 0.96, 0.86, 0.96)
            )

    # Bot markers are refreshed only five times per second.
    for bot in get_tree().get_nodes_in_group("bots"):
        if not is_instance_valid(bot) or not (bot is Node3D):
            continue
        if bool(bot.get("dead")):
            continue
        draw_circle(
            _world_to_map((bot as Node3D).global_position, map_rect),
            1.8,
            Color(1.0, 0.24, 0.18, 0.96)
        )

    if is_instance_valid(plane):
        var plane_marker := _world_to_map(plane.global_position, map_rect)
        var plane_diamond := PackedVector2Array([
            plane_marker + Vector2(0.0, -4.5),
            plane_marker + Vector2(4.5, 0.0),
            plane_marker + Vector2(0.0, 4.5),
            plane_marker + Vector2(-4.5, 0.0)
        ])
        draw_colored_polygon(plane_diamond, Color(1.0, 0.55, 0.12, 1.0))

    if is_instance_valid(player):
        var player_marker := _world_to_map(player.global_position, map_rect)
        var forward := -player.global_basis.z
        var direction := Vector2(forward.x, -forward.z)
        if direction.length_squared() < 0.001:
            direction = Vector2.UP
        else:
            direction = direction.normalized()
        var side := Vector2(-direction.y, direction.x)
        var player_arrow := PackedVector2Array([
            player_marker + direction * 6.5,
            player_marker - direction * 4.0 + side * 4.0,
            player_marker - direction * 4.0 - side * 4.0
        ])
        draw_colored_polygon(player_arrow, Color(1.0, 0.94, 0.25, 1.0))
        draw_circle(player_marker, 2.0, Color(0.08, 0.10, 0.08, 1.0))

    draw_rect(map_rect, Color(0.82, 0.88, 0.72, 0.9), false, 1.0)

func _world_to_map(world_position: Vector3, map_rect: Rect2) -> Vector2:
    var world_width := WORLD_HALF_EXTENT * 2.0
    var x_ratio := clampf((world_position.x + WORLD_HALF_EXTENT) / world_width, 0.0, 1.0)
    # Negative world Z is north and appears at the top of the map.
    var y_ratio := clampf((WORLD_HALF_EXTENT - world_position.z) / world_width, 0.0, 1.0)
    return map_rect.position + Vector2(
        x_ratio * map_rect.size.x,
        y_ratio * map_rect.size.y
    )
