<?php
namespace app\modelo;
require_once __DIR__ . '/../config/db.php';

class Course {
    private $pdo;
    
    public function __construct() {
        $this -> pdo = conectar();
    }

    public function crearCurso($nombre, $codigo, $contrasena, $correoProfesor) {
        $consulta = $this->pdo->prepare("CALL CrearCurso(?,?,?,?)");
        return $consulta->execute([$nombre, $codigo, $contrasena, $correoProfesor]);
    }

    public function obtenerCursosPorProfesor($correo) {
        $consulta = $this->pdo->prepare("CALL ObtenerCursosPorProfesor(?)");
        $consulta->execute([$correo]);
        return $consulta->fetchAll(\PDO::FETCH_ASSOC);
    }

    public function obtenerCursosPorEstudiante($correo) {
        $consulta = $this->pdo->prepare("CALL ObtenerCursosPorEstudiante(?)");
        $consulta->execute([$correo]);
        return $consulta->fetchAll(\PDO::FETCH_ASSOC);
    }

    public function unirseACurso($correoEstudiante, $contrasena) {
    $consulta = $this->pdo->prepare("CALL UnirEstudianteACurso(?,?)");
    $consulta->execute([$correoEstudiante, $contrasena]);
    $resultado = $consulta->fetch(\PDO::FETCH_ASSOC);
    if ($resultado) {
        $resultado['exito'] = $resultado['exito'] === 'true';
    }
    return $resultado;
    }

}
?>
