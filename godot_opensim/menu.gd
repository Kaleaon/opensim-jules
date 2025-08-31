extends Control

@onready var network = get_tree().get_root().get_node("network")
@onready var ip_address_edit = $VBoxContainer/IPAddressEdit

func _on_StartServerButton_pressed():
    network.start_server()
    get_tree().change_scene_to_file("res://main.tscn")

func _on_StartClientButton_pressed():
    var ip = ip_address_edit.text
    if ip.is_valid_ip_address():
        network.start_client(ip)
        get_tree().change_scene_to_file("res://main.tscn")
    else:
        print("Invalid IP address entered.")
