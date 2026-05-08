# UNUM — Guía de instalación y ejecución

Aplicación de escritorio WPF (.NET 8) para gestión de finanzas personales.

---

## Requisitos previos

- **Windows 10/11**
- **.NET 8 Desktop Runtime** — si no está instalado, descargarlo desde:
  `https://dotnet.microsoft.com/download/dotnet/8.0` (sección "Run desktop apps")
- **MySQL Server 8.x** corriendo en local en el puerto **3306**

---

## 1. Preparar la base de datos

Abrir MySQL Workbench (o cualquier cliente MySQL) y ejecutar el siguiente script para crear la base de datos y las tablas necesarias:

```sql
CREATE DATABASE IF NOT EXISTS UNUM CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE UNUM;

CREATE TABLE IF NOT EXISTS Usuarios (
    Id INT NOT NULL AUTO_INCREMENT,
    NombreUsuario VARCHAR(50) NOT NULL UNIQUE,
    Contrasena VARCHAR(255) NOT NULL,
    PRIMARY KEY (Id)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS Transacciones (
    Id INT NOT NULL AUTO_INCREMENT,
    UsuarioId INT NOT NULL,
    Tipo VARCHAR(20) NOT NULL,
    Categoria VARCHAR(50) NOT NULL,
    Importe DECIMAL(18,2) NOT NULL,
    FechaTransaccion DATETIME NOT NULL,
    Descripcion VARCHAR(255),
    PRIMARY KEY (Id),
    FOREIGN KEY (UsuarioId) REFERENCES Usuarios(Id) ON DELETE CASCADE
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS Objetivos (
    Id INT NOT NULL AUTO_INCREMENT,
    UsuarioId INT NOT NULL,
    Nombre VARCHAR(100) NOT NULL,
    CosteTotal DECIMAL(18,2) NOT NULL,
    AhorroActual DECIMAL(18,2) NOT NULL DEFAULT 0,
    Prioridad INT NOT NULL DEFAULT 2,
    PRIMARY KEY (Id),
    FOREIGN KEY (UsuarioId) REFERENCES Usuarios(Id) ON DELETE CASCADE
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS Presupuestos (
    Id INT NOT NULL AUTO_INCREMENT,
    UsuarioId INT NOT NULL,
    Categoria VARCHAR(128) NOT NULL,
    LimiteMensual DECIMAL(18,2) NOT NULL,
    CreadoEn DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (Id),
    UNIQUE KEY UK_Presupuestos_Usuario_Categoria (UsuarioId, Categoria),
    FOREIGN KEY (UsuarioId) REFERENCES Usuarios(Id) ON DELETE CASCADE
) ENGINE=InnoDB;
```

---

## 2. Configurar la conexión

Abrir el archivo **`UNUM.dll.config`** (está junto al ejecutable) con el Bloc de notas y cambiar la contraseña por la de su instalación de MySQL:

```xml
<connectionStrings>
  <add name="UNUM_DB"
       connectionString="Server=127.0.0.1;Port=3306;Database=UNUM;Uid=root;Pwd=SU_PASSWORD_AQUI;"
       providerName="MySql.Data.MySqlClient" />
</connectionStrings>
```

Guardar el archivo.

---

## 3. Ejecutar la aplicación

Hacer doble clic en **`UNUM.exe`**.

La primera vez que se ejecuta la aplicación se puede crear una cuenta desde la pantalla de inicio pulsando **Registrarse**.

---

## Notas

- Los datos de sesión se guardan localmente, por lo que al cerrar y volver a abrir la app el usuario permanece conectado.
- Si se desea probar con una cuenta nueva, usar el botón **Cerrar sesión** desde el panel principal.
- El tutorial interactivo se abre automáticamente la primera vez que se accede al panel principal y puede relanzarse en cualquier momento desde el botón **Tutorial** de la barra lateral.
