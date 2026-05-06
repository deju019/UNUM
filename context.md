# UNUM - Contexto Vivo del Proyecto

## Objetivo
Aplicacion de escritorio WPF (.NET 8) para finanzas personales con autenticacion, transacciones, objetivos, presupuestos y una guia de uso clara.

## Estado actual validado
**Tutorial mejorado (v3)**: posicionamiento al lado (derecha o izquierda discretamente) del elemento que referencia, sin tapar nada. Se intenta poner a la derecha; si no hay espacio, va a la izquierda. Alineado verticalmente. No interfiere con diálogos modales.
**Logo UNUM**: agregado en el sidebar con diseño visual (círculo dorado + arco) junto a texto "UNUM / Finanzas".
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
