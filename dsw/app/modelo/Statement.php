<?php
namespace app\modelo;
require_once __DIR__ . '/../config/db.php';

class Statement{

    private $pdo;

    public function __construct() {
        $this -> pdo = conectar();
    }

    public function crearEnunciado($titulo, $descripcion, $nombreCurso, $correoProfesor) {
        $consulta = $this->pdo->prepare("CALL CrearEnunciado(?,?,?,?)");
        return $consulta->execute([$titulo, $descripcion, $nombreCurso, $correoProfesor]);
    }

    public function obtenerEnunciadosPorCurso($nombreCurso, $correo) {
        $consulta = $this->pdo->prepare("CALL ObtenerEnunciadosPorCurso(?,?)");
        $consulta->execute([$nombreCurso, $correo]);
        
        return $consulta->fetchAll(\PDO::FETCH_ASSOC);
    }

    public function obtenerEstudiantesPorEnunciado($id){
        $consulta = $this ->pdo -> prepare("CALL ObtenerEstudiantesPorEnunciado(?)");
        $consulta ->execute([$id]);
        return $consulta -> fetchAll(\PDO::FETCH_ASSOC); 
    }

    public function editarEnunciado($id, $titulo, $descripcion) {
        $consulta = $this->pdo->prepare("CALL EditarEnunciado(?,?,?)");
        $consulta->execute([$id, $titulo, $descripcion]);
        return $consulta->fetch(\PDO::FETCH_ASSOC);
    }

    public function obtenerEnunciadoPorId($id){
        $consulta = $this->pdo->prepare("CALL ObtenerEnunciadoPorId(?)");
        $consulta->execute([$id]);
        return $consulta->fetch(\PDO::FETCH_ASSOC);
    }

    public function obtenerEnunciadosPorCursoEstudiante($nombreCurso, $correo, $correoProfesor) {
    $consulta = $this->pdo->prepare("CALL ObtenerEnunciadosPorCursoEstudiante(?,?,?)");
    $consulta->execute([$nombreCurso, $correo, $correoProfesor]);
    return $consulta->fetchAll(\PDO::FETCH_ASSOC);
    }



}
?>