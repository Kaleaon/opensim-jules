extends Node

var http_request = HTTPRequest.new()

func _ready():
    add_child(http_request)
    http_request.request_completed.connect(_on_login_request_completed)

func login_to_grid(first_name, last_name, password, login_uri):
    var password_md5 = password.md5_text()
    var final_password = "$1$" + password_md5

    var xml_body = """
<?xml version="1.0"?>
<methodCall>
  <methodName>login_to_simulator</methodName>
  <params>
    <param>
      <value>
        <struct>
          <member><name>first</name><value><string>{first}</string></value></member>
          <member><name>last</name><value><string>{last}</string></value></member>
          <member><name>passwd</name><value><string>{passwd}</string></value></member>
          <member><name>start</name><value><string>last</string></value></member>
          <member><name>version</name><value><string>Godot OpenSim Client 0.1</string></value></member>
        </struct>
      </value>
    </param>
  </params>
</methodCall>
""".format({"first": first_name, "last": last_name, "passwd": final_password})

    var headers = ["Content-Type: application/xml"]

    var error = http_request.request(login_uri, headers, HTTPClient.METHOD_POST, xml_body)
    if error != OK:
        print("Error making XML-RPC request: " + str(error))

func _on_login_request_completed(_result, _response_code, _headers, body):
    print("Login request completed!")
    var response_body_string = body.get_string_from_utf8()
    print("Login response body: " + response_body_string)
