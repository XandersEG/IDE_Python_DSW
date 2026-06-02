<?php
namespace Libreria;

class Route {


    private static $routes = [];

    public static function get($uri, $callback) {
        self::$routes['GET'][$uri] = $callback;  
    }

    public static function post($uri, $callback) {
        self::$routes['POST'][$uri] = $callback;  
    }

    public static function dispatch() {
        
        $metodo = $_SERVER['REQUEST_METHOD'];
        $uri = parse_url($_SERVER['REQUEST_URI'], PHP_URL_PATH) ?? '/';
        $uri = trim($uri, '/');
        $uri = str_replace('dsw/public', '', $uri);
        $uri = trim($uri, '/');
        
        if (strpos($uri, 'assets/') === 0) {
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
        foreach (self::$routes[$metodo] ?? [] as $route => $callback) {
            if (strpos($route, ':') !== false) {
                $route = preg_replace('#:[a-zA-Z]+#', '([a-zA-Z0-9]+)', $route);
            }
            $route = trim($route, '/');
            if (preg_match("#^$route$#", $uri, $matches)) {
                $params = array_slice($matches, 1);
                if (is_array($callback)){
                    $controller = new $callback[0];
                    $response = $controller->{$callback[1]}(...$params);
                } elseif (is_callable($callback)){
                    $response = $callback(...$params);
                }
                if (is_array($response) || is_object($response)) {
                    header('Content-Type: application/json');
                    echo json_encode($response);
                } else {
                    echo $response;
                }
                return;
            }
        }

        http_response_code(404);
        echo 'Ruta no encontrada';
    }
}

?>
