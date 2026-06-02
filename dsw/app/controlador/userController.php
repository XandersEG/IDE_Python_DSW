<?php
namespace app\controlador;



use app\modelo\User;
use Firebase\JWT\JWT;
use Firebase\JWT\Key;


class userController {

    private $user;

    public function __construct() {
        $this->user = new User();
    }

    public function showLogin() {
        require_once '../app/vista /vistaLogin/login.html';
    }

    public function showRegister(){
         require_once '../app/vista /vistaRegistros/registro.html';
    }


    public function registerUser() {
        $datos = json_decode(file_get_contents("php://input"), true);

        $primerNombre    = trim($datos['primerNombre']        ?? '');
        $segundoNombre   = trim($datos['segundoNombre']       ?? '');
        $primerApellido  = trim($datos['primerApellido']      ?? '');
        $segundoApellido = trim($datos['segundoApellido']     ?? '');
        $correo          = trim($datos['correo']              ?? '');
        $contrasena      = trim($datos['contrasena']          ?? '');
        $confirmar       = trim($datos['confirmarContrasena'] ?? '');

        if (!$primerNombre || !$primerApellido || !$correo || !$contrasena) {
            return ['exito' => false, 'mensaje' => 'Campos obligatorios incompletos'];
        }

        if ($contrasena !== $confirmar) {
            return ['exito' => false,
             'mensaje' => 'Las contraseñas no coinciden.', 
             ];
        }

        $resultado = $this->user->crearUsuario(
            $correo,
            $primerNombre,
            $segundoNombre,
            $primerApellido,
            $segundoApellido,
            $contrasena
        );


        if ($resultado['exito']) {
            return [
                'exito'          => true,
                'mensaje'        => $resultado['mensaje'],
                'correo'         => $correo
            ];
        }

        return ['exito' => false, 'mensaje' => $resultado['mensaje']];
    }

    public function loginUser() {
        $datos = json_decode(file_get_contents("php://input"), true);

        $correo     = trim($datos['correo']     ?? '');
        $contrasena = trim($datos['contrasena'] ?? '');

        if (!$correo || !$contrasena) {
            return ['exito' => false, 'mensaje' => 'Campos obligatorios incompletos'];
        }

        $resultado = $this->user->autenticar($correo, $contrasena);

        // Si no devuelve nada, las credenciales son incorrectas
        if (!$resultado) {
            return ['exito' => false, 'mensaje' => 'Correo o contraseña incorrectos'];
        }

        $secretKey = 'clave_secreta_dsw_2024';

        $payload = [
            'iss'    => 'dsw',
            'iat'    => time(),
            'exp'    => time() + 3600,
            'correo' => $resultado['Correo'],
            'nombre' => $resultado['PrimerNombre'],
        ];

        $datos = [
            'correo' => $resultado['Correo'],
            'nombre' => $resultado['PrimerNombre'],
            'apellido' => $resultado['PrimerApellido']
        ];

        $token = JWT::encode($payload, $secretKey, 'HS256');

        return [
            'exito'  => true,
            'mensaje' => 'Login exitoso',
            'token'  => $token,
            'datos' => $datos
        ];
    }
}
?>