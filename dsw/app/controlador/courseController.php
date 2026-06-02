<?php
namespace app\controlador;

use app\modelo\Course;
use Firebase\JWT\JWT;
use Firebase\JWT\Key;

class courseController {
    private $course;
    private const JWT_SECRET = 'clave_secreta_dsw_2024';

    public function __construct() {
        $this->course = new Course();
    }

    public function showCourse() {
        require_once '../app/vista /vistaCurso/curso.html';
    }

    public function showCourses() {
        require_once '../app/vista /vistaTotalCursos/cursos.html';
    }

    public function showCreateCourse() {
        require_once '../app/vista /vistaCrearCurso/crear_curso.html';
    }

    public function listarCursos() {
        $payload = $this->obtenerPayload();

        $cursos = $this->course->obtenerCursosPorProfesor($payload->correo);

        return [
            'exito' => true,
            'cursos' => $cursos
        ];
    }

    public function crearCurso() {
        $payload = $this->obtenerPayload();
        $datos = json_decode(file_get_contents("php://input"), true);

        $nombreCurso = trim($datos['nombre'] ?? '');
        $contrasena  = trim($datos['contrasena'] ?? '');
        $codigo      = trim($datos['codigo'] ?? '');

        if ($nombreCurso === '' || $contrasena === '') {
            return [
                'exito' => false,
                'mensaje' => 'Por favor complete todos los campos.'
            ];
        }

        if ($codigo === '') {
            $codigo = $this->generarCodigoCurso();
        }

        $resultado = $this->course->crearCurso($nombreCurso, $codigo, $contrasena, $payload->correo);

        if ($resultado) {
            return [
                'exito' => true,
                'mensaje' => 'Curso creado correctamente.'
            ];
        }

        return [
            'exito' => false,
            'mensaje' => 'No fue posible crear el curso.'
        ];
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

    public function generarCodigoCurso() {
        $letras = 'ABCDEFGHIJKLMNOPQRSTUVWXYZ';
        $codigo = '';
        for ($i = 0; $i < 3; $i++) {
            $codigo .= $letras[random_int(0, 25)];
        }
        $codigo .= random_int(1000, 9999);
        return ['exito' => true, 'codigo' => $codigo];
    }

    public function listarCursosEstudiante() {
    $payload = $this->obtenerPayload();
    $cursos = $this->course->obtenerCursosPorEstudiante($payload->correo);
    return [
        'exito' => true,
        'cursos' => $cursos
        ];
    }

    public function unirseACurso() {
    $payload = $this->obtenerPayload();
    $datos = json_decode(file_get_contents("php://input"), true);

    $contrasena = trim($datos['contrasena'] ?? '');

    if ($contrasena === '') {
        return ['exito' => false, 'mensaje' => 'Ingresa la contraseña del curso.'];
    }

    $resultado = $this->course->unirseACurso($payload->correo, $contrasena);
    return $resultado;
    }

    /*public function verCurso() {
    return ['exito' => true, 'prueba' => 'funciona'];
}*/

    public function verCurso() {
        
        $payload = $this->obtenerPayload();
        $nombre = trim($_GET['nombre'] ?? '');

        if ($nombre === '') {
            return ['exito' => false, 'mensaje' => 'Debe indicar el nombre del curso.'];
        }

        $cursos = $this->course->obtenerCursosPorProfesor($payload -> correo);
        foreach ($cursos as $curso) {
            if ($curso['Nombre'] === $nombre) {
                return ['exito' => true, 'curso' => $curso];
            }
        }

        return ['exito' => false, 'mensaje' => 'Curso no encontrado.'];
}
}