<?php 

ini_set('display_errors', 1);
ini_set('display_startup_errors', 1);
error_reporting(E_ALL);

//cargar clases automaticamente
require_once '../libreria/autoloader.php';

//cargar el sistema de las rutas
require_once '../libreria/Route.php';

//cargar las rutas definidas de la web
require_once '../routes/routesWeb.php';
require_once '../vendor/autoload.php'; 

// Servir archivos estáticos desde /public
$uri = parse_url($_SERVER['REQUEST_URI'], PHP_URL_PATH) ?? '/';
$uri = trim($uri, '/');
$uri = str_replace('dsw/public', '', $uri);
$uri = trim($uri, '/');

if ($uri !== '') {
    $file = '/var/www/html/dsw/public/' . $uri;
    if (file_exists($file)) {
        $ext = pathinfo($file, PATHINFO_EXTENSION);
        $tipos = [
            'css' => 'text/css',
            'js'  => 'application/javascript',
            'png' => 'image/png',
            'jpg' => 'image/jpeg',
        ];
        if (isset($tipos[$ext])) {
            header('Content-Type: ' . $tipos[$ext]);
        }
        readfile($file);
        exit;
    }
}


//procesa la petición
libreria\Route::dispatch();

?>
