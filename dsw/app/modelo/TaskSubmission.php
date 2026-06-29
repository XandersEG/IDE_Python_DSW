<?php
namespace app\modelo;
require_once __DIR__ . '/../config/db.php';

class TaskSubmission{
    private $pdo;
    
    public function __construct() {
        $this -> pdo = conectar();
    }

    public function registerSubmission($correoEstudiante, $idEntrega){
        $consulta = $this->pdo->prepare("CALL RegistrarEstudianteEnEntrega(?,?)");
        $consulta->execute([$correoEstudiante, $idEntrega]);
        $resultado = $consulta->fetch(\PDO::FETCH_ASSOC);
        return $resultado;
    }

    public function createSubmission($nombreArchivo, $idEnunciado, $contenidoBase64) {
        $contenido = base64_decode($contenidoBase64);

        $consulta = $this->pdo->prepare(
            "INSERT INTO Entrega
            (timeStamp, Contenido, idEnunciado, nombreArchivo)
            VALUES (NOW(), ?, ?, ?)"
        );

        $consulta->execute([
            $contenido,
            $idEnunciado,
            $nombreArchivo
        ]);

        return [
            'exito' => true,
            'idEntrega' =>
                $this->pdo->lastInsertId()
        ];
    }

    public function getSubmission($idEntrega) {
        $consulta = $this->pdo->prepare("CALL DescargarEntrega(?)");
        $consulta->execute([$idEntrega]);
        return $consulta->fetch(\PDO::FETCH_ASSOC);
    }

}
?>
