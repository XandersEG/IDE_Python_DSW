CREATE DATABASE IF NOT EXISTS DB_IDE_Python_DSW;
USE DB_IDE_Python_DSW;

-- TABLAS

CREATE TABLE IF NOT EXISTS Usuario (
    Correo VARCHAR(100) NOT NULL,
    PrimerNombre VARCHAR(50) NOT NULL,
    SegundoNombre VARCHAR(50) NULL,
    PrimerApellido VARCHAR(50) NOT NULL,
    SegundoApellido VARCHAR(50) NOT NULL,
    Contrasena VARCHAR(255) NOT NULL,
    PRIMARY KEY (Correo)
);



CREATE TABLE IF NOT EXISTS Curso (
    CorreoUsuarioProfesor VARCHAR(100) NOT NULL,
    Nombre VARCHAR(100) NOT NULL,
    Codigo VARCHAR(20) NOT NULL,
    Contrasena VARCHAR(255) NOT NULL UNIQUE,
    FechaLimite DATE NOT NULL DEFAULT (CURRENT_DATE),
    PRIMARY KEY (CorreoUsuarioProfesor, Nombre),
    FOREIGN KEY (CorreoUsuarioProfesor) REFERENCES Usuario(Correo)
);


CREATE TABLE IF NOT EXISTS EstudianteXCurso (
    idEstudianteCurso INT NOT NULL AUTO_INCREMENT,
    CorreoUsuarioEstudiante VARCHAR(100) NOT NULL,
    NombreCurso VARCHAR(100) NOT NULL,
    CorreoUsuarioProfesor VARCHAR(100) NOT NULL,
    PRIMARY KEY (idEstudianteCurso),
    FOREIGN KEY (CorreoUsuarioEstudiante) REFERENCES Usuario(Correo),
    FOREIGN KEY (CorreoUsuarioProfesor, NombreCurso) REFERENCES Curso(CorreoUsuarioProfesor, Nombre)
);

CREATE TABLE IF NOT EXISTS Enunciado (
    idEnunciado INT NOT NULL AUTO_INCREMENT,
    Titulo VARCHAR(200) NOT NULL,
    Descripcion VARCHAR(1000) NOT NULL,
    nombreCurso VARCHAR(100) NOT NULL,
    correoUsuarioProfesor VARCHAR(100) NOT NULL,
    PRIMARY KEY (idEnunciado),
    FOREIGN KEY (correoUsuarioProfesor, nombreCurso) REFERENCES Curso(CorreoUsuarioProfesor, Nombre)
);

CREATE TABLE IF NOT EXISTS Entrega (
    idEntrega INT NOT NULL AUTO_INCREMENT,
    timeStamp DATETIME NOT NULL,
    nombreArchivo VARCHAR(255) NOT NULL,
    Contenido LONGBLOB NOT NULL,
    idEnunciado INT NOT NULL,
    PRIMARY KEY (idEntrega),
    FOREIGN KEY (idEnunciado) REFERENCES Enunciado(idEnunciado)
);

CREATE TABLE IF NOT EXISTS EstudianteXEntrega (
    CorreoUsuarioEstudiante VARCHAR(100) NOT NULL,
    idEntrega INT NOT NULL,
    PRIMARY KEY (CorreoUsuarioEstudiante, idEntrega),
    FOREIGN KEY (CorreoUsuarioEstudiante) REFERENCES Usuario(Correo),
    FOREIGN KEY (idEntrega) REFERENCES Entrega(idEntrega)
);

CREATE TABLE IF NOT EXISTS Proyecto (
    Nombre VARCHAR(100) NOT NULL,
    PRIMARY KEY (Nombre)
);

CREATE TABLE IF NOT EXISTS EstudianteXProyecto (
    idEstudianteXCurso INT NOT NULL AUTO_INCREMENT,
    correoUsuarioEstudiante VARCHAR(100) NOT NULL,
    nombreProyecto VARCHAR(100) NOT NULL,
    PRIMARY KEY (idEstudianteXCurso),
    FOREIGN KEY (correoUsuarioEstudiante) REFERENCES Usuario(Correo),
    FOREIGN KEY (nombreProyecto) REFERENCES Proyecto(Nombre)
);


-- PROCEDURES


DELIMITER //

CREATE PROCEDURE IF NOT EXISTS IniciarSesion(
    IN p_correo VARCHAR(100),
    IN p_contrasena VARCHAR(255)
)
BEGIN
    SELECT Correo, PrimerNombre, PrimerApellido
    FROM Usuario
    WHERE Correo = p_correo AND Contrasena = p_contrasena;
END //

CREATE PROCEDURE IF NOT EXISTS CrearUsuario(
    IN p_correo VARCHAR(100),
    IN p_primerNombre VARCHAR(50),
    IN p_segundoNombre VARCHAR(50),
    IN p_primerApellido VARCHAR(50),
    IN p_segundoApellido VARCHAR(50),
    IN p_contrasena VARCHAR(255)
)
BEGIN
    IF EXISTS (SELECT 1 FROM Usuario WHERE Correo = p_correo) THEN
        SELECT FALSE AS exito, 'El correo ya está registrado' AS mensaje;
    ELSE
        INSERT INTO Usuario (Correo, PrimerNombre, SegundoNombre, PrimerApellido, SegundoApellido, Contrasena)
        VALUES (p_correo, p_primerNombre, p_segundoNombre, p_primerApellido, p_segundoApellido, p_contrasena);
        SELECT TRUE AS exito, 'Usuario creado correctamente' AS mensaje;
    END IF;
END //

CREATE PROCEDURE IF NOT EXISTS CrearCurso(
    IN p_nombre VARCHAR(100),
    IN p_codigo VARCHAR(20),
    IN p_contrasena VARCHAR(255),
    IN p_correoProfesor VARCHAR(100)
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM Usuario WHERE Correo = p_correoProfesor) THEN
        SELECT FALSE AS exito, 'El usuario no existe' AS mensaje, 2 AS datos;
    ELSEIF EXISTS (SELECT 1 FROM Curso WHERE Nombre = p_nombre AND CorreoUsuarioProfesor = p_correoProfesor) THEN
        SELECT FALSE AS exito, 'Ya usted tiene un curso con ese nombre' AS mensaje, 3 AS datos;
    ELSEIF EXISTS (SELECT 1 FROM Curso WHERE Contrasena = p_contrasena) THEN
        SELECT FALSE AS exito, 'Contraseña duplicada, intente de nuevo' AS mensaje, 4 AS datos;
    ELSE
        INSERT INTO Curso (CorreoUsuarioProfesor, Nombre, Codigo, Contrasena)
        VALUES (p_correoProfesor, p_nombre, p_codigo, p_contrasena);
        SELECT TRUE AS exito, 'Curso creado correctamente' AS mensaje, 1 AS datos;
    END IF;
END //

CREATE PROCEDURE IF NOT EXISTS ObtenerCursosPorEstudiante(
    IN p_correoEstudiante VARCHAR(100)
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM Usuario WHERE Correo = p_correoEstudiante) THEN
        SELECT FALSE AS exito, 'El usuario no existe' AS mensaje;
    ELSE
        SELECT c.Nombre, c.Codigo, c.CorreoUsuarioProfesor
        FROM Curso c
        JOIN EstudianteXCurso exc ON c.Nombre = exc.NombreCurso
        AND c.CorreoUsuarioProfesor = exc.CorreoUsuarioProfesor
        WHERE exc.CorreoUsuarioEstudiante = p_correoEstudiante;
    END IF;
END //

CREATE PROCEDURE IF NOT EXISTS ObtenerCursosPorProfesor(
    IN p_correoProfesor VARCHAR(100)
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM Usuario WHERE Correo = p_correoProfesor) THEN
        SELECT FALSE AS exito, 'El usuario no existe' AS mensaje;
    ELSE
        SELECT Nombre, Codigo, Contrasena
        FROM Curso
        WHERE CorreoUsuarioProfesor = p_correoProfesor;
    END IF;
END //

CREATE PROCEDURE IF NOT EXISTS CrearEnunciado(
    IN p_titulo VARCHAR(200),
    IN p_descripcion VARCHAR(1000),
    IN p_nombreCurso VARCHAR(100),
    IN p_correoProfesor VARCHAR(100),
    IN p_fechaLimite DATE
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM Usuario WHERE Correo = p_correoProfesor) THEN
        SELECT FALSE AS exito, 'El usuario no existe' AS mensaje, 1 AS datos;
    ELSEIF NOT EXISTS (SELECT 1 FROM Curso WHERE Nombre = p_nombreCurso AND CorreoUsuarioProfesor = p_correoProfesor) THEN
        SELECT FALSE AS exito, 'El curso no existe' AS mensaje, 2 AS datos;
    ELSE
        INSERT INTO Enunciado (Titulo, Descripcion, nombreCurso, correoUsuarioProfesor, FechaLimite)
        VALUES (p_titulo, p_descripcion, p_nombreCurso, p_correoProfesor, p_fechaLimite);
        SELECT TRUE AS exito, 'Enunciado creado correctamente' AS mensaje, LAST_INSERT_ID() AS datos;
    END IF;
END //

CREATE PROCEDURE IF NOT EXISTS ObtenerEnunciadosPorCurso(
    IN p_nombreCurso VARCHAR(100),
    IN p_correoProfesor VARCHAR(100)
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM Usuario WHERE Correo = p_correoProfesor) THEN
        SELECT FALSE AS exito, 'El usuario no existe' AS mensaje;
    ELSEIF NOT EXISTS (SELECT 1 FROM Curso WHERE Nombre = p_nombreCurso AND CorreoUsuarioProfesor = p_correoProfesor) THEN
        SELECT FALSE AS exito, 'El curso no existe' AS mensaje;
    ELSE
        SELECT idEnunciado, Titulo, Descripcion, FechaLimite
        FROM Enunciado
        WHERE nombreCurso = p_nombreCurso AND correoUsuarioProfesor = p_correoProfesor;
    END IF;
END //

CREATE PROCEDURE IF NOT EXISTS ObtenerEnunciadoPorId(
    IN p_idEnunciado INT
)
BEGIN
    SELECT idEnunciado, Titulo, Descripcion, FechaLimite
    FROM Enunciado
    WHERE idEnunciado = p_idEnunciado;
END //

sqlDELIMITER $$



CREATE PROCEDURE ObtenerEstudiantesPorEnunciado(
    IN p_idEnunciado INT
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM Enunciado WHERE idEnunciado = p_idEnunciado) THEN
        SELECT FALSE AS exito, 'El enunciado no existe' AS mensaje;
    ELSE
        SELECT u.Correo AS CorreoUsuarioEstudiante, u.PrimerNombre, u.PrimerApellido, 
               e.idEntrega, e.`timeStamp`
        FROM EstudianteXEntrega exe
        JOIN Usuario u ON exe.CorreoUsuarioEstudiante = u.Correo
        JOIN Entrega e ON exe.idEntrega = e.idEntrega
        WHERE e.idEnunciado = p_idEnunciado
        ORDER BY u.Correo, e.`timeStamp` ASC;
    END IF;
END$$

DELIMITER //


CREATE PROCEDURE IF NOT EXISTS DescargarEntrega(IN p_idEntrega INT)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM Entrega WHERE idEntrega = p_idEntrega) THEN
        SELECT FALSE AS exito, 'La entrega no existe' AS mensaje, NULL AS datos;
    ELSE
        SELECT idEntrega, timeStamp, Contenido, nombreArchivo
        FROM Entrega
        WHERE idEntrega = p_idEntrega;
    END IF;
END //


CREATE PROCEDURE IF NOT EXISTS UnirEstudianteACurso(
    IN p_correoEstudiante VARCHAR(100),
    IN p_contrasena VARCHAR(255)
)
BEGIN
    DECLARE v_nombreCurso VARCHAR(100);
    DECLARE v_correoProfesor VARCHAR(100);

    SELECT Nombre, CorreoUsuarioProfesor INTO v_nombreCurso, v_correoProfesor
    FROM Curso WHERE Contrasena = p_contrasena LIMIT 1;

    IF v_nombreCurso IS NULL THEN
        SELECT FALSE AS exito, 'Contraseña incorrecta' AS mensaje;
    ELSEIF NOT EXISTS (SELECT 1 FROM Usuario WHERE Correo = p_correoEstudiante) THEN
        SELECT FALSE AS exito, 'El usuario no existe' AS mensaje;
    ELSEIF EXISTS (
        SELECT 1 FROM EstudianteXCurso
        WHERE CorreoUsuarioEstudiante = p_correoEstudiante
        AND NombreCurso = v_nombreCurso
    ) THEN
        SELECT FALSE AS exito, 'Ya estás inscrito en este curso' AS mensaje;
    ELSE
        INSERT INTO EstudianteXCurso (CorreoUsuarioEstudiante, NombreCurso, CorreoUsuarioProfesor)
        VALUES (p_correoEstudiante, v_nombreCurso, v_correoProfesor);
        SELECT TRUE AS exito, 'Inscripción exitosa' AS mensaje;
    END IF;
END //

CREATE PROCEDURE IF NOT EXISTS ObtenerEnunciadosPorCursoEstudiante(
    IN p_nombreCurso VARCHAR(100),
    IN p_correoEstudiante VARCHAR(100),
    IN p_correoProfesor VARCHAR(100)
)
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM EstudianteXCurso
        WHERE NombreCurso = p_nombreCurso
        AND CorreoUsuarioEstudiante = p_correoEstudiante
        AND CorreoUsuarioProfesor = p_correoProfesor
    ) THEN
        SELECT FALSE AS exito, 'No estás inscrito en este curso' AS mensaje;
    ELSE
        SELECT idEnunciado, Titulo, Descripcion, FechaLimite
        FROM Enunciado
        WHERE nombreCurso = p_nombreCurso AND correoUsuarioProfesor = p_correoProfesor;
    END IF;
END //


CREATE PROCEDURE IF NOT EXISTS EditarEnunciado(
    IN p_idEnunciado INT,
    IN p_titulo VARCHAR(200),
    IN p_descripcion VARCHAR(1000),
    IN p_fechaLimite DATE
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM Enunciado WHERE idEnunciado = p_idEnunciado) THEN
        SELECT FALSE AS exito, 'La tarea no existe' AS mensaje;
    ELSE
        UPDATE Enunciado SET Titulo = p_titulo, Descripcion = p_descripcion, FechaLimite = p_fechaLimite
        WHERE idEnunciado = p_idEnunciado;
        SELECT TRUE AS exito, 'Tarea actualizada correctamente' AS mensaje;
    END IF;
END //

DELIMITER ;