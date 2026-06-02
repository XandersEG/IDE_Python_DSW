<?php
namespace app\controlador;

use app\modelo\Statement;
use Firebase\JWT\JWT;
use Firebase\JWT\Key;

class statementController {
    private $statement;
    private const JWT_SECRET = 'clave_secreta_dsw_DXAT';

    public function __construct() {
        $this->statement = new Statement();
    }

    public function showTareas() {
        require_once '../app/vista /vistaTotalTareas/tareas.html';
    }

    public function showCrearTarea() {
        require_once '../app/vista /vistaCrearTarea/crear_tarea.html';
    }

    public function showTarea() {
        require_once '../app/vista /vistaTarea/tarea.html';
    }

    public function showTareaEstudiante() {
    require_once '../app/vista /vistaTarea/tareaEstudiante.html';
}
    

    public function listarTareas() {
        $payload = $this->obtenerPayload();
        $nombreCurso = trim($_GET['nombreCurso'] ?? '');

        if ($nombreCurso === '') {
            return [
                'exito' => false,
                'mensaje' => 'Debe indicar el curso.'
            ];
        }

        $tareas = $this->statement->obtenerEnunciadosPorCurso($nombreCurso, $payload->correo);

        return [
            'exito' => true,
            'tareas' => $tareas
        ];
    }

    public function crearTarea() {
        $payload = $this->obtenerPayload();
        $datos = json_decode(file_get_contents("php://input"), true);

        $titulo      = trim($datos['titulo']      ?? '');
        $descripcion = trim($datos['descripcion'] ?? '');
        $nombreCurso = trim($datos['nombreCurso'] ?? '');

        if ($titulo === '' || $descripcion === '' || $nombreCurso === '') {
            return [
                'exito' => false,
                'mensaje' => 'Por favor complete todos los campos.'
            ];
        }

        $resultado = $this->statement->crearEnunciado($titulo, $descripcion, $nombreCurso, $payload->correo);

        if ($resultado) {
            return [
                'exito' => true,
                'mensaje' => 'Tarea creada correctamente.'
            ];
        }

        return [
            'exito' => false,
            'mensaje' => 'No fue posible crear la tarea.'
        ];
    }

    public function verTarea() {
        $this->obtenerPayload();
        $id = trim($_GET['id'] ?? '');

        if ($id === '') {
            return [
                'exito' => false,
                'mensaje' => 'Debe indicar la tarea.'
            ];
        }

        $tarea = $this->statement->obtenerEnunciadoPorId($id);

        if (!$tarea) {
            return [
                'exito' => false,
                'mensaje' => 'Tarea no encontrada.'
            ];
        }

        $estudiantes = $this->statement->obtenerEstudiantesPorEnunciado($id);

        return [
            'exito' => true,
            'tarea' => $tarea,
            'estudiantes' => $estudiantes
        ];
    }

    public function listarTareasEstudiante() {
        $payload = $this->obtenerPayload();
        $nombreCurso = trim($_GET['nombreCurso'] ?? '');

        if ($nombreCurso === '') {
            return ['exito' => false, 'mensaje' => 'Debe indicar el curso.'];
        }

        $correoProfesor = trim($_GET['correoProfesor'] ?? '');
        $tareas = $this->statement->obtenerEnunciadosPorCursoEstudiante($nombreCurso, $payload->correo, $correoProfesor);

        return [
            'exito' => true,
            'tareas' => $tareas
        ];
    }

    public function editarTarea() {
        $this->obtenerPayload();
        $datos = json_decode(file_get_contents("php://input"), true);

        $id          = trim($datos['id']          ?? '');
        $titulo      = trim($datos['titulo']      ?? '');
        $descripcion = trim($datos['descripcion'] ?? '');

        if ($id === '' || $titulo === '' || $descripcion === '') {
            return ['exito' => false, 'mensaje' => 'Por favor complete todos los campos.'];
        }

        $resultado = $this->statement->editarEnunciado($id, $titulo, $descripcion);
        return $resultado;
    }

    private function obtenerPayload() {
    try {
        $headers   = getallheaders();
        $token     = str_replace('Bearer ', '', $headers['Authorization'] ?? '');
        $secretKey = 'clave_secreta_dsw_2024';
        return JWT::decode($token, new Key($secretKey, 'HS256'));
    } catch (\Exception $e) {
        http_response_code(401);
        header('Content-Type: application/json');
        echo json_encode(['exito' => false, 'mensaje' => 'Token expirado o inválido']);
        exit;
    }
}
    
}
?>