# 🌿 **Faeterna**  
### *Un metroidvania de acción y exploración sobre la caída de una diosa que se niega a abandonar a la humanidad.*

<div align="center">

![Status](https://img.shields.io/badge/Estado-En%20desarrollo-yellow)
![Engine](https://img.shields.io/badge/Motor-Godot%204-blue)
![License](https://img.shields.io/badge/Licencia-MIT-green)
![Contributions](https://img.shields.io/badge/Contribuciones-Abiertas-brightgreen)
![Version](https://img.shields.io/badge/Versión-Pre--Alpha-orange)

</div>

---

## 🌌 Sinopsis

**Faeterna** cuenta la historia de **Lira**, una antigua diosa que se niega a permitir que la humanidad sea borrada de la existencia.  
Por desobedecer el mandato del cielo, es expulsada y cae al mundo mortal, donde adopta un nuevo nombre y deberá recuperar su poder para proteger aquello que juró defender.

Explora un mundo mágico consumido por la decadencia, enfréntate a criaturas corrompidas y desbloquea nuevas habilidades mientras descubres los restos de una civilización en lucha por sobrevivir.

---

## 🎮 Características Principales

- 🏹 Metroidvania con progresión basada en exploración.  
- 🌳 Árbol de habilidades que desbloquea:
  - Correr  
  - Dash  
  - Magia  
  - Doble salto  
  - Salto en pared  
  - Gancho  
  - Ataque a distancia  
  - Ataque cargado  
- 💎 Sistema de recursos (naturales + magia).  
- ⚔️ Sistema de riesgo–recompensa: muerte = perder recursos durante 30 minutos.  
- 🏘️ Pueblos conectados mediante almacenamiento compartido que crece al mejorar el Árbol.  

---

## 📁 Estructura del Proyecto
/assets/               → Sprites, tilesets, efectos, UI...
/scenes/               → Escenas del juego (.tscn) y niveles.
/script/               → Scripts GDScript/C#.
/.vscode/              → Configuración opcional del editor.
.gitattributes         → Normalización de archivos.
/.gitignore            → Archivos ignorados por Git.
Faeterna.csproj        → Configuración del proyecto C# (si existe).
Faeterna.sln           → Solución del proyecto.
/bosqueTutorial.tscn   → Escena tutorial.
/icon.svg              → Icono del proyecto.
project.godot          → Archivo principal de Godot.
LICENSE                → Licencia MIT.
README.md              → Documento actual.


---

## 🌱 Sistema de Recursos

### Recursos disponibles
- 🌿 Naturales (plantas, madera, minerales…)  
- ✨ Mágicos

### Cómo se obtienen
- Derrotando enemigos  
- Recolectando nodos  
- Producidos automáticamente por los pueblos  

### Uso principal
- Mejorar el Árbol de Habilidades  
- Comercio con NPCs  
- Mejora de pueblos y futuros sistemas  

### Muerte
- Pierdes todos tus recursos actuales  
- Tienes **30 minutos** para recuperarlos  

### Almacenamiento
Cada pueblo añade una base al almacenamiento global.  
Su capacidad aumenta con mejoras del Árbol.

---

## 🌳 Árbol de Habilidades

| Habilidad | Descripción |
|----------|-------------|
| **Correr** | Incrementa la velocidad de movimiento un 20%. |
| **Dash** | Impulso rápido horizontal con 1s de cooldown. |
| **Magia** | Desbloquea ataques mágicos básicos y el uso de fuentes mágicas. |
| **Doble salto** | Permite un segundo salto en el aire. |
| **Salto en pared** | Permite rebotar entre paredes. |
| **Gancho** | Permite agarrarse a puntos especiales del escenario. |
| **Ataque a distancia** | Dispara proyectiles a media distancia. |
| **Ataque cargado** | Ataque más poderoso tras mantener el botón. |

---

## 🛠️ Tecnologías

- Godot 4.x  
- GDScript / C#  
- Sistema de animación 2D  
- Tilesets y colisiones personalizadas  

---

## 🤝 Contribuir

1. Haz un fork del repositorio  
2. Crea una rama nueva  
3. Realiza tus cambios  
4. Abre un Pull Request  

---

## 📜 Licencia

Este proyecto se distribuye bajo la **Licencia MIT**.  
Puedes usarlo, modificarlo y compartirlo libremente con atribución.

---

## 🌟 Créditos

- Protagonista: **Lira**  
- Desarrollo: Proyecto Faeterna  
- Motor: Godot Engine  

## 👥 Colaboradores

<div align="center">

<table>
  <tr>
    <td align="center" width="200">
      <a href="https://github.com/BaenaArnau">
        <img src="https://github.com/BaenaArnau.png" width="100px" style="border-radius:50%;">
        <br />
        <sub><b>Arnau Baena Perez</b></sub>
      </a>
    </td>
    <td align="center" width="200">
      <a href="https://github.com/MrWezh">
        <img src="https://github.com/MrWezh.png" width="100px" style="border-radius:50%;">
        <br />
        <sub><b>Wenkai Zhou</b></sub>
      </a>
    </td>
  </tr>
  <tr>
    <td align="center" width="200">
      <a href="https://github.com/HaolinZheng">
        <img src="https://github.com/HaolinZheng.png" width="100px" style="border-radius:50%;">
        <br />
        <sub><b>Haolin Zheng</b></sub>
      </a>
    </td>
    <td align="center" width="200">
      <a href="https://github.com/SrG640">
        <img src="https://github.com/SrG640.png" width="100px" style="border-radius:50%;">
        <br />
        <sub><b>Sergi Flores Varo</b></sub>
      </a>
    </td>
  </tr>
</table>

</div>

