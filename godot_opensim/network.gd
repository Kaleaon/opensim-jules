extends Node

const PORT = 7777
const MAX_CLIENTS = 4

func _ready():
    # The network peer will be set by the menu script.
    pass

func start_server():
    var peer = MultiplayerPeerENet.new()
    var error = peer.create_server(PORT, MAX_CLIENTS)
    if error != OK:
        print("Failed to create server.")
        return
    get_tree().multiplayer.multiplayer_peer = peer

func start_client(ip_address):
    var peer = MultiplayerPeerENet.new()
    var error = peer.create_client(ip_address, PORT)
    if error != OK:
        print("Failed to create client.")
        return
    get_tree().multiplayer.multiplayer_peer = peer
