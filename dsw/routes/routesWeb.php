<?php 

use Libreria\Route;
use app\controlador\userController;

use app\controlador\courseController;

use app\controlador\statementController;

use app\controlador\taskSubmissionController;


//vistas 
Route::get('/', [userController::class, 'showLogin']);
Route::get('/login', [userController::class, 'showLogin']);

Route::get('/register', [userController::class, 'showRegister']);

Route::get('/cursos', [courseController::class, 'showCourses']);
Route::get('/crearCurso', [courseController::class, 'showCreateCourse']);
Route::get('/api/listarCursos', [courseController::class, 'listarCursos']);
Route::get('/curso', [courseController::class, 'showCourse']);
Route::get('/api/generarCodigoCurso', [courseController::class, 'generarCodigoCurso']);
Route::get('/api/verCurso', [courseController::class, 'verCurso']);


Route::get('/tareas', [statementController::class, 'showTareas']);
Route::get('/crearTarea', [statementController::class, 'showCrearTarea']);
Route::get('/tarea', [statementController::class, 'showTarea']);
Route::get('/api/listarTareas', [statementController::class, 'listarTareas']);
Route::get('/api/verTarea', [statementController::class, 'verTarea']);
Route::get('/api/listarCursosEstudiante', [courseController::class, 'listarCursosEstudiante']);
Route::get('/api/listarTareasEstudiante', [statementController::class, 'listarTareasEstudiante']);
Route::get('/tareaEstudiante', [statementController::class, 'showTareaEstudiante']);
Route::get('/api/downloadSubmission', [taskSubmissionController::class, 'downloadSubmission']);



//post
Route::post('/api/login', [userController::class, 'loginUser']);
Route::post('/api/register', [userController::class, 'registerUser']);
Route::post('/api/crearCurso', [courseController::class, 'crearCurso']);
Route::post('/api/crearTarea', [statementController::class, 'crearTarea']);
Route::post('/api/unirseACurso', [courseController::class, 'unirseACurso']);
Route::post('/api/editarTarea', [statementController::class, 'editarTarea']);
Route::post('/api/registrarEstudianteEntrega', [taskSubmissionController::class, 'registerSubmission']);

Route::post('/api/createSubmission', [taskSubmissionController::class, 'createSubmission']);


?>

