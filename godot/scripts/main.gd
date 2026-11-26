extends Node3D

const PLAYER_SPEED := 6.0
const SPRINT_SPEED := 9.0
const ENEMY_SPEED := 3.4

var player: CharacterBody3D
var chaser: CharacterBody3D
var camera: Camera3D
var score := 0
var xp := 0
var level := 1
var coins := 0
var caught := false
var escaped := false
var hiding := false
var sprinting := false
var move_axis := Vector2.ZERO
var hud_label: Label
var message_label: Label
var hide_button: Button
var sprint_button: Button

func _ready() -> void:
	_build_world()
	_build_actors()
	_build_camera()
	_build_hud()
	_spawn_coins()
	_update_hud()

func _build_world() -> void:
	var env := WorldEnvironment.new()
	var environment := Environment.new()
	environment.background_mode = Environment.BG_COLOR
	environment.background_color = Color(0.055, 0.075, 0.09)
	environment.ambient_light_source = Environment.AMBIENT_SOURCE_COLOR
	environment.ambient_light_color = Color(0.72, 0.78, 0.85)
	environment.ambient_light_energy = 0.8
	env.environment = environment
	add_child(env)

	var light := DirectionalLight3D.new()
	light.rotation_degrees = Vector3(-48, -28, 0)
	light.light_energy = 1.15
	add_child(light)

	_box("House Floor", Vector3(0, -0.25, 0), Vector3(30, 0.5, 22), Color(0.48, 0.39, 0.29), true)
	_box("Back Wall", Vector3(0, 2.2, -11), Vector3(30, 4.4, 0.4), Color(0.77, 0.71, 0.61), true)
	_box("Front Wall Left", Vector3(-10, 2.2, 11), Vector3(10, 4.4, 0.4), Color(0.77, 0.71, 0.61), true)
	_box("Front Wall Right", Vector3(10, 2.2, 11), Vector3(10, 4.4, 0.4), Color(0.77, 0.71, 0.61), true)
	_box("Left Wall", Vector3(-15, 2.2, 0), Vector3(0.4, 4.4, 22), Color(0.77, 0.71, 0.61), true)
	_box("Right Wall", Vector3(15, 2.2, 0), Vector3(0.4, 4.4, 22), Color(0.77, 0.71, 0.61), true)

	# Room dividers and furniture create four connected areas.
	_box("Living Room Divider", Vector3(-5, 1.6, -3), Vector3(0.25, 3.2, 7), Color(0.68, 0.61, 0.51), true)
	_box("Kitchen Divider", Vector3(5, 1.6, -3), Vector3(0.25, 3.2, 7), Color(0.68, 0.61, 0.51), true)
	_box("Bedroom Divider", Vector3(-5, 1.6, 5), Vector3(0.25, 3.2, 7), Color(0.68, 0.61, 0.51), true)
	_box("Storage Divider", Vector3(5, 1.6, 5), Vector3(0.25, 3.2, 7), Color(0.68, 0.61, 0.51), true)

	_box("Sofa Hideout", Vector3(-10, 0.8, -5), Vector3(3.2, 1.6, 1.4), Color(0.22, 0.34, 0.42), true)
	_box("Kitchen Counter", Vector3(10, 0.8, -5), Vector3(3.5, 1.6, 1.3), Color(0.42, 0.31, 0.22), true)
	_box("Bed Hideout", Vector3(-10, 0.65, 7), Vector3(3.5, 1.3, 2.4), Color(0.35, 0.43, 0.56), true)
	_box("Storage Crates", Vector3(10, 0.8, 7), Vector3(2.6, 1.6, 2.6), Color(0.39, 0.27, 0.16), true)
	_box("Dining Table", Vector3(0, 0.9, -7), Vector3(3.2, 1.8, 1.8), Color(0.39, 0.25, 0.15), true)

func _box(label_text: String, pos: Vector3, size: Vector3, color: Color, solid: bool) -> void:
	var root: Node3D
	if solid:
		var body := StaticBody3D.new()
		body.name = label_text
		root = body
		var collision := CollisionShape3D.new()
		var shape := BoxShape3D.new()
		shape.size = size
		collision.shape = shape
		body.add_child(collision)
	else:
		root = Node3D.new()
	root.position = pos
	var mesh_instance := MeshInstance3D.new()
	var mesh := BoxMesh.new()
	mesh.size = size
	mesh_instance.mesh = mesh
	var material := StandardMaterial3D.new()
	material.albedo_color = color
	material.roughness = 0.88
	mesh_instance.material_override = material
	root.add_child(mesh_instance)
	add_child(root)

func _build_actors() -> void:
	player = CharacterBody3D.new()
	player.name = "Gecko"
	player.position = Vector3(0, 0.8, 8)
	var player_shape := CollisionShape3D.new()
	var capsule := CapsuleShape3D.new()
	capsule.radius = 0.38
	capsule.height = 1.0
	player_shape.shape = capsule
	player.add_child(player_shape)
	var gecko_mesh := MeshInstance3D.new()
	var gecko_capsule := CapsuleMesh.new()
	gecko_capsule.radius = 0.38
	gecko_capsule.height = 1.0
	gecko_mesh.mesh = gecko_capsule
	var gecko_mat := StandardMaterial3D.new()
	gecko_mat.albedo_color = Color(0.24, 0.78, 0.39)
	gecko_mesh.material_override = gecko_mat
	player.add_child(gecko_mesh)
	add_child(player)

	chaser = CharacterBody3D.new()
	chaser.name = "Human Chaser"
	chaser.position = Vector3(0, 0.9, -8)
	var human_shape := CollisionShape3D.new()
	var human_capsule := CapsuleShape3D.new()
	human_capsule.radius = 0.52
	human_capsule.height = 1.8
	human_shape.shape = human_capsule
	chaser.add_child(human_shape)
	var human_mesh := MeshInstance3D.new()
	var human_visual := CapsuleMesh.new()
	human_visual.radius = 0.52
	human_visual.height = 1.8
	human_mesh.mesh = human_visual
	var human_mat := StandardMaterial3D.new()
	human_mat.albedo_color = Color(0.76, 0.35, 0.27)
	human_mesh.material_override = human_mat
	chaser.add_child(human_mesh)
	add_child(chaser)

func _build_camera() -> void:
	camera = Camera3D.new()
	camera.position = Vector3(0, 10, 15)
	camera.current = true
	add_child(camera)
	camera.look_at(Vector3(0, 0, 0), Vector3.UP)

func _spawn_coins() -> void:
	for p in [Vector3(-11, 0.6, -8), Vector3(10, 0.6, -8), Vector3(-11, 0.6, 8), Vector3(10, 0.6, 8), Vector3(0, 0.6, 0)]:
		var coin := Area3D.new()
		coin.position = p
		var shape_node := CollisionShape3D.new()
		var sphere := SphereShape3D.new()
		sphere.radius = 0.75
		shape_node.shape = sphere
		coin.add_child(shape_node)
		var visual := MeshInstance3D.new()
		var sphere_mesh := SphereMesh.new()
		sphere_mesh.radius = 0.28
		sphere_mesh.height = 0.56
		visual.mesh = sphere_mesh
		var gold := StandardMaterial3D.new()
		gold.albedo_color = Color(1.0, 0.72, 0.12)
		gold.metallic = 0.55
		visual.material_override = gold
		coin.add_child(visual)
		coin.body_entered.connect(_on_coin_body_entered.bind(coin))
		add_child(coin)

func _on_coin_body_entered(body: Node3D, coin: Area3D) -> void:
	if body == player and is_instance_valid(coin):
		coins += 1
		score += 100
		xp += 25
		if xp >= level * 100:
			xp -= level * 100
			level += 1
			_show_message("LEVEL UP! LEVEL %d" % level)
		coin.queue_free()
		_update_hud()

func _build_hud() -> void:
	var layer := CanvasLayer.new()
	add_child(layer)
	var panel := PanelContainer.new()
	panel.position = Vector2(18, 14)
	panel.custom_minimum_size = Vector2(300, 78)
	layer.add_child(panel)
	hud_label = Label.new()
	hud_label.add_theme_font_size_override("font_size", 19)
	panel.add_child(hud_label)

	message_label = Label.new()
	message_label.position = Vector2(330, 20)
	message_label.add_theme_font_size_override("font_size", 22)
	layer.add_child(message_label)

	var left := _make_button(layer, "◀", Vector2(24, 420), Vector2(76, 76))
	left.button_down.connect(func(): move_axis.x = -1.0)
	left.button_up.connect(func(): if move_axis.x < 0: move_axis.x = 0.0)
	var right := _make_button(layer, "▶", Vector2(190, 420), Vector2(76, 76))
	right.button_down.connect(func(): move_axis.x = 1.0)
	right.button_up.connect(func(): if move_axis.x > 0: move_axis.x = 0.0)
	var forward := _make_button(layer, "▲", Vector2(107, 340), Vector2(76, 76))
	forward.button_down.connect(func(): move_axis.y = -1.0)
	forward.button_up.connect(func(): if move_axis.y < 0: move_axis.y = 0.0)
	var back := _make_button(layer, "▼", Vector2(107, 420), Vector2(76, 76))
	back.button_down.connect(func(): move_axis.y = 1.0)
	back.button_up.connect(func(): if move_axis.y > 0: move_axis.y = 0.0)

	sprint_button = _make_button(layer, "SPRINT", Vector2(790, 425), Vector2(145, 64))
	sprint_button.button_down.connect(func(): sprinting = true)
	sprint_button.button_up.connect(func(): sprinting = false)
	hide_button = _make_button(layer, "HIDE", Vector2(790, 345), Vector2(145, 64))
	hide_button.pressed.connect(_toggle_hide)

func _make_button(parent: Control, text_value: String, pos: Vector2, size: Vector2) -> Button:
	var button := Button.new()
	button.text = text_value
	button.position = pos
	button.size = size
	button.add_theme_font_size_override("font_size", 20)
	parent.add_child(button)
	return button

func _toggle_hide() -> void:
	hiding = not hiding
	if hiding:
		_show_message("BERSEMBUNYI")
	else:
		_show_message("LANJUT LARI")

func _show_message(value: String) -> void:
	message_label.text = value
	get_tree().create_timer(2.0).timeout.connect(func(): if is_instance_valid(message_label): message_label.text = "")

func _update_hud() -> void:
	if hud_label:
		hud_label.text = "RUN CICAAK   |   SCORE %d\nCOIN %d   XP %d/%d   LEVEL %d" % [score, coins, xp, level * 100, level]

func _physics_process(_delta: float) -> void:
	if caught or escaped:
		return
	var keyboard := Input.get_vector("ui_left", "ui_right", "ui_up", "ui_down")
	var direction := Vector3(move_axis.x + keyboard.x, 0, move_axis.y + keyboard.y)
	if direction.length() > 1.0:
		direction = direction.normalized()
	var speed := SPRINT_SPEED if sprinting else PLAYER_SPEED
	player.velocity.x = direction.x * speed
	player.velocity.z = direction.z * speed
	if not player.is_on_floor():
		player.velocity.y -= 18.0 * get_physics_process_delta_time()
	else:
		player.velocity.y = 0
	player.move_and_slide()
	player.position.x = clampf(player.position.x, -13.5, 13.5)
	player.position.z = clampf(player.position.z, -9.5, 9.5)

	if not hiding:
		var chase_dir := player.global_position - chaser.global_position
		chase_dir.y = 0
		if chase_dir.length() > 0.8:
			chase_dir = chase_dir.normalized()
			chaser.velocity.x = chase_dir.x * ENEMY_SPEED
			chaser.velocity.z = chase_dir.z * ENEMY_SPEED
			chaser.move_and_slide()
			chaser.look_at(Vector3(player.global_position.x, chaser.global_position.y, player.global_position.z), Vector3.UP)
		if chaser.global_position.distance_to(player.global_position) < 1.25:
			caught = true
			_show_message("KETANGKAP! COBA LAGI")
			await get_tree().create_timer(1.5).timeout
			get_tree().reload_current_scene()

	if player.global_position.z < -9.0 and player.global_position.x > -2.0 and player.global_position.x < 2.0:
		escaped = true
		score += 500
		xp += 100
		_show_message("BERHASIL KABUR! +500 SCORE")
		_update_hud()
