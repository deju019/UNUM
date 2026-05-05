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

## 7) Refactor MVVM - Progreso (2026-05-05)

### ✅ Completado:

**Arquitectura**

- `ViewModels/ViewModelBase.cs`: Clase base con INotifyPropertyChanged
- `ViewModels/RelayCommand.cs`: Implementación de comandos (sync/async, genéricos)
- `Models/TransactionModel.cs`: Modelos de datos (Transaction, Budget, FilterState, etc.)
- `ViewModels/MainWindowViewModel.cs`: ViewModel con toda la lógica de negocio refactorizada

**Características del ViewModel**:

- ✅ Propiedades bindables (DataView, Summary, Budgets, Filters)
- ✅ Commands: GuardarTransacción, BorrarTransacción, AplicarFiltros, ExportarCSV, etc.
- ✅ Lógica de filtros persistente (GuardarEstadoFiltros/RestaurarEstadoFiltros)
- ✅ Lógica de presupuestos (GuardarPresupuesto, ActualizarPanelPresupuestos)
- ✅ Validaciones integradas en métodos del ViewModel
- ✅ Cálculos centralizados (ActualizarResumenFinanciero)

### ⏳ Pendiente:

1. **Simplificar MainWindow.xaml.cs** ✅ COMPLETADO
    - Mantuvo: DataContext setup, navegación modal, animaciones
    - Eliminó: toda lógica de filtros, transacciones, presupuestos (→ ViewModel)
    - Resultado: 1500 líneas → 105 líneas

2. **Actualizar MainWindow.xaml - PRÓXIMO PASO**
    - Reemplazar Text=" value" con bindings `Text="{Binding Propiedad}"`
    - Reemplazar ComboBox items con ItemsSource bindings
    - Reemplazar DataGrid con ItemsSource binding
    - Reemplazar botones Click handlers con Command bindings
    - Añadir Converters si es necesario

3. **Compilación**: ✅ Sin errores actualmente

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

## 14) Estado Consolidado Actual (2026-04-20 - tarde)

### 14.1 Rama y estado de trabajo

- Rama activa: integration/ur02-ur03
- El trabajo se mantiene en esta rama por decision del usuario (sin merge a main por ahora).
- Hay cambios locales pendientes de commit en:
    - App.xaml
    - Components/Frontend/MainWindow.xaml
    - Components/Backend/MainWindow.xaml.cs
    - Components/Backend/LoginWindow.xaml.cs
    - Components/Backend/ObjetivoModalWindow.xaml.cs
    - Components/Backend/TimerWindow.xaml.cs

### 14.2 Integracion y estabilizacion aplicadas

- Se resolvieron conflictos de merge que quedaban incrustados en:
    - MainWindow.xaml
    - MainWindow.xaml.cs
    - LoginWindow.xaml.cs
- Se restauro el flujo de arranque funcional de la app:
    - StartupUri apunta a Components/Frontend/InicioWindow.xaml
- Se unifico MainWindow con enfoque por servicios:
    - TransactionService para transacciones
    - ObjectiveService para objetivos
    - eliminada mezcla de SQL directo residual en MainWindow
- Se elimino cadena de conexion hardcodeada en ObjetivoModalWindow y se usa DbConnectionFactory.

### 14.3 Validacion tecnica actual

- Compilacion de solucion:
    - dotnet build UNUM.sln -> correcta
    - 0 errores
    - 0 advertencias
- Warning CS8622 en TimerWindow resuelto ajustando firma de evento:
    - Timer_Tick(object? sender, EventArgs e)

### 14.4 Estado funcional esperado

- Inicio -> Login/Registro -> MainWindow operativo.
- Modulo de transacciones operativo (alta/listado/borrado/refresco y calculo de saldo en UI).
- Modulo de objetivos operativo desde panel simulador y modal de alta.

## 15) Actualizacion Reciente (2026-04-21 / 2026-04-22)

### 15.1 UX y guia de uso

- Se anadio onboarding inicial y guia interactiva por pasos en dashboard:
    - bienvenida de primer uso
    - guia paso a paso para registrar, visualizar y borrar transacciones
    - foco visual por seccion (registro/historial/acciones)
- El estado del onboarding ya es por usuario (no global):
    - persistencia en `onboarding-mainwindow.json` bajo LocalApplicationData
    - cada usuario nuevo ve su tutorial la primera vez
- Se corrigio el cierre del tutorial para restaurar siempre opacidades/estilos (sin dejar pantalla atenuada).

### 15.2 Autenticacion y flujo de registro

- Registro mejorado:
    - al crear cuenta se inicia sesion automaticamente
    - se abre MainWindow directo sin pasar por login manual
- `AuthService.RegisterUser` ahora devuelve el `Id` insertado para habilitar auto-login.

### 15.3 Dashboard y animaciones

- MainWindow incorpora animaciones suaves:
    - entrada escalonada de bloques principales (fade + slide)
    - transicion al cambiar entre panel Transacciones y panel Objetivos
- Se mantuvo la logica existente de negocio, aplicando solo mejoras de percepcion visual.

### 15.4 Filtros y coherencia de datos

- Rediseño del bloque de filtros:
    - layout mas compacto e intuitivo
    - acciones mas claras (`Aplicar`, `Limpiar`, `Actualizar`)
    - mensaje explicito de que filtros afectan a busqueda
- Correccion funcional importante:
    - el resumen financiero y saldo ya no cambian al filtrar
    - filtros afectan solo a la tabla de resultados
    - calculo de resumen usa siempre el conjunto completo de transacciones del usuario

### 15.5 Presupuestos por categoria (nuevo modulo)

- Implementado modulo de presupuestos mensuales por categoria de gasto:
    - selector de categoria
    - limite mensual
    - guardar y reset de presupuestos
    - lista de progreso por categoria con barra y semaforo (verde/amarillo/rojo)

### 15.6 Cambios MVVM recientes (2026-05-05)

- Se movió la responsabilidad de visibilidad de paneles (`Transacciones` / `Simulador`) al `ViewModel`:
    - nuevas propiedades bindables en `ViewModels/MainWindowViewModel.cs`: `IsTransaccionesVisible`, `IsSimuladorVisible`.
    - `MostrarPanel` ahora actualiza estas propiedades además de las banderas de menú.
- Se actualizaron bindings en `Components/Frontend/MainWindow.xaml` para usar `Visibility` enlazada a las nuevas propiedades (con `BooleanToVisibilityConverter`).
- `Components/Backend/MainWindow.xaml.cs` ahora se suscribe a `INotifyPropertyChanged` del ViewModel y lanza las animaciones de entrada/transición cuando las propiedades de visibilidad cambian. La animación y lógica UI permanece en la View.
- Se compiló la solución tras cambios y el build resultó exitoso (0 errores, 0 advertencias).

Estado: `Actualizar context.md con los cambios realizados` — en progreso (esta entrada la completa).
- Persistencia por usuario en LocalApplicationData:
    - archivo `presupuestos-mainwindow.json`

### 15.6 Build y tooling

- Se creo tarea de VS Code para build estable sobre `UNUM.sln` (evitando confusion con `.slnx`).
- Incidencia conocida:
    - si `UNUM.exe` esta abierto, `dotnet build` puede fallar por archivo bloqueado (`MSB3026`).

### 15.7 Estado actual validado

- Compilaciones recientes correctas en `UNUM.sln` tras los cambios UX/funcionales.
- Proyecto en estado operativo con foco en siguiente etapa por bloques:
    1. Alertas de presupuestos (80% / 100%)
    2. Resumen de exceso por categorias
    3. Tendencias y recordatorios inteligentes
- Autenticacion y registro por AuthService con hash BCrypt y compatibilidad de columna Contrasena/Contraseña.

### 14.5 Pendiente inmediato recomendado

- Commit de estabilizacion completado.

## 15) Avance Implementado - Dashboard Resumen (2026-04-20)

### 15.1 Commit de estabilizacion registrado

- Commit realizado en integration/ur02-ur03:
    - 45c310c
    - mensaje: fix: estabilizar integracion y limpiar conflictos

### 15.2 Mejora funcional aplicada

- Se anadio resumen financiero visual en el panel de transacciones de MainWindow:
    - total ingresos
    - total gastos
    - balance neto
- Cambios de UI en Components/Frontend/MainWindow.xaml:
    - nueva fila de tarjetas resumen entre formulario y tabla
    - nuevos TextBlock: txtTotalIngresos, txtTotalGastos, txtBalanceNeto

### 15.3 Cambios de logica aplicados

- Se reemplazo el calculo de saldo por un calculo de resumen completo en Components/Backend/MainWindow.xaml.cs:
    - nuevo metodo: CalcularResumenFinanciero(DataTable)
    - calcula ingresos acumulados, gastos acumulados y balance
    - mantiene actualizacion de txtSaldoTotal y color de borderSaldo segun signo

### 15.4 Estado actual para continuar

- Quedan pendientes de commit los cambios del nuevo resumen en:
    - Components/Frontend/MainWindow.xaml
    - Components/Backend/MainWindow.xaml.cs
- Siguiente iteracion sugerida:
    - filtros por rango de fechas y categoria
    - resumen mensual (mes actual) separado del total historico

## 16) Avance Implementado - Filtros de Transacciones (2026-04-20)

### 16.1 Mejora funcional aplicada

- Se implementaron filtros en el panel de transacciones por:
    - categoria
    - fecha desde
    - fecha hasta
- Se anadieron botones de accion:
    - Aplicar filtros
    - Limpiar filtros

### 16.2 Cambios de UI

- Components/Frontend/MainWindow.xaml:
    - nueva barra de filtros entre tarjetas resumen y DataGrid de transacciones
    - nuevos controles:
        - cmbFiltroCategoria
        - dpFechaDesde
        - dpFechaHasta
        - btnAplicarFiltros
        - btnLimpiarFiltros

### 16.3 Cambios de logica

- Components/Backend/MainWindow.xaml.cs:
    - nueva cache local de transacciones: \_transacciones (DataTable)
    - nuevo metodo AplicarFiltrosTransacciones()
    - filtro aplicado con DataView.RowFilter sobre categoria/fechas
    - validacion: fecha desde no puede ser mayor que fecha hasta
    - el resumen financiero se recalcula sobre el conjunto filtrado visible

### 16.4 Validacion tecnica

- Compilacion de solucion tras cambios:
    - dotnet build UNUM.sln correcta
    - 0 errores
    - 0 advertencias

## 17) Avance Implementado - Tipo y Periodo Rapido (2026-04-20)

### 17.1 Mejora funcional aplicada

- Se ampliaron los filtros de transacciones con:
    - tipo (Todos, Ingreso, Gasto)
    - periodo rapido (Historico, Mes actual)
- Los filtros de tipo/periodo se combinan con categoria y rango de fechas manual.

### 17.2 Cambios de UI

- Components/Frontend/MainWindow.xaml:
    - nuevos controles en barra de filtros:
        - cmbFiltroTipo
        - cmbFiltroPeriodo
    - reajuste del layout de columnas para soportar mas criterios.

### 17.3 Cambios de logica

- Components/Backend/MainWindow.xaml.cs:
    - AplicarFiltrosTransacciones() ahora incorpora filtro por Tipo.
    - AplicarFiltrosTransacciones() incorpora filtro por periodo Mes actual (primer y ultimo dia del mes).
    - btnLimpiarFiltros_Click ahora resetea tambien tipo y periodo.

### 17.4 Validacion tecnica

## 18) Continuacion MVVM MainWindow (2026-05-05)

### 18.1 Simulador de objetivos migrado a bindings

- En MainWindow (panel simulador), los botones de objetivos dejaron de depender de handlers en code-behind:
    - `Nuevo Objetivo` usa `NuevoObjetivoCommand`
    - `Borrar Seleccionado` usa `BorrarObjetivoCommand`
- El `DataGrid` de objetivos ahora se alimenta desde ViewModel:
    - `ItemsSource` -> `Objetivos`
    - `SelectedItem` -> `ObjetivoSeleccionado`
- El estado vacio del panel de objetivos se enlaza por converter (`EmptyCollectionToVisibilityConverter`).

### 18.2 ViewModel ampliado para objetivos

- `MainWindowViewModel` ahora incluye:
    - estado: `ObservableCollection<ObjectiveItemModel> Objetivos`
    - seleccion: `ObjectiveItemModel? ObjetivoSeleccionado`
    - carga: `CargarObjetivos()` desde `ObjectiveService`
    - acciones: crear y borrar objetivos via comandos
- Se anadio modelo dedicado `ObjectiveItemModel` para desacoplar la vista del DataRow/DataTable crudo.

### 18.3 Limpieza adicional de code-behind

- `MainWindow.xaml.cs` se simplifico retirando codigo muerto relacionado con objetivos:
    - eliminado el campo `_usuarioId` (ya no necesario en esta ventana)
    - eliminado `btnObjetivos_Click` (sin referencias activas)
- Se mantiene en code-behind solo lo estrictamente UI (navegacion global, animaciones, lifecycle inicial).

### 18.4 Validacion tecnica

- `dotnet build UNUM.sln` ejecutado tras los cambios: compilacion correcta.
- Resultado: 0 errores, 0 advertencias.

- Validacion de XAML y C# sin errores de editor.
- Build lanzado durante ejecucion de la app mostro bloqueos de archivo UNUM.exe (MSB3026) por proceso en uso.
- No se detectaron errores de compilacion de codigo en los archivos modificados.

## 18) Avance Implementado - Coherencia Tipo/Categoria (2026-04-20)

### 18.1 Regla de negocio aplicada

- En alta de transacciones, las categorias disponibles ahora dependen del tipo:
    - Ingreso: Nomina, Otros
    - Gasto: Ocio, Supermercado, Facturas, Otros
- Se evita explicitamente combinaciones sin sentido (ejemplo: Ingreso + Ocio).

### 18.2 Cambios tecnicos

- Components/Frontend/MainWindow.xaml:
    - cmbTipo ahora dispara SelectionChanged para refrescar categorias.
    - cmbCategoria pasa a llenarse dinamicamente desde code-behind.
- Components/Backend/MainWindow.xaml.cs:
    - catalogos de categorias por tipo (ingreso/gasto).
    - nuevo metodo ActualizarCategoriasPorTipo().
    - nuevo metodo EsCategoriaValidaParaTipo(tipo, categoria).
    - validacion defensiva en btnGuardarTransaccion_Click antes de insertar en BD.

### 18.3 Estado de validacion

- Sin errores de editor en los archivos modificados.
- Validacion de compilacion por diagnostico de editor: sin errores globales.

## 19) Avance Implementado - Coherencia en Filtros (2026-04-20)

### 19.1 Mejora funcional aplicada

- La barra de filtros de transacciones ahora mantiene coherencia entre tipo y categoria.
- Si el filtro de tipo es Ingreso, solo se muestran categorias de ingreso.
- Si el filtro de tipo es Gasto, solo se muestran categorias de gasto.
- Si el filtro de tipo es Todos, se muestran todas las categorias disponibles.

### 19.2 Cambios tecnicos

- Components/Frontend/MainWindow.xaml:
    - cmbFiltroTipo ahora usa SelectionChanged para refrescar categorias de filtro.
    - cmbFiltroCategoria pasa a poblarse dinamicamente desde code-behind.
- Components/Backend/MainWindow.xaml.cs:
    - nuevo metodo ActualizarCategoriasFiltroPorTipo().
    - nuevo handler cmbFiltroTipo_SelectionChanged.
    - btnLimpiarFiltros_Click ajustado para reconstruir categorias antes de aplicar filtros.

### 19.3 Validacion tecnica

- dotnet build UNUM.sln correcto.
- 0 errores y 0 advertencias.

## 20) Avance Implementado - Persistencia de Filtros (2026-04-20)

### 20.1 Mejora funcional aplicada

- El estado de filtros de transacciones ahora se guarda y restaura automaticamente al abrir MainWindow.
- Persistencia por usuario autenticado (UsuarioId), incluyendo:
    - tipo
    - periodo
    - categoria
    - fecha desde
    - fecha hasta

### 20.2 Cambios tecnicos

- Components/Backend/MainWindow.xaml.cs:
    - nuevos metodos:
        - GuardarEstadoFiltros()
        - RestaurarEstadoFiltros()
        - ObtenerRutaEstadoFiltros()
        - LeerEstadoFiltros(...)
        - SeleccionarComboPorTexto(...)
    - nuevo modelo interno EstadoFiltros para serializacion.
    - persistencia JSON en LocalAppData/UNUM/filtros-mainwindow.json.
    - control de restauracion con bandera \_restaurandoEstadoFiltros para evitar escrituras recursivas.

### 20.3 Validacion tecnica

- dotnet build UNUM.sln correcto tras cambios.
- 0 errores y 0 advertencias.

## 21) Avance Implementado - Chips de Filtros Activos (2026-04-20)

### 21.1 Mejora funcional aplicada

- Se anadio una visualizacion tipo "chips" con los filtros activos encima de la tabla de transacciones.
- Los chips muestran en tiempo real:
    - tipo (si no es "Todos")
    - periodo (si no es "Historico")
    - categoria (si no es "Todas")
    - fechas desde/hasta cuando estan informadas
- Si no hay filtros activos, se muestra el estado: "Sin filtros (historico completo)".

### 21.2 Cambios tecnicos

- Components/Frontend/MainWindow.xaml:
    - nueva franja visual "Filtros activos" con WrapPanel (panelFiltrosActivos).
    - reajuste de filas para insertar la zona de chips entre filtros y DataGrid.
- Components/Backend/MainWindow.xaml.cs:
    - nuevos metodos:
        - ActualizarIndicadoresFiltrosActivos()
        - CrearChip(...)
    - llamada a refresco de chips dentro de AplicarFiltrosTransacciones().

### 21.3 Validacion tecnica

- dotnet build UNUM.sln correcto.
- 0 errores y 0 advertencias.

## 22) Avance Implementado - Contador de Resultados (2026-04-20)

### 22.1 Mejora funcional aplicada

- Se anadio un contador visual junto a los chips de filtros activos con formato:
    - N de M transacciones
- El valor se actualiza automaticamente al aplicar/limpiar filtros y al recargar historial.

### 22.2 Cambios tecnicos

- Components/Frontend/MainWindow.xaml:
    - nuevo TextBlock txtResumenResultados en la cabecera de "Filtros activos".
- Components/Backend/MainWindow.xaml.cs:
    - ActualizarIndicadoresFiltrosActivos() ahora calcula:
        - total = filas del DataTable base
        - visibles = filas del DataView filtrado
    - actualiza txtResumenResultados con ambos valores.

### 22.3 Validacion tecnica

- dotnet build UNUM.sln correcto.
- 0 errores y 0 advertencias.

## 23) Avance Implementado - Utilidades Reales de Productividad (2026-04-20)

### 23.1 Mejora funcional aplicada

- Se amplio el panel de filtros con utilidades de uso diario:
    - busqueda libre por texto (descripcion, categoria o tipo)
    - rango de importes (minimo y maximo)
    - exportacion CSV de la vista filtrada actual

### 23.2 Cambios de UI

- Components/Frontend/MainWindow.xaml:
    - barra de filtros en 2 filas para mejorar capacidad sin perder claridad.
    - nuevos controles:
        - txtFiltroTexto
        - txtImporteMin
        - txtImporteMax
        - btnExportarCsv

### 23.3 Cambios de logica

- Components/Backend/MainWindow.xaml.cs:
    - filtros nuevos integrados en AplicarFiltrosTransacciones():
        - texto libre (LIKE sobre descripcion/categoria/tipo)
        - importe minimo y maximo con validaciones
    - soporte de parseo de decimales robusto por cultura:
        - TryParseImporteFiltro(...)
    - exportacion CSV:
        - btnExportarCsv_Click(...)
        - EscapeCsvValue(...)
    - escape de texto para RowFilter:
        - EscapeRowFilterLikeValue(...)
    - persistencia de nuevos filtros en EstadoFiltros:
        - TextoLibre
        - ImporteMin
        - ImporteMax

### 23.4 Validacion tecnica

- dotnet build UNUM.sln correcto tras integrar el paquete de utilidades.
- 0 errores y 0 advertencias.

## 24) Incidencia Resuelta - NullReference tras login exitoso (2026-04-20)

### 24.1 Sintoma

- El usuario autenticaba correctamente ("Login exitoso") y acto seguido aparecia:
    - Object reference not set to an instance of an object.

### 24.2 Causa probable

- Durante la inicializacion de MainWindow, algunos SelectionChanged podian dispararse antes de que todos los controles de filtros estuvieran listos.
- En ese estado, metodos de refresco de categorias/filtros podian tocar controles aun no inicializados.

### 24.3 Correccion aplicada

- Components/Backend/MainWindow.xaml.cs:
    - guards defensivos en:
        - ActualizarCategoriasPorTipo()
        - ActualizarCategoriasFiltroPorTipo()
        - RestaurarEstadoFiltros()
    - si controles clave aun son null, se sale de forma segura.

### 24.4 Estado

- Diagnostico de editor sin errores.
- Build con app abierta arrojo bloqueos de archivo (MSB3026) por proceso UNUM.exe en uso, no errores de codigo.

### 24.5 Mejora de diagnostico aplicada

- LoginWindow ahora separa:
    - errores de autenticacion/BD

## 25) Avance Implementado - Servicio de Dialogos para MVVM (2026-05-05)

### 25.1 Objetivo

- Reducir acoplamiento del `MainWindowViewModel` con clases de UI concretas (ventanas/modales).

### 25.2 Cambios tecnicos

- Se incorporo abstraccion de dialogos:
    - `Services/IWindowDialogService.cs`
    - `Services/WindowDialogService.cs`
- `MainWindowViewModel` ahora recibe `IWindowDialogService` por constructor.
- El comando `NuevoObjetivoCommand` ya no instancia `ObjetivoModalWindow` directamente; delega en el servicio.
- `MainWindow.xaml.cs` inyecta `WindowDialogService` al crear el ViewModel.

### 25.3 Limpieza relacionada

- Se mantiene la ventana principal mas enfocada en composicion (DataContext + animaciones + navegacion de paneles), con menos logica de detalle en ViewModel sobre UI concreta.

### 25.4 Validacion tecnica

- `dotnet build UNUM.sln` correcto tras la refactorizacion.
- Resultado: 0 errores, 0 advertencias.
    - errores al abrir MainWindow tras login exitoso
- Si falla la apertura del panel principal, se muestra mensaje especifico:
    - "Error al abrir el panel principal".

## 25) Ajuste UX - Ventana y Filtros Clean (2026-04-20)

### 25.1 Mejora visual aplicada

- MainWindow ahora abre mas grande por defecto para evitar arrastre horizontal:
    - Width: 1320
    - Height: 820
    - MinWidth: 1180
    - MinHeight: 760

### 25.2 Rediseño de barra de filtros

- El layout de filtros se optimizo con una estructura mas limpia (2 filas en WrapPanel):
    - fila 1: tipo, periodo, categoria, desde, hasta
    - fila 2: buscar, min/max importe, aplicar, limpiar, exportar CSV
- Se priorizo legibilidad y reduccion de densidad visual.

### 25.3 Validacion tecnica

- dotnet build UNUM.sln correcto tras los cambios.
- 0 errores y 0 advertencias.

## 26) Estado de Objetivos vs Gastos/Ingresos (2026-04-20)

### 26.1 Lo que SI esta conectado

- Objetivos y transacciones comparten el mismo usuario autenticado (UsuarioId).
- Se pueden gestionar en la misma sesion/flujo de la app.

### 26.2 Lo que NO esta conectado aun (a nivel de negocio)

- No existe regla automatica que al registrar ingresos/gastos actualice AhorroActual de objetivos.
- El progreso de objetivos se actualiza manualmente desde la ventana de objetivos.

## 27) Continuacion MVVM - Cierre de Sesion por Command (2026-05-05)

### 27.1 Objetivo

- Reducir responsabilidad de `MainWindow.xaml.cs` moviendo cierre de sesion a comando del ViewModel.

### 27.2 Cambios tecnicos

- `MainWindow.xaml`:
    - `btnCerrarSesion` paso de `Click` a `Command` (`CerrarSesionCommand`).
- `MainWindowViewModel`:
    - nuevo comando `CerrarSesionCommand`.
    - nuevo evento `RequestClose` para solicitar cierre de la ventana host.
    - `CerrarSesion()` ahora abre Inicio via servicio y solicita cierre por evento.
- `IWindowDialogService` / `WindowDialogService`:
    - nuevo metodo `ShowInicioWindow()`.
- `MainWindow.xaml.cs`:
    - se suscribe a `RequestClose` para ejecutar `Close()`.
    - se elimina `btnCerrarSesion_Click`.

### 27.3 Resultado

- Menor acoplamiento del code-behind con flujo de sesion.
- Navegacion y dialogos gestionados por servicios y comandos MVVM.

### 27.4 Validacion tecnica

- `dotnet build UNUM.sln` correcto.
- Resultado: 0 errores, 0 advertencias.

## 28) Continuacion MVVM - Navegacion de Paneles por Command (2026-05-05)

### 28.1 Objetivo

- Pasar la navegacion entre `Transacciones` y `Simulador` a comandos del ViewModel, manteniendo en code-behind solo la parte visual/animacion.

### 28.2 Cambios tecnicos

- `MainWindowViewModel`:
    - nuevo enum `MainPanelType` (`Transacciones`, `Simulador`).
    - nuevo evento `RequestPanelNavigation`.
    - nuevos comandos:
        - `MostrarTransaccionesCommand`
        - `MostrarSimuladorCommand`
- `MainWindow.xaml`:
    - `btnMenuTransacciones` migra de `Click` a `Command`.
    - `btnMenuSimulador` migra de `Click` a `Command`.
- `MainWindow.xaml.cs`:
    - suscripcion a `RequestPanelNavigation`.
    - nuevo metodo `NavigateToPanel(MainPanelType panel)` para ejecutar animaciones y estilos activos.
    - eliminados handlers `btnMenuTransacciones_Click` y `btnMenuSimulador_Click`.

### 28.3 Resultado

- El origen de la intencion de navegacion queda en ViewModel (comandos MVVM).
- El code-behind conserva solo logica de transicion visual (animacion/estilo).

### 28.4 Validacion tecnica

- `dotnet build UNUM.sln` correcto tras la migracion.
- Resultado: 0 errores, 0 advertencias.

## 29) Continuacion MVVM - Estado Visual de Menu por Binding (2026-05-05)

### 29.1 Objetivo

- Quitar del code-behind el control manual de color activo del menu lateral y moverlo a estado bindable del ViewModel.

### 29.2 Cambios tecnicos

- `MainWindowViewModel`:
    - nuevas propiedades bindables:
        - `MenuTransaccionesActivo`
        - `MenuSimuladorActivo`
    - los comandos de navegacion (`MostrarTransaccionesCommand`, `MostrarSimuladorCommand`) ahora actualizan estas propiedades antes de disparar `RequestPanelNavigation`.
- `ValueConverters`:
    - nuevo converter `MenuActiveBackgroundConverter` para mapear bool -> brush de menu activo/inactivo.
- `MainWindow.xaml`:
    - `Background` de `btnMenuTransacciones` y `btnMenuSimulador` enlazado por binding con `MenuActiveBackgroundConverter`.
- `MainWindow.xaml.cs`:
    - `NavigateToPanel(...)` conserva solo animacion/transicion de paneles.
    - eliminada la asignacion manual de colores de botones.

### 29.3 Resultado

- El estado visual del menu queda centralizado en ViewModel.
- Code-behind mas delgado y limitado a comportamiento visual no bindable (animaciones).

### 29.4 Validacion tecnica

- `dotnet build UNUM.sln` correcto tras el ajuste.
- Resultado: 0 errores, 0 advertencias.
