extends Control

func _on_ConnectButton_pressed():
    var first_name = "Kaleaon"
    var last_name = "Engineer"
    var password = "R@nossian9897"
    # The correct login URI for OSGrid.
    var login_uri = "http://login.osgrid.org/"

    OpenSimClient.login_to_grid(first_name, last_name, password, login_uri)
