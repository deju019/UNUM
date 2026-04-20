# UNUM - Contexto Vivo del Proyecto

## 1) Objetivo General

Desarrollar una aplicacion de escritorio WPF (.NET 8, C#) para gestion financiera personal con:

- autenticacion de usuarios
- registro y gestion de transacciones (ingresos/gastos)
- objetivos de ahorro
- evolucion hacia producto completo con buena arquitectura, seguridad y UX

## 2) Estado Actual del Workspace (2026-04-20)

Proyecto existente en C# WPF:

- InicioWindow: seleccion entre login y registro
- LoginWindow: valida usuario/password contra MySQL y abre MainWindow
- RegisterWindow: crea usuario
- MainWindow: alta, listado, refresco y borrado de transacciones del usuario autenticado

Tecnologias observadas:

- .NET 8 WPF
- MySql.Data (acceso directo desde code-behind)
- SQL con tablas: Usuarios, Transacciones, Objetivos (definidas externamente)

## 3) Observaciones Tecnicas Relevantes

- La cadena de conexion esta hardcodeada en varias ventanas.
- La contrasena se inserta/consulta en texto plano actualmente.
- Hay inconsistencias de nombre entre columna de password:
    - script inicial menciona "Contraseña"
    - codigo usa "Contrasena"
- En el script SQL enviado hay 2 errores de sintaxis (texto extra dentro de CREATE TABLE).

## 4) Script SQL Corregido Base (propuesto)

```sql
CREATE DATABASE IF NOT EXISTS UNUM;
USE UNUM;

CREATE TABLE IF NOT EXISTS Usuarios (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    NombreUsuario VARCHAR(50) NOT NULL UNIQUE,
    Contrasena VARCHAR(255) NOT NULL,
    FechaRegistro DATETIME DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS Transacciones (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    UsuarioId INT NOT NULL,
    Tipo VARCHAR(10) NOT NULL,
    Categoria VARCHAR(50) NOT NULL,
    Importe DECIMAL(10,2) NOT NULL,
    FechaTransaccion DATE NOT NULL,
    Descripcion VARCHAR(255),
    FOREIGN KEY (UsuarioId) REFERENCES Usuarios(Id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS Objetivos (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    UsuarioId INT NOT NULL,
    Nombre VARCHAR(100) NOT NULL,
    CosteTotal DECIMAL(10,2) NOT NULL,
    AhorroActual DECIMAL(10,2) NOT NULL DEFAULT 0,
    Prioridad INT NOT NULL,
    FOREIGN KEY (UsuarioId) REFERENCES Usuarios(Id) ON DELETE CASCADE
);
```

## 5) Backlog de Desarrollo (v1)

1. Estabilizar base tecnica:
    - centralizar cadena de conexion en configuracion
    - crear capa de acceso a datos (repositorios/servicios)
    - normalizar nombres de columnas y tipos
2. Seguridad de autenticacion:
    - guardar contrasenas con hash+salt (BCrypt)
    - ajustar login/registro
3. Funcionalidad de objetivos:
    - CRUD completo de Objetivos
    - vinculacion con ahorro acumulado
4. Dashboard y analitica:
    - resumen mensual, balance, categorias top
    - filtros por fecha/categoria/tipo
5. Calidad:
    - validaciones robustas UI/negocio
    - manejo de errores consistente
    - pruebas manuales guiadas y, cuando proceda, unit tests

## 6) Metodo de Trabajo Acordado

- Este archivo se considera la fuente de verdad de contexto.
- En cada nuevo prompt, se debe:
    1. leer este archivo
    2. ejecutar cambios solicitados
    3. actualizar este archivo con avances, decisiones y pendientes

## 7) Pendientes de Contexto

- Falta el PDF funcional en el workspace/adjuntos de esta sesion para extraer requisitos detallados.
- Cuando se reciba, incorporar seccion de requisitos funcionales/no funcionales con trazabilidad a backlog.

## 8) Revision Tecnica Detallada (codigo ya implementado)

### 8.1 Navegacion y flujo de ventanas

- Arranque de app en InicioWindow via StartupUri.
- Desde InicioWindow:
    - boton "Iniciar Sesion" abre LoginWindow y cierra InicioWindow
    - boton "Crear una Cuenta" abre RegisterWindow y cierra InicioWindow
- Desde LoginWindow:
    - valida no vacios
    - autentica contra MySQL
    - al autenticar, abre MainWindow(usuarioId) y cierra LoginWindow
- Desde RegisterWindow:
    - inserta usuario
    - vuelve a InicioWindow tras registro correcto
- Desde MainWindow:
    - permite cerrar sesion y volver a InicioWindow

### 8.2 Funcionalidad de datos disponible hoy

- Usuarios:
    - alta de usuario basica en RegisterWindow
    - login por usuario+password en LoginWindow
- Transacciones:
    - alta de transaccion (tipo, categoria, importe, descripcion, fecha actual)
    - listado filtrado por UsuarioId en DataGrid
    - borrado de transaccion seleccionada con confirmacion
    - refresco manual del historial
- Objetivos:
    - no implementado en UI ni en backend de aplicacion

### 8.3 Riesgos y deuda tecnica detectada

- Seguridad:
    - password en texto plano (riesgo alto)
    - credenciales de BD hardcodeadas en codigo (riesgo alto)
- Mantenibilidad:
    - logica de acceso a datos en code-behind de ventanas
    - repeticion de cadena de conexion y sentencias SQL
- Integridad funcional:
    - posible inconsistencia de nombres Contrasena/Contraseña segun script externo
- UX y robustez:
    - falta validacion avanzada (longitudes, caracteres, rangos por categoria)
    - falta feedback de carga/estado en operaciones de BD
- Calidad de codigo:
    - aparecen textos con encoding corrupto en algunos MessageBox (tildes)

### 8.4 Prioridad de trabajo recomendada (orden de ejecucion)

1. Seguridad minima viable: hash de password y conexion en configuracion.
2. Refactor base: capa de servicios/repositorios para desacoplar UI de BD.
3. Completar modulo Objetivos con CRUD y pantalla dedicada.
4. Mejorar dashboard (metricas, balance mensual, filtros).
5. Endurecer validaciones, errores y pruebas.

### 8.5 Estado del PDF

- A fecha de esta revision no se localiza PDF en:
    - workspace UNUM
    - busqueda recursiva de PDFs en perfil de usuario
- Accion pendiente: recibir ruta exacta o volver a adjuntarlo para extraer requisitos completos.

## 9) Registro de Sesion (2026-04-20)

- El usuario indica que el PDF fue enviado "linkeado" en mensaje.
- Se realizaron nuevas busquedas por:
    - archivos .pdf en workspace
    - busqueda recursiva en perfil de usuario
    - inspeccion de recursos de chat en workspaceStorage (content.txt)
- Resultado: no se obtuvo URL ni archivo PDF utilizable para lectura automatica.
- Decision temporal: continuar desarrollo por roadmap tecnico ya definido hasta disponer del PDF.

## 10) Documento de Requisitos Localizado (fuente real)

- Archivo encontrado en proyecto: Grupo4C_Investigacion .docx
- No se encontro PDF, pero se extrajo texto del DOCX.
- El documento contiene 3 propuestas distintas:
    1. app/pagina de ahorro y control financiero
    2. app de cumplimiento de objetivos con calendario
    3. app de consentimiento y seguridad en encuentros casuales

### 10.1 Alcance asumido para UNUM

- Se asume que UNUM corresponde a la propuesta 1 (ahorro y control financiero), por coherencia con:
    - tablas actuales (Usuarios, Transacciones, Objetivos)
    - interfaz ya implementada de ingresos/gastos

### 10.2 Requisitos funcionales inferidos para UNUM

- Registrar gastos e ingresos de forma simple.
- Visualizar historial de movimientos.
- Ayudar a tomar decisiones con proyecciones basadas en ingresos y posibles gastos extraordinarios.
- Incluir modulo de ahorro/objetivos para planificacion a futuro.
- Mantener enfoque intuitivo y visual para usuarios no expertos.
- No requerir vinculacion de cuentas bancarias reales.

### 10.3 Requisitos no funcionales inferidos

- Usabilidad alta: interfaz clara y curva de aprendizaje baja.
- Seguridad y privacidad razonables de datos personales.
- Rendimiento fluido en operaciones habituales.
- Viabilidad de desarrollo por fases (MVP + mejoras).

### 10.4 Brecha entre vision del documento y estado actual

- Ya existe base operativa de autenticacion y transacciones.
- Faltan proyecciones economicas/simulaciones.
- Falta modulo completo de objetivos (solo existe tabla).
- Falta endurecimiento de seguridad (hash de password y configuracion segura).

## 11) Avance Implementado - Fase 1 (2026-04-20)

### 11.1 Arquitectura base aplicada

- Se creo App.config con cadena de conexion UNUM_DB.
- Se centralizo la apertura de conexion en Infrastructure/DbConnectionFactory.cs.
- Se extrajo logica de negocio a servicios:
    - Services/AuthService.cs
    - Services/TransactionService.cs
- Resultado: ventanas WPF ya no contienen SQL directo para login/registro/transacciones.

### 11.2 Seguridad aplicada

- Se incorporo BCrypt.Net-Next.
- Registro ahora guarda password hasheada (BCrypt) en Usuarios.Contrasena.
- Login ahora valida contra hash.
- Compatibilidad temporal: si existe usuario legacy en texto plano, el login sigue funcionando.

### 11.3 Configuracion y dependencias

- UNUM.csproj actualizado con:
    - BCrypt.Net-Next
    - System.Configuration.ConfigurationManager
    - MySql.Data (ya existente)

### 11.4 Estado de compilacion

- dotnet build ejecutado con exito.
- Se corrigieron warnings de nullability en seleccion de ComboBoxItem de MainWindow.

### 11.5 Proximo paso sugerido

- Fase 2: modulo Objetivos (CRUD + vista en MainWindow o ventana dedicada) y preparacion de base para proyecciones.

## 12) Incidencia Resuelta - Columna de contrasena (2026-04-20)

- Error reportado en ejecucion: Unknown column 'Contrasena' in field list.
- Causa: en algunas BDs la columna existe como Contraseña (con enye), mientras el codigo consultaba Contrasena.
- Solucion aplicada en Services/AuthService.cs:
    - deteccion dinamica del nombre real de columna desde INFORMATION_SCHEMA
    - consultas de login/registro construidas con el identificador detectado
    - compatibilidad mantenida para ambos esquemas sin requerir migracion inmediata

## 13) Avance Implementado - Fase 2 parcial (2026-04-20)

### 13.1 Modulo Objetivos integrado

- Nuevo servicio: Services/ObjectiveService.cs
    - alta de objetivo
    - listado de objetivos del usuario
    - actualizacion de ahorro actual
    - borrado de objetivo
    - calculo de progreso porcentual en consulta SQL
- Nueva ventana: Components/Frontend/ObjetivosWindow.xaml
- Code-behind de objetivos: Components/Backend/ObjetivosWindow.xaml.cs

### 13.2 Integracion con MainWindow

- Se agrego boton Objetivos en MainWindow.
- Se agrego apertura modal de ObjetivosWindow con el usuario autenticado.

### 13.3 Proximo paso sugerido

- Fase 2 siguiente: mejorar dashboard con resumen (saldo total, ingresos, gastos, ahorro acumulado y avance por prioridad).
