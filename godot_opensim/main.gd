extends Spatial

var player_scene = preload("res://player.tscn")

func _ready():
    # The server is the only one that should spawn players.
    if get_tree().is_network_server():
        get_tree().multiplayer.peer_connected.connect(_on_player_connected)
        get_tree().multiplayer.peer_disconnected.connect(_on_player_disconnected)

func _on_player_connected(id):
    var player = player_scene.instantiate()
    player.name = str(id)
    add_child(player)
    # The authority is set on the server, and the client will automatically
    # have authority over its own player character.
    player.set_multiplayer_authority(id)

func _on_player_disconnected(id):
    var player = get_node_or_null(str(id))
    if player:
        player.queue_free()
