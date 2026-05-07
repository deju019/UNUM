-- Presupuestos mensuales por categoría de gasto (por usuario).
-- Ejecutar contra la base de datos UNUM (o la que uses en la cadena de conexión).

CREATE TABLE IF NOT EXISTS Presupuestos (
    Id INT NOT NULL AUTO_INCREMENT,
    UsuarioId INT NOT NULL,
    Categoria VARCHAR(128) NOT NULL,
    LimiteMensual DECIMAL(18, 2) NOT NULL,
    CreadoEn DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (Id),
    UNIQUE KEY UK_Presupuestos_Usuario_Categoria (UsuarioId, Categoria),
    CONSTRAINT FK_Presupuestos_Usuario
        FOREIGN KEY (UsuarioId) REFERENCES Usuarios (Id)
        ON DELETE CASCADE
        ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
