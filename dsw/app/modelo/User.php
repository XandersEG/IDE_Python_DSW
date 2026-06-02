<?php
namespace app\modelo;
require_once __DIR__ . '/../config/db.php';

class User{
    private $pdo;

    public function __construct() {
        $this -> pdo = conectar();
    }
    

    /*public function crearUsuario($correo, $primerNombre, $segundoNombre, $primerApellido, $segundoApellido, $contrasena) {
        $consulta = $this->pdo->prepare("CALL CrearProfesor(?,?,?,?,?,?)");
        $consulta->execute([$correo, $primerNombre, $segundoNombre, $primerApellido, $segundoApellido, $contrasena]);
        return $consulta->fetch(PDO::FETCH_ASSOC);
    }*/

    public function crearUsuario($correo, $primerNombre, $segundoNombre, $primerApellido, $segundoApellido, $contrasena) {
    try {
        $consulta = $this->pdo->prepare("CALL CrearUsuario(?,?,?,?,?,?)");
        $consulta->execute([$correo, $primerNombre, $segundoNombre, $primerApellido, $segundoApellido, $contrasena]);
        $resultado = $consulta->fetch(\PDO::FETCH_ASSOC);
        
        return $resultado;
    } catch (\PDOException $e) {
        
        return ['exito' => false, 'mensaje' => $e->getMessage()];
    }
}

    public function autenticar($correo, $contrasena) {
        $consulta = $this->pdo->prepare("CALL IniciarSesion(?,?)");
        $consulta->execute([$correo, $contrasena]);
        return $consulta->fetch(\PDO::FETCH_ASSOC);
    }

    
}
?>
