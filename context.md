# UNUM - Contexto Vivo del Proyecto

## Objetivo
Aplicacion de escritorio WPF (.NET 8) para finanzas personales con autenticacion, transacciones, objetivos, presupuestos y una guia de uso clara.

## Estado actual validado
- Flujo principal operativo: Inicio -> Login/Register -> MainWindow.
- MainWindow concentra el trabajo diario: altas y edicion de transacciones, filtros, resumen financiero, analitica mensual, presupuestos y objetivos.
- El tutorial inicial existe y se puede reabrir desde el menu lateral.
- El onboarding interactivo ya no es una ventana aislada: acompana al dashboard y lleva la vista a la seccion correcta.
- El saldo actual se pinta en rojo cuando es negativo.
- La solucion compila correctamente tras los ultimos cambios.

## Arquitectura actual
- `Infrastructure/DbConnectionFactory.cs` centraliza la creacion de conexiones.
- `Services/AuthService.cs`, `TransactionService.cs`, `ObjectiveService.cs`, `WindowDialogService.cs` y `OnboardingService.cs` cubren la logica principal.
- `ViewModels/MainWindowViewModel.cs` concentra la logica del dashboard y expone comandos/bindings.
- `Security/PasswordHasher.cs` usa BCrypt; si existe un usuario legado en texto plano, se migra al primer login valido.

## UI y funcionalidad destacada
- Transacciones: crear, editar, borrar, exportar y filtrar.
- Filtros: vista mas limpia con filtros avanzados desplegables.
- Presupuestos: alta, edicion inline, eliminacion individual y cancelacion de edicion.
- Objetivos: modal reutilizable para alta/edicion, progreso y borrado por objetivo.
- Dashboard: resumen financiero, analitica mensual, proyeccion simple y badges de riesgo.

## Configuracion y base de datos
- `App.config` mantiene `UNUM_DB` como configuracion base.
- `DbConnectionFactory` tambien acepta overrides por variables de entorno.
- Se eliminaron ventanas viejas que ya no aportaban al flujo real: `DbConfigWindow`, `ObjetivosWindow` y `TimerWindow`.

## Limpieza reciente
- Se quito el checkbox de "no volver a mostrar" del intro inicial.
- El tutorial se actualizo para cubrir funciones nuevas y llevar al usuario a la zona correcta sin obligarlo a scrollear.
- Se hizo una pasada de limpieza para eliminar restos obsoletos del proyecto.

## Pendiente natural
- Seguir puliendo la seccion de objetivos y pequenos detalles visuales.
