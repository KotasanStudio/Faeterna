# 🔒 Política de Seguridad — Faeterna

## Versiones soportadas

Solo la versión activa en desarrollo recibe actualizaciones de seguridad.

| Versión    | Soporte de seguridad |
|------------|----------------------|
| Pre-Alpha (rama `main`) | ✅ Activa |
| Versiones antiguas      | ❌ Sin soporte |

---

## 📣 Cómo reportar una vulnerabilidad

Si descubres una vulnerabilidad de seguridad en este proyecto, **no abras un Issue público**. En su lugar, repórtala de forma responsable siguiendo estos pasos:

1. **GitHub Private Vulnerability Reporting**  
   Usa la función de [reporte privado de vulnerabilidades de GitHub](https://docs.github.com/es/code-security/security-advisories/guidance-on-reporting-and-writing/privately-reporting-a-security-vulnerability) directamente en este repositorio:  
   `Security → Report a vulnerability`

2. **Información a incluir en el reporte:**
   - Descripción clara de la vulnerabilidad.
   - Pasos detallados para reproducirla.
   - Impacto potencial estimado.
   - Versión o commit afectado.
   - Cualquier propuesta de mitigación o corrección (opcional).

---

## ⏱️ Tiempo de respuesta

| Acción                         | Plazo estimado |
|--------------------------------|----------------|
| Acuse de recibo del reporte    | 3 días hábiles  |
| Evaluación inicial             | 7 días hábiles  |
| Corrección y publicación del parche | Según criticidad (ver abajo) |

### Prioridad por criticidad

| Nivel       | Tiempo de corrección objetivo |
|-------------|-------------------------------|
| Crítica     | ≤ 7 días                      |
| Alta        | ≤ 14 días                     |
| Media       | ≤ 30 días                     |
| Baja        | En la próxima versión          |

---

## 🔍 Alcance de la política

### ✅ En alcance

- Vulnerabilidades en dependencias del proyecto (NuGet, addons de Godot).
- Manipulación maliciosa de archivos de guardado (`SaveFileBuilder`).
- Ejecución de código arbitrario a través de activos o escenas del juego.
- Exposición de datos sensibles del jugador.
- Vulnerabilidades en scripts C# o GDScript que puedan afectar la integridad del juego.

### ❌ Fuera de alcance

- Bugs de jugabilidad sin impacto en seguridad.
- Problemas de rendimiento o estabilidad que no sean explotables.
- Vulnerabilidades en herramientas externas no mantenidas por este proyecto (Godot Engine, .NET SDK).
- Ingeniería inversa del juego con fines de trampa (*cheating*) en modo de un solo jugador.

---

## 🛡️ Buenas prácticas del equipo

- Todas las dependencias NuGet se mantienen actualizadas y se revisan periódicamente.
- Se utiliza `.gitignore` para evitar incluir credenciales o datos sensibles en el repositorio.
- Los archivos de guardado se gestionan con [`Chickensoft.SaveFileBuilder`](https://github.com/chickensoft-games/SaveFileBuilder) para garantizar integridad estructural.

---

## 📜 Reconocimientos

Agradecemos a quienes reporten vulnerabilidades de forma responsable y contribuyan a mejorar la seguridad del proyecto. Su colaboración ayuda a proteger a toda la comunidad.

---

*Esta política está basada en las recomendaciones de [GitHub Security Advisories](https://docs.github.com/es/code-security/security-advisories).*
