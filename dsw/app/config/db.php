<?php
function conectar() {
    $host     = 'localhost';
    $dbname   = 'DB_IDE_Python_DSW';
    $user     = 'root';
    $password = 'dsw123';

    try {
        $pdo = new PDO("mysql:host=$host;dbname=$dbname;charset=utf8", $user, $password);
        $pdo->setAttribute(PDO::ATTR_ERRMODE, PDO::ERRMODE_EXCEPTION);
        return $pdo;
    } catch (PDOException $e) {
        die(json_encode([
            'exito'   => false,
            'mensaje' => 'Error de conexión: ' . $e->getMessage()
        ]));
    }
}
?>