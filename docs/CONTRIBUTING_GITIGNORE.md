# 🚫 Guía de Desarrollo - Lo que NO debe subirse a GitHub

Este documento explica qué archivos y carpetas **nunca deben** subirse al repositorio y por qué.

## 📋 Índice
- [Carpetas y archivos ignorados](#carpetas-y-archivos-ignorados)
- [Por qué ignoramos estos archivos](#por-qué-ignoramos-estos-archivos)
- [Cómo asegurar que git ignore funciona](#cómo-asegurar-que-git-ignore-funciona)
- [Ayuda si accidentalmente subiste archivos](#ayuda-si-accidentalmente-subiste-archivos)

---

## Carpetas y archivos ignorados

### 🎮 **Godot Específico**

```
.godot/              # Cache de Godot 4
.import/             # Archivos de importación generados
mono_assemblies/     # Assemblies compilados
export_presets.cfg   # Configuración de export del proyecto
*.tscn~              # Backups de escenas
*.tres~              # Backups de recursos
```

**Por qué:** Se generan automáticamente al abrir el proyecto en Godot.

---

### 💾 **C# / .NET**

```
bin/                 # Archivos compilados (.dll, .exe)
obj/                 # Objetos intermedios de compilación
packages/            # Paquetes NuGet descargados
*.pdb                # Archivos de debug
*.nupkg              # Paquetes NuGet
```

**Por qué:** Se generan durante la compilación y pueden causar conflictos entre sistemas operativos.

---

### 🔧 **IDE & Editores**

```
.vs/                 # Carpeta de Visual Studio
.idea/               # Carpeta de JetBrains Rider
.vscode/             # Configuración personal de VSCode
*.user               # Archivos de usuario de Visual Studio
*.userprefs          # Preferencias de usuario
*.suo                # Solución de Visual Studio
```

**Por qué:** Contienen configuraciones personales de cada desarrollador. Cada uno puede tener sus preferencias distintas.

---

### 🖥️ **Archivos del Sistema Operativo**

```
.DS_Store            # macOS folder metadata
Thumbs.db            # Windows folder metadata
.directory           # Linux folder metadata
*.lnk                # Windows shortcuts
$RECYCLE.BIN/        # Papelera de Windows
ehthumbs.db          # Thumbnails de Windows
```

**Por qué:** Se crean automáticamente por el SO y no son relevantes para el código.

---

### 📝 **Local Development**

```
.env                 # Variables de entorno
.env.local           # Variables locales
logs/                # Archivos de log
*.log                # Registros
tmp/                 # Archivos temporales
temp/                # Archivos de temporal
```

**Por qué:** Pueden contener información sensible o ser específicos de tu máquina.

---

### 🧪 **Testing & Debugging**

```
test_results/        # Resultados de pruebas
debug_output/        # Output de debugging
*.debug              # Archivos de debug
coverage/            # Reporte de coverage
```

**Por qué:** Son específicos de cada sesión de desarrollo/testing.

---

### 🎮 **Datos de Juego**

```
user://              # Datos de usuario en runtime
crashes/             # Crashlogs automáticos
save_data/           # Datos guardados del juego
*.save               # Archivos de guardado
```

**Por qué:** Se generan durante la ejecución del juego.

---

## Por qué ignoramos estos archivos

### ✅ **Beneficios de un buen `.gitignore`**

1. **Mantiene el repo limpio**
   - Solo se trackean archivos esenciales
   - El historio es más legible

2. **Evita conflictos**
   - Archivos generados no causen merges conflictivos
   - Cada dev puede tener su configuración

3. **Reduce tamaño**
   - Los binarios compilados son muy pesados
   - Los archivos de cache pueden ser gigantescos

4. **Seguridad**
   - Las credenciales/envs nunca se exponen accidentalmente
   - Información sensible se queda local

5. **Rendimiento**
   - Git trabaja más rápido con menos archivos
   - Los clones/pushes/pulls son más velóces

---

## Cómo asegurar que git ignore funciona

### ✔️ **Verificar que está configurado correctamente**

```bash
# Ver qué archivos Git tiene tracked
git ls-files

# Ver qué archivos fueron ignorados por .gitignore
git check-ignore -v <archivo>

# Hacer dry-run de un add para ver qué se añadiría
git add --dry-run .
```

### ✔️ **Antes de hacer commit**

```bash
# Ver qué archivos van a subirse
git status

# Asegúrate de que NO ves:
# - bin/ obj/
# - .vs/ .idea/ .vscode/
# - Archivos de SO (.DS_Store, Thumbs.db)
# - Archivos *.log
# - La carpeta .godot/
```

---

## Ayuda si accidentalmente subiste archivos

### 🔧 **Si ya hiciste commit (pero no push)**

```bash
# Deshacer el último commit sin perder cambios
git reset --soft HEAD~1

# Luego, reintentar solo con los archivos correctos
git reset
git add <tus_archivos_correctos>
git commit -m "Tu mensaje"
```

### 🔧 **Si ya hiciste push**

```bash
# Remover un archivo del historial (más complejo)
git rm --cached <archivo>
git commit --amend
git push --force-with-lease
```

⚠️ **Advertencia:** `--force-with-lease` reescribe historial. Usa con cuidado en repos compartidos.

### 🔧 **Si quieres ignorar un archivo ya tracked**

```bash
# Remover del tracking pero mantener en disco
git rm --cached <archivo>

# Luego, añadir al .gitignore
echo "<archivo>" >> .gitignore

# Commit
git add .gitignore
git commit -m "Stop tracking <archivo>"
```

---

## 📚 Archivos que SÍ deben subirse

✅ **Siempre incluye:**
- `scripts/` → Todos los archivos .cs y .gd
- `scenes/` → Todas las escenas .tscn
- `assets/` → Sprites, audio, etc.
- `Faeterna.csproj` → Configuración del proyecto
- `Faeterna.sln` → Solución de Visual Studio
- `project.godot` → Configuración de Godot
- `README.md` → Documentación
- `.gitignore` → Este archivo de configuración
- `.gitattributes` → Normalización de líneas
- `.editorconfig` → Consistencia de código

---

## 🤔 ¿Necesitas agregar más excepciones?

Si encuentras archivos que **necesitas** que se trackeen pero que normalmente se ignoran:

```bash
# Puedes usar ! para hacer excepciones en .gitignore
# Ejemplo:
logs/                # Ignora la carpeta logs
!logs/README.md      # EXCEPTO este archivo
```

---

## 💡 Tips finales

- **Configura Global:** Considera un `.gitignore` global en tu máquina
- **Actualiza siempre:** Si encuentras archivos que deberían ignorarse, actualiza el `.gitignore`
- **Comunica:** Si algo debería ignorarse, menciónalo en el PR
- **Usa .gitattributes:** Ya está configurado para normalizar line endings

---

**¿Preguntas?** Consulta la [documentación oficial de git](https://git-scm.com/docs/gitignore)
