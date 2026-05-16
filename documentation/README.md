# Documentación Faeterna

Bienvenido a la documentación generada por **Doxygen** para el proyecto **Faeterna**.

## Contenidos

- **[Documentación HTML](html/index.html)** - Documentación interactiva completa con:
  - Diagramas de clases y jerarquía
  - Diagramas de dependencias
  - Gráficos de llamadas a funciones
  - Listado completo de clases, métodos y propiedades
  - Búsqueda integrada
  - Ejemplos de uso

## Estructura del Proyecto

El proyecto Faeterna está organizado en los siguientes módulos:

### Personaje (`Scripts/Personaje`)
- **Lira.cs** - Clase principal del jugador
- **LiraAnimationTree.cs** - Sistema de animaciones del jugador
- **Shot.cs** - Proyectiles disparados por el jugador
- **MaquinasDeEstados/** - Sistema de máquina de estados para movimiento

### Enemigos (`Scripts/Enemigos`)
- **Enemy.cs** - Clase base para todos los enemigos
- **Jabali/** - Enemigo jabalí con ataque acelerado
- **Slime/** - Enemigo que salta periódicamente
- **Wolf/** - Enemigo lobo
- **ReyJabali/** - Boss jabalí

### Mapas (`Scripts/Mapa`)
- **Bosque.cs** - Escena del bosque
- **Objeto.cs** - Objetos interactivos del mapa
- **Portal.cs** - Portales de transición
- **BossArea.cs** - Áreas de jefes

### Menús (`Scripts/Menus`)
- **MainMenu.cs** - Menú principal
- **PlayMenu.cs** - Menú de selección de juego
- **OptionsMenu.cs** - Menú de opciones
- **PauseMenu.cs** - Menú de pausa
- **DeathScreen.cs** - Pantalla de muerte
- **ObjetoDescription.cs** - Descripción de objetos

### Herramientas (`Scripts/Tools`)
- **GameSaveData.cs** - Estructuras de datos de guardado
- **GameSaveService.cs** - Servicio de persistencia
- **Saves.cs** - Sistema de guardado y carga
- **CursorManager.cs** - Gestor de cursor
- **CheckPoint.cs** - Sistema de puntos de control

### Tutorial (`Scripts/Tutorial`)
- **Turotial.cs** - Sistema de tutorial del juego

## Generación de Documentación

Esta documentación se generó usando **Doxygen 1.16.1** a partir del código fuente C# del proyecto.

Para regenerar la documentación:

```bash
cd /mnt/Cosas/Proyectos/Godot/C#/Faeterna
doxygen Doxyfile
```

### Requisitos
- **Doxygen** 1.16.1+
- **Graphviz** (para generar diagramas)
- Compilador de C# (.NET 8.0+)

## Navegación Rápida

- [Listado de clases](html/annotated.html)
- [Jerarquía de clases](html/hierarchy.html)
- [Diagramas de colaboración](html/graph_legend.html)
- [Índice de ficheros](html/files.html)
- [Búsqueda](html/search.html)

## Información del Proyecto

- **Nombre:** Faeterna
- **Versión:** 1.0.0
- **Motor:** Godot 4.x con C#
- **Tipo:** Videojuego de plataformas 2D

---

Para más información sobre el proyecto, consulta el `README.md` en la raíz del proyecto.

