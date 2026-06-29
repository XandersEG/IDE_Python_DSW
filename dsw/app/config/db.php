<?php
function conectar() {
    $host     = 'Your host';
    $dbname   = 'Your DB name';
    $user     = 'Your user';
    $password = 'Your password';

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
