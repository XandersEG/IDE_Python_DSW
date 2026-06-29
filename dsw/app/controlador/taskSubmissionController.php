<?php
namespace app\controlador;

use app\modelo\TaskSubmission;
use Firebase\JWT\JWT;
use Firebase\JWT\Key;

class taskSubmissionController {

    private $submission;
    private const JWT_SECRET = 'clave_secreta_dsw_DXAT';

    public function __construct() {
        $this->submission = new TaskSubmission();
    }
    

    public function registerSubmission() {
        $data = json_decode(file_get_contents('php://input'), true);
        $resultado = $this->submission->registerSubmission(
            $data['correoEstudiante'],
            $data['idEntrega']
        );
        echo json_encode([
                'exito'     => (bool)($resultado['exito'] ?? false),
                'mensaje'   => $resultado['mensaje'] ?? ''
            ]);
    }

    public function createSubmission() {
        $payload = $this->obtenerPayload();
        
        $data = json_decode(file_get_contents('php://input'), true);
        
        $resultado = $this->submission->createSubmission(
            $data['nombreArchivo'],
            $data['idEnunciado'],
            $data['contenido']
        );

        if ($resultado['exito'] == true) {
            $reg = $this->submission->registerSubmission(
                $payload->correo,
                $resultado['idEntrega']
            );

            echo json_encode([
                'exito'     => (bool)($reg['exito'] ?? false),
                'idEntrega' => (int)($resultado['idEntrega']),
                'mensaje'   => $reg['mensaje'] ?? ''
            ]);
            return;
        }
        
        echo json_encode($resultado);
    }

    public function downloadSubmission() {
        $idEntrega = $_GET['id'];
        $entrega = $this->submission->getSubmission($idEntrega);

        if (!$entrega || $entrega['Contenido'] === null) {
            echo json_encode(['exito' => false, 'mensaje' => 'Entrega no encontrada.']);
            return;
        }

        ob_clean();
        flush();
        header('Content-Type: application/zip');
        header('Content-Disposition: attachment; filename=" ' . $entrega['nombreArchivo'] . '.zip"');
        header('Content-Length: ' . strlen($entrega['Contenido']));
        echo $entrega['Contenido'];
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